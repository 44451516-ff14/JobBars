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

        private readonly CooldownConfig Config;

        private TrackerState State = TrackerState.None;
        private DateTime LastActiveTime;
        private Item LastActiveTrigger;
        private float TimeLeft;
        private int CurrentCharges = 0; // 当前充能次数

        public TrackerState CurrentState => State;
        public ActionIds Icon => Config.Icon;

        public CooldownTracker( CooldownConfig config )
        {
            Config = config;
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
            foreach( var configTrigger in Config.Triggers )
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
            State = Config.Duration == 0 ? TrackerState.OnCD : TrackerState.Running;
            LastActiveTime = DateTime.Now;
            LastActiveTrigger = trigger;
        }

        public void Tick( Dictionary< Item, Status > buffDict )
        {
            // 计算充能次数（如果技能有充能）
            if( Config.MaxCharges > 1 )
            {
                CurrentCharges = CalculateCurrentCharges();
            }
            else
            {
                CurrentCharges = 0; // 非充能技能，不使用充能计数
            }

            if( State != TrackerState.Running &&
                UiHelper.CheckForTriggers( buffDict, Config.Triggers, out var trigger ) ) SetActive( trigger );

            if( State == TrackerState.Running )
            {
                TimeLeft = UiHelper.TimeLeft(
                    JobBars.Configuration.CooldownsHideActiveBuffDuration ? 0 : Config.Duration, buffDict,
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
                if( Config.MaxCharges > 1 )
                {
                    if( CurrentCharges > 0 )
                    {
                        // 还有充能可用，切换到 OffCD 状态
                        State = TrackerState.OffCD;
                        TimeLeft = 0;
                    }
                    else
                    {
                        // 所有充能都在冷却，计算剩余冷却时间
                        // 需要计算到下一次充能完成的时间
                        var recastActive = false;
                        float timeElapsed = 0;
                        foreach( var triggerItem in Config.Triggers )
                        {
                            if( triggerItem.Type != ItemType.Buff )
                            {
                                recastActive = UiHelper.GetRecastActive( triggerItem.Id, out timeElapsed );
                                if( recastActive ) break;
                            }
                        }
                        if( recastActive && Config.CD > 0 )
                        {
                            // 计算到下一次充能完成的时间
                            var timeUntilNextCharge = Config.CD - ( timeElapsed % Config.CD );
                            TimeLeft = timeUntilNextCharge;
                        }
                        else
                        {
                            TimeLeft = Config.CD;
                        }
                    }
                }
                else
                {
                    // 非充能技能，使用原来的逻辑
                    TimeLeft = ( float )( Config.CD - ( DateTime.Now - LastActiveTime ).TotalSeconds );

                    if( TimeLeft <= 0 )
                    {
                        State = TrackerState.OffCD;
                    }
                }
            }
            else if( State == TrackerState.OffCD && Config.MaxCharges > 1 )
            {
                // 对于充能技能，检查是否还有充能
                if( CurrentCharges <= 0 )
                {
                    State = TrackerState.OnCD;
                    // 计算剩余冷却时间
                    var recastActive = false;
                    float timeElapsed = 0;
                    foreach( var triggerItem in Config.Triggers )
                    {
                        if( triggerItem.Type != ItemType.Buff )
                        {
                            recastActive = UiHelper.GetRecastActive( triggerItem.Id, out timeElapsed );
                            if( recastActive ) break;
                        }
                    }
                    if( recastActive && Config.CD > 0 )
                    {
                        var timeUntilNextCharge = Config.CD - ( timeElapsed % Config.CD );
                        TimeLeft = timeUntilNextCharge;
                    }
                    else
                    {
                        TimeLeft = Config.CD;
                    }
                }
            }
        }

        private int CalculateCurrentCharges()
        {
            // 查找第一个有效的动作触发器来计算充能
            foreach( var trigger in Config.Triggers )
            {
                // 只处理动作类型（Action, GCD, OGCD），不处理 Buff
                if( trigger.Type != ItemType.Buff )
                {
                    var recastActive = UiHelper.GetRecastActive( trigger.Id, out var timeElapsed );
                    if( recastActive && Config.CD > 0 )
                    {
                        // 正在冷却中，计算已充能次数
                        // timeElapsed 是从上次使用开始经过的时间
                        // 每次充能需要 CD 时间，所以已充能次数 = floor(timeElapsed / CD)
                        var charges = ( int )Math.Floor( timeElapsed / Config.CD );
                        return Math.Min( charges, Config.MaxCharges );
                    }
                    else
                    {
                        // 不在冷却，充能已满
                        return Config.MaxCharges;
                    }
                }
            }
            // 如果没有找到有效的触发器，返回最大充能
            return Config.MaxCharges;
        }

        public void TickUi( CooldownNode node, float percent )
        {
            if( node == null ) return;

            if( node?.IconId != Config.Icon ) node.LoadIcon( Config.Icon );

            node.IsVisible = true;

            // 设置充能次数显示（右下角）
            if( Config.MaxCharges > 1 )
            {
                node.SetCharges( CurrentCharges, Config.MaxCharges );
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
                if( Config.ShowBorderWhenActive )
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
                node.SetOnCd();
                node.SetText( ( ( int )Math.Round( TimeLeft ) ).ToString() );
                node.SetNoDash();
            }
            else if( State == TrackerState.OffCD )
            {
                node.SetOffCd();
                node.SetText( "" );
                if( Config.ShowBorderWhenOffCD ) node.SetDash( percent );
                else node.SetNoDash();
            }
        }

        public void Reset()
        {
            State = TrackerState.None;
        }
    }
}