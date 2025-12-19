using Dalamud.Bindings.ImGui;
using JobBars.Data;

namespace JobBars.Icons.Manager {
    public partial class IconManager {
        private readonly InfoBox<IconManager> LargeIconInfoBox = new() {
            Label = "大文字",
            ContentsAction = ( IconManager manager ) => {
                if( ImGui.Checkbox( "增益图标" + manager.Id, ref JobBars.Configuration.IconBuffLarge ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( "计时器图标" + manager.Id, ref JobBars.Configuration.IconTimerLarge ) ) JobBars.Configuration.Save();
            }
        };

        protected override void DrawHeader() {
            if( ImGui.Checkbox( "启用图标替换", ref JobBars.Configuration.IconsEnabled ) ) {
                JobBars.Configuration.Save();
                Reset();
            }
        }

        protected override void DrawSettings() {
            LargeIconInfoBox.Draw( this );
        }

        protected override void DrawItem( IconReplacer[] item, JobIds _ ) {
            foreach( var icon in item ) {
                icon.Draw( Id, SelectedJob );
            }
        }
    }
}