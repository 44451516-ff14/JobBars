using Dalamud.Bindings.ImGui;
using JobBars.Data;
using JobBars.Nodes.Builder;
using System;
using System.Numerics;

namespace JobBars.Gauges.Manager {
    public partial class GaugeManager {
        public bool LOCKED = true;

        private static readonly GaugePositionType[] ValidGaugePositionType = ( GaugePositionType[] )Enum.GetValues( typeof( GaugePositionType ) );

        private readonly InfoBox<GaugeManager> PositionInfoBox = new() {
            Label = "位置",
            ContentsAction = ( GaugeManager manager ) => {
                ImGui.Checkbox( "锁定位置" + manager.Id, ref manager.LOCKED );

                if( JobBars.Configuration.GaugePositionType != GaugePositionType.Split ) {
                    if( ImGui.Checkbox( "横向量表", ref JobBars.Configuration.GaugeHorizontal ) ) {
                        manager.UpdatePositionScale();
                        JobBars.Configuration.Save();
                    }

                    if( ImGui.Checkbox( "从下到上", ref JobBars.Configuration.GaugeBottomToTop ) ) {
                        manager.UpdatePositionScale();
                        JobBars.Configuration.Save();
                    }

                    if( ImGui.Checkbox( "右对齐", ref JobBars.Configuration.GaugeAlignRight ) ) {
                        manager.UpdatePositionScale();
                        JobBars.Configuration.Save();
                    }
                }

                if( JobBars.DrawCombo( ValidGaugePositionType, JobBars.Configuration.GaugePositionType, "量表定位", manager.Id, out var newPosition ) ) {
                    JobBars.Configuration.GaugePositionType = newPosition;
                    JobBars.Configuration.Save();

                    manager.UpdatePositionScale();
                }

                if( JobBars.Configuration.GaugePositionType == GaugePositionType.Global ) { // GLOBAL POSITIONING
                    var pos = JobBars.Configuration.GaugePositionGlobal;
                    if( ImGui.InputFloat2( "位置" + manager.Id, ref pos ) ) {
                        SetGaugePositionGlobal( pos );
                    }
                }
                else if( JobBars.Configuration.GaugePositionType == GaugePositionType.PerJob ) { // PER-JOB POSITIONING
                    var pos = manager.GetPerJobPosition();
                    if( ImGui.InputFloat2( $"位置 ({manager.CurrentJob})" + manager.Id, ref pos ) ) {
                        SetGaugePositionPerJob( manager.CurrentJob, pos );
                    }
                }

                if( ImGui.SliderFloat( "缩放" + manager.Id, ref JobBars.Configuration.GaugeScale, 0.1f, 2.0f ) ) {
                    manager.UpdatePositionScale();
                    JobBars.Configuration.Save();
                }
            }
        };

        private readonly InfoBox<GaugeManager> HideWhenInfoBox = new() {
            Label = "隐藏条件",
            ContentsAction = ( GaugeManager manager ) => {
                if( ImGui.Checkbox( "战斗外", ref JobBars.Configuration.GaugesHideOutOfCombat ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "收武器时", ref JobBars.Configuration.GaugesHideWeaponSheathed ) ) JobBars.Configuration.Save();
            }
        };

        protected override void DrawHeader() {
            if( ImGui.Checkbox( "启用量表" + Id, ref JobBars.Configuration.GaugesEnabled ) ) {
                JobBars.Configuration.Save();
            }
        }

        protected override void DrawSettings() {
            PositionInfoBox.Draw( this );
            HideWhenInfoBox.Draw( this );

            if( ImGui.Checkbox( "脉冲钻石和箭头颜色", ref JobBars.Configuration.GaugePulse ) ) JobBars.Configuration.Save();

            ImGui.SetNextItemWidth( 50f );
            if( ImGui.InputFloat( "滑读秒数 (0 = 关闭)", ref JobBars.Configuration.GaugeSlidecastTime ) ) JobBars.Configuration.Save();
        }

        public void DrawPositionBox() {
            if( LOCKED ) return;
            if( JobBars.Configuration.GaugePositionType == GaugePositionType.Split ) {
                foreach( var config in CurrentConfigs ) config.DrawPositionBox();
            }
            else if( JobBars.Configuration.GaugePositionType == GaugePositionType.PerJob ) {
                var currentPos = GetPerJobPosition();
                if( JobBars.DrawPositionView( $"量表栏 ({CurrentJob})##GaugePosition", currentPos, out var pos ) ) {
                    SetGaugePositionPerJob( CurrentJob, pos );
                }
            }
            else { // GLOBAL
                var currentPos = JobBars.Configuration.GaugePositionGlobal;
                if( JobBars.DrawPositionView( "量表栏##GaugePosition", currentPos, out var pos ) ) {
                    SetGaugePositionGlobal( pos );
                }
            }
        }

        private static void SetGaugePositionGlobal( Vector2 pos ) {
            JobBars.SetWindowPosition( "量表栏##GaugePosition", pos );
            JobBars.Configuration.GaugePositionGlobal = pos;
            JobBars.Configuration.Save();
            NodeBuilder.SetPositionGlobal( JobBars.NodeBuilder.GaugeRoot, pos );
        }

        private static void SetGaugePositionPerJob( JobIds job, Vector2 pos ) {
            JobBars.SetWindowPosition( $"量表栏 ({job})##GaugePosition", pos );
            JobBars.Configuration.GaugePerJobPosition.Set( $"{job}", pos );
            JobBars.Configuration.Save();
            NodeBuilder.SetPositionGlobal( JobBars.NodeBuilder.GaugeRoot, pos );
        }

        // ==========================================

        protected override void DrawItem( GaugeConfig item ) {
            ImGui.Indent( 5 );
            item.Draw( Id, out var newVisual, out var reset );
            ImGui.Unindent();

            if( SelectedJob != CurrentJob ) return;
            if( newVisual ) {
                UpdateVisuals();
                UpdatePositionScale();
            }
            if( reset ) Reset();
        }

        protected override string ItemToString( GaugeConfig item ) => item.Name;

        protected override bool IsEnabled( GaugeConfig item ) => item.Enabled;
    }
}
