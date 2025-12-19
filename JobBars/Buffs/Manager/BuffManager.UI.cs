using Dalamud.Bindings.ImGui;
using JobBars.Data;
using JobBars.Nodes.Builder;
using System.Numerics;

namespace JobBars.Buffs.Manager {
    public partial class BuffManager {
        public bool LOCKED = true;

        private readonly InfoBox<BuffManager> PositionInfoBox = new() {
            Label = "位置",
            ContentsAction = ( BuffManager manager ) => {
                ImGui.Checkbox( "锁定位置" + manager.Id, ref manager.LOCKED );

                ImGui.SetNextItemWidth( 25f );
                if( ImGui.InputInt( "每行增益数" + manager.Id, ref JobBars.Configuration.BuffHorizontal, 0 ) ) {
                    JobBars.Configuration.Save();
                    JobBars.NodeBuilder.BuffRoot.Update();
                }

                if( ImGui.Checkbox( "从右到左" + manager.Id, ref JobBars.Configuration.BuffRightToLeft ) ) {
                    JobBars.Configuration.Save();
                    JobBars.NodeBuilder.BuffRoot.Update();
                }

                if( ImGui.Checkbox( "从下到上" + manager.Id, ref JobBars.Configuration.BuffBottomToTop ) ) {
                    JobBars.Configuration.Save();
                    JobBars.NodeBuilder.BuffRoot.Update();
                }

                if( ImGui.Checkbox( "方形增益" + manager.Id, ref JobBars.Configuration.BuffSquare ) ) {
                    JobBars.Configuration.Save();
                    JobBars.NodeBuilder.BuffRoot.Update();
                }

                if( ImGui.SliderFloat( "缩放" + manager.Id, ref JobBars.Configuration.BuffScale, 0.1f, 2.0f ) ) {
                    UpdatePositionScale();
                    JobBars.Configuration.Save();
                }

                var pos = JobBars.Configuration.BuffPosition;
                if( ImGui.InputFloat2( "位置" + manager.Id, ref pos ) ) {
                    SetBuffPosition( pos );
                }
            }
        };

        private readonly InfoBox<BuffManager> HideWhenInfoBox = new() {
            Label = "隐藏条件",
            ContentsAction = ( BuffManager manager ) => {
                if( ImGui.Checkbox( "战斗外", ref JobBars.Configuration.BuffHideOutOfCombat ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "收武器时", ref JobBars.Configuration.BuffHideWeaponSheathed ) ) JobBars.Configuration.Save();
            }
        };

        protected override void DrawHeader() {
            if( ImGui.Checkbox( "启用增益栏" + Id, ref JobBars.Configuration.BuffBarEnabled ) ) {
                JobBars.Configuration.Save();
                ResetUi();
            }
        }

        protected override void DrawSettings() {
            PositionInfoBox.Draw( this );
            HideWhenInfoBox.Draw( this );

            ImGui.SetNextItemWidth( 50f );
            if( ImGui.InputFloat( "隐藏冷却时间超过" + Id, ref JobBars.Configuration.BuffDisplayTimer ) ) JobBars.Configuration.Save();

            if( ImGui.Checkbox( "显示队友的增益", ref JobBars.Configuration.BuffIncludeParty ) ) {
                JobBars.Configuration.Save();
                ResetUi();
            }

            if( ImGui.Checkbox( "细增益边框", ref JobBars.Configuration.BuffThinBorder ) ) {
                JobBars.Configuration.Save();
                JobBars.NodeBuilder.BuffRoot.Update();
            }

            ImGui.SetNextItemWidth( 50f );
            if( ImGui.InputFloat( "冷却时透明度" + Id, ref JobBars.Configuration.BuffOnCDOpacity ) ) JobBars.Configuration.Save();

            ImGui.SetNextItemWidth( 100f );
            if( ImGui.InputInt( "增益文字大小", ref JobBars.Configuration.BuffTextSize_v2 ) ) {
                if( JobBars.Configuration.BuffTextSize_v2 <= 0 ) JobBars.Configuration.BuffTextSize_v2 = 1;
                if( JobBars.Configuration.BuffTextSize_v2 > 255 ) JobBars.Configuration.BuffTextSize_v2 = 255;
                JobBars.Configuration.Save();
                JobBars.NodeBuilder.BuffRoot.Update();
            }
        }

        protected override void DrawItem( BuffConfig[] item, JobIds _ ) {
            var reset = false;
            foreach( var buff in item ) buff.Draw( Id, ref reset );
            if( reset ) ResetUi();
        }

        public void DrawPositionBox() {
            if( LOCKED ) return;
            if( JobBars.DrawPositionView( "增益栏##BuffPosition", JobBars.Configuration.BuffPosition, out var pos ) ) SetBuffPosition( pos );
        }

        private static void SetBuffPosition( Vector2 pos ) {
            JobBars.SetWindowPosition( "增益栏##BuffPosition", pos );
            JobBars.Configuration.BuffPosition = pos;
            JobBars.Configuration.Save();
            NodeBuilder.SetPositionGlobal( JobBars.NodeBuilder.BuffRoot, JobBars.Configuration.BuffPosition );
        }
    }
}
