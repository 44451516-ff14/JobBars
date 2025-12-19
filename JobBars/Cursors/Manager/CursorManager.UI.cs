using Dalamud.Bindings.ImGui;
using JobBars.Data;
using System;

namespace JobBars.Cursors.Manager {
    public partial class CursorManager {
        private static readonly CursorPositionType[] ValidCursorPositionType = ( CursorPositionType[] )Enum.GetValues( typeof( CursorPositionType ) );

        private readonly InfoBox<CursorManager> HideWhenInfoBox = new() {
            Label = "隐藏条件",
            ContentsAction = ( CursorManager manager ) => {
                if( ImGui.Checkbox( "按住鼠标时", ref JobBars.Configuration.CursorHideWhenHeld ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "战斗外", ref JobBars.Configuration.CursorHideOutOfCombat ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "收武器时", ref JobBars.Configuration.CursorHideWeaponSheathed ) ) JobBars.Configuration.Save();
            }
        };

        protected override void DrawHeader() {
            if( ImGui.Checkbox( "启用光标" + Id, ref JobBars.Configuration.CursorsEnabled ) ) JobBars.Configuration.Save();
        }

        protected override void DrawSettings() {
            HideWhenInfoBox.Draw( this );

            if( JobBars.DrawCombo( ValidCursorPositionType, JobBars.Configuration.CursorPosition, "光标定位", Id, out var newPosition ) ) {
                JobBars.Configuration.CursorPosition = newPosition;
                JobBars.Configuration.Save();
            }

            if( JobBars.Configuration.CursorPosition == CursorPositionType.CustomPosition ) {
                if( ImGui.InputFloat2( "自定义光标位置", ref JobBars.Configuration.CursorCustomPosition ) ) {
                    JobBars.Configuration.Save();
                }
            }

            if( ImGui.InputFloat( "内圈缩放" + Id, ref JobBars.Configuration.CursorInnerScale ) ) JobBars.Configuration.Save();
            if( ImGui.InputFloat( "外圈缩放" + Id, ref JobBars.Configuration.CursorOuterScale ) ) JobBars.Configuration.Save();

            if( Configuration.DrawColor( "内圈颜色", InnerColor, out var newColorInner ) ) {
                InnerColor = newColorInner;
                JobBars.Configuration.CursorInnerColor = newColorInner.Name;
                JobBars.Configuration.Save();
                JobBars.NodeBuilder.CursorRoot.SetInnerColor( InnerColor );
            }

            if( Configuration.DrawColor( "外圈颜色", OuterColor, out var newColorOuter ) ) {
                OuterColor = newColorOuter;
                JobBars.Configuration.CursorOuterColor = newColorOuter.Name;
                JobBars.Configuration.Save();
                JobBars.NodeBuilder.CursorRoot.SetOuterColor( OuterColor );
            }
        }

        protected override void DrawItem( Cursor item, JobIds _ ) {
            item.Draw( Id );
        }
    }
}
