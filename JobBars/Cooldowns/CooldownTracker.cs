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
        private readonly ulong _ObjectId;
        private readonly bool _IsLocalPlayer;

        private TrackerState State = TrackerState.None;
        private DateTime LastActiveTime;
        private Item LastActiveTrigger;
        private float TimeLeft;
        private int CurrentCharges = 0; // 当前充能次数
        private int ChargesUsedSinceMax = 0; // 从满充能开始使用的次数（用于队友的时间推算）

        public TrackerState CurrentState => State;
        public ActionIds Icon => _CooldownConfig.Icon;

        public CooldownTracker( CooldownConfig cooldownConfig, ulong objectId )
        {
            _CooldownConfig = cooldownConfig;
            _ObjectId = objectId;
            _IsLocalPlayer = Dalamud.ClientState.LocalPlayer != null && 
                             objectId == Dalamud.ClientState.LocalPlayer.GameObjectId;
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
            
            // 对于队友的充能技能，记录使用次数
            if( !_IsLocalPlayer && _CooldownConfig.MaxCharges > 1 )
            {
                ChargesUsedSinceMax++;
                // 限制最大使用次数
                if( ChargesUsedSinceMax > _CooldownConfig.MaxCharges )
                {
                    ChargesUsedSinceMax = _CooldownConfig.MaxCharges;
                }
            }
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
                        TimeLeft = CalculateTimeUntilNextCharge();
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
                    TimeLeft = CalculateTimeUntilNextCharge();
                }
                else if( CurrentCharges < _CooldownConfig.MaxCharges )
                {
                    // 充能未满，计算到下一次充能完成的时间
                    TimeLeft = CalculateTimeUntilNextCharge();
                }
                else
                {
                    // 充能已满
                    TimeLeft = 0;
                }
            }
        }
        
        // 统一的计算到下一次充能完成时间的方法
        private float CalculateTimeUntilNextCharge()
        {
            if( _IsLocalPlayer )
            {
                // 本地玩家：使用 ActionManager 获取精确时间
                foreach( var triggerItem in _CooldownConfig.Triggers )
                {
                    if( triggerItem.Type != ItemType.Buff )
                    {
                        var recastActive = UiHelper.GetRecastActive( triggerItem.Id, out var timeElapsed );
                        if( recastActive && _CooldownConfig.CD > 0 )
                        {
                            var timeUntilNextCharge = _CooldownConfig.CD - ( timeElapsed % _CooldownConfig.CD );
                            return timeUntilNextCharge;
                        }
                    }
                }
                return _CooldownConfig.CD;
            }
            else
            {
                // 队友：使用基于时间的推算
                return CalculatePartyMemberTimeUntilNextCharge();
            }
        }

        private int CalculateCurrentCharges()
        {
            // 对于本地玩家，使用 ActionManager 获取精确的充能信息
            if( _IsLocalPlayer )
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
            else
            {
                // 对于队友，使用基于时间的推算
                if( State == TrackerState.None )
                {
                    // 从未使用过，充能已满
                    ChargesUsedSinceMax = 0;
                    return _CooldownConfig.MaxCharges;
                }
                
                // 计算从第一次使用到现在已经恢复了多少充能
                var timeSinceFirstUse = ( float )( DateTime.Now - LastActiveTime ).TotalSeconds;
                
                // 如果技能有持续时间（Running 状态），需要加上持续时间
                if( State == TrackerState.Running )
                {
                    // 技能正在生效，还没开始冷却
                    var chargesAfterUse = _CooldownConfig.MaxCharges - ChargesUsedSinceMax;
                    return Math.Max( 0, chargesAfterUse );
                }
                
                // 计算已恢复的充能数
                var chargesRecharged = ( int )Math.Floor( timeSinceFirstUse / _CooldownConfig.CD );
                
                // 当前充能 = 最大充能 - 已使用 + 已恢复
                var currentCharges = _CooldownConfig.MaxCharges - ChargesUsedSinceMax + chargesRecharged;
                
                // 确保在有效范围内
                currentCharges = Math.Clamp( currentCharges, 0, _CooldownConfig.MaxCharges );
                
                // 如果充能已满，重置使用计数
                if( currentCharges >= _CooldownConfig.MaxCharges )
                {
                    ChargesUsedSinceMax = 0;
                }
                
                return currentCharges;
            }
        }
        
        // 计算队友充能技能到下一次充能完成的剩余时间
        private float CalculatePartyMemberTimeUntilNextCharge()
        {
            if( State == TrackerState.None || _CooldownConfig.CD <= 0 )
            {
                return 0;
            }
            
            var timeSinceFirstUse = ( float )( DateTime.Now - LastActiveTime ).TotalSeconds;
            
            // 如果在 Running 状态，需要等技能结束后才开始冷却
            if( State == TrackerState.Running )
            {
                var timeLeftInRunning = _CooldownConfig.Duration - timeSinceFirstUse;
                if( timeLeftInRunning > 0 )
                {
                    return timeLeftInRunning + _CooldownConfig.CD;
                }
            }
            
            // 计算到下一次充能完成的时间
            var timeUntilNextCharge = _CooldownConfig.CD - ( timeSinceFirstUse % _CooldownConfig.CD );
            return timeUntilNextCharge;
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
                    // 使用统一的计算方法
                    var timeUntilNextCharge = CalculateTimeUntilNextCharge();
                    if( timeUntilNextCharge > 0 )
                    {
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
            ChargesUsedSinceMax = 0;
        }
    }
}