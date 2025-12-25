using FFXIVClientStructs.FFXIV.Client.Game;
using JobBars.Data;
using JobBars.Helper;
using JobBars.Nodes.Cooldown;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobBars.Cooldowns
{
    public class CooldownTracker
    {
        public enum TrackerState
        {
            None,
            Running,
            OnCD,
            OffCD
        }

        private readonly CooldownConfig _CooldownConfig;

        private TrackerState State = TrackerState.None;
        private DateTime LastActiveTime;
        private Item LastActiveTrigger;
        private float TimeLeft;
        private int CurrentCharges = 0; // 当前充能次数

        public TrackerState CurrentState => State;
        public ActionIds Icon => _CooldownConfig.Icon;

        public CooldownTracker( CooldownConfig cooldownConfig )
        {
            _CooldownConfig = cooldownConfig;
        }

        public static List< uint > AdjustedActionList = new()
        {
            ( uint )ActionIds.预警,
            ( uint )ActionIds.圣盾阵,
            ( uint )ActionIds.星云,
            ( uint )ActionIds.暗影墙,
            ( uint )ActionIds.石之心,
            ( uint )ActionIds.自生
        };

        public void ProcessAction( Item action )
        {
            foreach( var configTrigger in _CooldownConfig.Triggers )
            {
                var trigger = configTrigger;
                if( AdjustedActionList.Contains( trigger.Id ) )
                {
                    var adjustedAction = UiHelper.GetAdjustedAction( trigger.Id );
                    if( adjustedAction == action.Id )
                    {
                        SetActive( action );
                    }
                }
                else
                {
                    if( trigger.Id == action.Id )
                    {
                        SetActive( action );
                    }
                }
            }
        }

        private void SetActive( Item trigger )
        {
            State = _CooldownConfig.Duration == 0 ? TrackerState.OnCD : TrackerState.Running;
            LastActiveTime = DateTime.Now;
            LastActiveTrigger = trigger;
        }

        public void Tick( Dictionary< Item, Status > buffDict )
        {
            // 计算充能次数（如果技能有充能）
            if( _CooldownConfig.MaxCharges > 1 )
            {
                var calculateCurrentCharges = CalculateCurrentCharges();
                CurrentCharges = calculateCurrentCharges;
            }
            else
            {
                CurrentCharges = 0; // 非充能技能，不使用充能计数
            }

            if( State != TrackerState.Running &&
                UiHelper.CheckForTriggers( buffDict, _CooldownConfig.Triggers, out var trigger ) ) SetActive( trigger );

            if( State == TrackerState.Running )
            {
                TimeLeft = UiHelper.TimeLeft(
                    JobBars.Configuration.CooldownsHideActiveBuffDuration ? 0 : _CooldownConfig.Duration, buffDict,
                    LastActiveTrigger, LastActiveTime );
                if( TimeLeft <= 0 )
                {
                    TimeLeft = 0;
                    State = TrackerState.OnCD; // mitigation needs to have a CD
                }
            }
            else if( State == TrackerState.OnCD )
            {
                // 对于充能技能，使用实时计算的充能次数来判断状态
                if( _CooldownConfig.MaxCharges > 1 )
                {
                    if( CurrentCharges >= _CooldownConfig.MaxCharges )
                    {
                        // 充能已满，切换到 OffCD 状态
                        State = TrackerState.OffCD;
                        TimeLeft = 0;
                    }
                    else
                    {
                        // 充能未满，保持在 OnCD 状态，计算到下一次充能完成的时间
                        var recastActive = false;
                        float timeElapsed = 0;
                        foreach( var triggerItem in _CooldownConfig.Triggers )
                        {
                            if( triggerItem.Type != ItemType.Buff )
                            {
                                recastActive = UiHelper.GetRecastActive( triggerItem.Id, out timeElapsed );
                                if( recastActive ) break;
                            }
                        }

                        if( recastActive && _CooldownConfig.CD > 0 )
                        {
                            // 计算到下一次充能完成的时间
                            var timeUntilNextCharge = _CooldownConfig.CD - ( timeElapsed % _CooldownConfig.CD );
                            TimeLeft = timeUntilNextCharge;
                        }
                        else
                        {
                            TimeLeft = _CooldownConfig.CD;
                        }
                    }
                }
                else
                {
                    // 非充能技能，使用原来的逻辑
                    TimeLeft = ( float )( _CooldownConfig.CD - ( DateTime.Now - LastActiveTime ).TotalSeconds );

                    if( TimeLeft <= 0 )
                    {
                        State = TrackerState.OffCD;
                    }
                }
            }
            else if( State == TrackerState.OffCD && _CooldownConfig.MaxCharges > 1 )
            {
                // 对于充能技能，检查充能状态
                if( CurrentCharges <= 0 )
                {
                    // 所有充能都用完，切换到 OnCD 状态
                    State = TrackerState.OnCD;
                    // 计算剩余冷却时间
                    var recastActive = false;
                    float timeElapsed = 0;
                    foreach( var triggerItem in _CooldownConfig.Triggers )
                    {
                        if( triggerItem.Type != ItemType.Buff )
                        {
                            recastActive = UiHelper.GetRecastActive( triggerItem.Id, out timeElapsed );
                            if( recastActive ) break;
                        }
                    }

                    if( recastActive && _CooldownConfig.CD > 0 )
                    {
                        var timeUntilNextCharge = _CooldownConfig.CD - ( timeElapsed % _CooldownConfig.CD );
                        TimeLeft = timeUntilNextCharge;
                    }
                    else
                    {
                        TimeLeft = _CooldownConfig.CD;
                    }
                }
                else if( CurrentCharges < _CooldownConfig.MaxCharges )
                {
                    // 充能未满，计算到下一次充能完成的时间
                    var recastActive = false;
                    float timeElapsed = 0;
                    foreach( var triggerItem in _CooldownConfig.Triggers )
                    {
                        if( triggerItem.Type != ItemType.Buff )
                        {
                            recastActive = UiHelper.GetRecastActive( triggerItem.Id, out timeElapsed );
                            if( recastActive ) break;
                        }
                    }

                    if( recastActive && _CooldownConfig.CD > 0 )
                    {
                        var timeUntilNextCharge = _CooldownConfig.CD - ( timeElapsed % _CooldownConfig.CD );
                        TimeLeft = timeUntilNextCharge;
                    }
                    else
                    {
                        TimeLeft = 0;
                    }
                }
                else
                {
                    // 充能已满
                    TimeLeft = 0;
                }
            }
        }

        private int CalculateCurrentCharges()
        {
            // 查找第一个有效的动作触发器来计算充能
            foreach( var trigger in _CooldownConfig.Triggers )
            {
                // 只处理动作类型（Action, GCD, OGCD），不处理 Buff
                if( trigger.Type != ItemType.Buff )
                {
                    var recastActive = UiHelper.GetRecastActive( trigger.Id, out var timeElapsed );
                    if( recastActive && _CooldownConfig.CD > 0 )
                    {
                        // 参考 GaugeChargesTracker 的逻辑
                        var charges = ( int )Math.Floor( timeElapsed / _CooldownConfig.CD );
                        // 确保充能次数不超过最大充能
                        return Math.Min( charges, _CooldownConfig.MaxCharges );
                    }
                    else
                    {
                        // 不在冷却，充能已满
                        return _CooldownConfig.MaxCharges;
                    }
                }
            }

            // 如果没有找到有效的触发器，返回最大充能
            return _CooldownConfig.MaxCharges;
        }

        public void TickUi( CooldownNode node, float percent )
        {
            if( node == null ) 
                return;

            if( node?.IconId != _CooldownConfig.Icon )
                node.LoadIcon( _CooldownConfig.Icon );

            node.IsVisible = true;

            // 设置充能次数显示（右下角）
            if( _CooldownConfig.MaxCharges > 1 )
            {
                node.SetCharges( CurrentCharges, _CooldownConfig.MaxCharges );
            }
            else
            {
                node.SetCharges( 0, 0 ); // 隐藏充能显示
            }

            if( State == TrackerState.None )
            {
                node.SetOffCd();
                node.SetText( "" );
                node.SetNoDash();
            }
            else if( State == TrackerState.Running )
            {
                node.SetOffCd();
                node.SetText( ( ( int )Math.Round( TimeLeft ) ).ToString() );
                if( _CooldownConfig.ShowBorderWhenActive )
                {
                    node.SetDash( percent );
                }
                else
                {
                    node.SetNoDash();
                }
            }
            else if( State == TrackerState.OnCD )
            {
                // 对于充能技能，只有当充能次数为0时才应用冷却透明度
                if( _CooldownConfig.MaxCharges > 1 && CurrentCharges > 0 )
                {
                    node.SetOffCd(); // 有充能可用，不应用透明度
                }
                else
                {
                    node.SetOnCd(); // 充能用完或非充能技能，应用透明度
                }

                // 对于充能技能，如果充能未满，显示倒计时
                if( _CooldownConfig.MaxCharges > 1 && CurrentCharges < _CooldownConfig.MaxCharges )
                {
                    node.SetText( ( ( int )Math.Round( TimeLeft ) ).ToString() );
                }
                else if( _CooldownConfig.MaxCharges <= 1 )
                {
                    // 非充能技能，正常显示倒计时
                    node.SetText( ( ( int )Math.Round( TimeLeft ) ).ToString() );
                }
                else
                {
                    // 充能已满，不显示倒计时
                    node.SetText( "" );
                }

                node.SetNoDash();
            }
            else if( State == TrackerState.OffCD )
            {
                node.SetOffCd();
                // 对于充能技能，如果充能未满，显示倒计时
                if( _CooldownConfig.MaxCharges > 1 && CurrentCharges < _CooldownConfig.MaxCharges )
                {
                    // 计算到下一次充能完成的时间
                    var recastActive = false;
                    float timeElapsed = 0;
                    foreach( var triggerItem in _CooldownConfig.Triggers )
                    {
                        if( triggerItem.Type != ItemType.Buff )
                        {
                            recastActive = UiHelper.GetRecastActive( triggerItem.Id, out timeElapsed );
                            if( recastActive )
                                break;
                        }
                    }

                    if( recastActive && _CooldownConfig.CD > 0 )
                    {
                        var timeUntilNextCharge = _CooldownConfig.CD - ( timeElapsed % _CooldownConfig.CD );
                        node.SetText( ( ( int )Math.Round( timeUntilNextCharge ) ).ToString() );
                    }
                    else
                    {
                        node.SetText( "" );
                    }
                }
                else
                {
                    node.SetText( "" );
                }

                if( _CooldownConfig.ShowBorderWhenOffCD )
                    node.SetDash( percent );
                else node.SetNoDash();
            }
        }

        public void Reset()
        {
            State = TrackerState.None;
        }
    }
}