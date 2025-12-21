using Dalamud.Bindings.ImGui;
using JobBars.Data;

namespace JobBars.Cooldowns.Manager {
    public unsafe partial class CooldownManager {
        private readonly InfoBox<CooldownManager> PositionInfoBox = new() {
            Label = "位置",
            ContentsAction = ( CooldownManager manager ) => {
                if( ImGui.Checkbox( "左对齐" + manager.Id, ref JobBars.Configuration.CooldownsLeftAligned ) ) {
                    JobBars.Configuration.Save();
                    manager.ResetUi();
                }

                if( ImGui.SliderFloat( "缩放" + manager.Id, ref JobBars.Configuration.CooldownScale, 0.1f, 2.0f ) ) {
                    UpdatePositionScale();
                    JobBars.Configuration.Save();
                }

                if( ImGui.InputFloat2( "位置" + manager.Id, ref JobBars.Configuration.CooldownPosition ) ) {
                    UpdatePositionScale();
                    JobBars.Configuration.Save();
                }

                if( ImGui.InputFloat( "行高" + manager.Id, ref JobBars.Configuration.CooldownsSpacing ) ) {
                    UpdatePositionScale();
                    JobBars.Configuration.Save();
                }
            }
        };

        private readonly InfoBox<CooldownManager> ShowIconInfoBox = new() {
            Label = "显示图标条件",
            ContentsAction = ( CooldownManager manager ) => {
                if( ImGui.Checkbox( "默认" + manager.Id, ref JobBars.Configuration.CooldownsStateShowDefault ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "激活时" + manager.Id, ref JobBars.Configuration.CooldownsStateShowRunning ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "冷却中" + manager.Id, ref JobBars.Configuration.CooldownsStateShowOnCD ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "冷却完毕" + manager.Id, ref JobBars.Configuration.CooldownsStateShowOffCD ) ) JobBars.Configuration.Save();
            }
        };

        private readonly InfoBox<CooldownManager> HideWhenInfoBox = new() {
            Label = "隐藏条件",
            ContentsAction = ( CooldownManager manager ) => {
                if( ImGui.Checkbox( "战斗外", ref JobBars.Configuration.CooldownsHideOutOfCombat ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "收武器时", ref JobBars.Configuration.CooldownsHideWeaponSheathed ) ) JobBars.Configuration.Save();
            }
        };

        private readonly CustomCooldownDialog CustomCooldownDialog = new();

        protected override void DrawHeader() {
            CustomCooldownDialog.Draw();

            if( ImGui.Checkbox( "启用冷却时间" + Id, ref JobBars.Configuration.CooldownsEnabled ) ) {
                JobBars.Configuration.Save();
                ResetUi();
            }
        }

        protected override void DrawSettings() {
            PositionInfoBox.Draw( this );
            ShowIconInfoBox.Draw( this );
            HideWhenInfoBox.Draw( this );

            if( ImGui.Checkbox( "隐藏激活增益文字" + Id, ref JobBars.Configuration.CooldownsHideActiveBuffDuration ) ) JobBars.Configuration.Save();

            if( ImGui.Checkbox( "显示队友的冷却时间" + Id, ref JobBars.Configuration.CooldownsShowPartyMembers ) ) {
                JobBars.Configuration.Save();
                ResetUi();
            }

            ImGui.SetNextItemWidth( 50f );
            if( ImGui.InputFloat( "冷却时透明度" + Id, ref JobBars.Configuration.CooldownsOnCDOpacity ) ) JobBars.Configuration.Save();

            ImGui.SetNextItemWidth( 100f );
            if( ImGui.InputInt( "冷却时间文字大小" + Id, ref JobBars.Configuration.CooldownsTextSize ) ) {
                if( JobBars.Configuration.CooldownsTextSize <= 0 ) JobBars.Configuration.CooldownsTextSize = 1;
                if( JobBars.Configuration.CooldownsTextSize > 255 ) JobBars.Configuration.CooldownsTextSize = 255;
                JobBars.Configuration.Save();
            }

            ImGui.SetNextItemWidth( 100f );
            if( ImGui.InputInt( "充能次数字体大小" + Id, ref JobBars.Configuration.CooldownsChargesTextSize ) ) {
                if( JobBars.Configuration.CooldownsChargesTextSize <= 0 ) JobBars.Configuration.CooldownsChargesTextSize = 1;
                if( JobBars.Configuration.CooldownsChargesTextSize > 255 ) JobBars.Configuration.CooldownsChargesTextSize = 255;
                JobBars.Configuration.Save();
            }
        }

        protected override void DrawItem( CooldownConfig[] item, JobIds job ) {
            var reset = false;
            foreach( var cooldown in item ) cooldown.Draw( Id, false, ref reset );

            // Delete custom
            if( CustomCooldowns.TryGetValue( job, out var customCooldowns ) ) {
                foreach( var custom in customCooldowns ) {
                    if( custom.Draw( Id, true, ref reset ) ) {
                        DeleteCustomCooldown( job, custom );
                        reset = true;
                        break;
                    }
                }
            }

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 10 );
            if( ImGui.Button( $"+ 添加自定义冷却时间{Id}" ) ) CustomCooldownDialog.Show( job );

            if( reset ) ResetUi();
        }
    }
}