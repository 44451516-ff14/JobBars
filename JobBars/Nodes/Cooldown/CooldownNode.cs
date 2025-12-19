using FFXIVClientStructs.FFXIV.Component.GUI;
using JobBars.Data;
using JobBars.Helper;
using KamiToolKit.Classes;
using KamiToolKit;
using KamiToolKit.Nodes;

namespace JobBars.Nodes.Cooldown {
    public unsafe class CooldownNode : NodeBase<AtkResNode> {
        public static readonly ushort WIDTH = 40;
        public static readonly ushort HEIGHT = 40;

        private readonly TextNode Text;
        private readonly SimpleImageNode Icon;
        private readonly SimpleImageNode Border;

        private ActionIds LastAction = 0;
        public ActionIds IconId => LastAction;

        public CooldownNode() : base( NodeType.Res ) {
            NodeId = JobBars.NodeId++;
            Size = new( WIDTH, HEIGHT );

            Icon = new SimpleImageNode() {
                NodeId = JobBars.NodeId++,
                Size = new( WIDTH, HEIGHT ),
                NodeFlags = NodeFlags.Visible,
                ImageNodeFlags = ImageNodeFlags.AutoFit,
                TextureSize = new( 40, 40 ),
            };
            Icon.LoadIcon( 405 );

            Border = new SimpleImageNode() {
                NodeId = JobBars.NodeId++,
                Size = new( 49, 47 ),
                NodeFlags = NodeFlags.Visible,
                WrapMode = WrapMode.None,
                ImageNodeFlags = 0,
                Position = new( -4, -2 ),
                TextureCoordinates = new( 0, 96 ),
                TextureSize = new( 48, 48 ),
                Scale = new( ( ( float )WIDTH + 8 ) / 49.0f, ( ( float )HEIGHT + 6 ) / 47.0f )
            };
            Border.TexturePath = "ui/uld/IconA_Frame.tex";

            Text = new TextNode() {
                NodeId = JobBars.NodeId++,
                Size = new( WIDTH, HEIGHT ),
                FontSize = 24,
                LineSpacing = ( byte )HEIGHT,
                AlignmentType = (AlignmentType)52,
                TextColor = new( 1, 1, 1, 1 ),
                TextOutlineColor = new( 0, 0, 0, 1 ),
                TextId = 0,
                TextFlags = TextFlags.Glare,
                String = "",
            };

            Icon.AttachNode( this, NodePosition.AsLastChild );
            Border.AttachNode( this, NodePosition.AsLastChild );
            Text.AttachNode( this, NodePosition.AsLastChild );
        }

        public void SetNoDash() {
            Border.TextureCoordinates = new( 0, 96 );
            Border.TextureSize = new( 48, 48 );
        }

        public void SetDash( float percent ) {
            var partId = ( int )( percent * 7 ); // 0 - 6

            var row = partId % 3;
            var column = ( partId - row ) / 3;

            var u = ( ushort )( 96 + ( 48 * row ) );
            var v = ( ushort )( 48 * column );

            Border.TextureCoordinates = new( u, v );
            Border.TextureSize = new( 48, 48 );
        }

        public void SetText( string text ) {
            Text.FontSize = text.Length > 2 ? ( byte )20 : ( byte )24;
            Text.String = text;
            Text.IsVisible = true;
        }

        public void SetOnCd() {
            Icon.MultiplyColor = new( 75f / 255f, 75f / 255f, 75f / 255f );
            Color = Color with {
                W = JobBars.Configuration.CooldownsOnCDOpacity
            };
        }

        public void SetOffCd() {
            Icon.MultiplyColor = new( 1.0f, 1.0f, 1.0f );
            Color = Color with {
                W = 1f
            };
        }

        public void LoadIcon( ActionIds action ) {
            if( action == LastAction ) return;
            LastAction = action;
            var actionid = (uint)action;
            var adjustedAction = UiHelper.GetAdjustedAction(actionid);
            uint icon = UiHelper.GetIcon( adjustedAction );
            Icon.LoadIcon(icon);
        }

        protected override void Dispose( bool disposing, bool isNativeDestructor ) {
            if( disposing ) {
                Icon.Dispose();
                Border.Dispose();
                Text.Dispose();
                base.Dispose( disposing, isNativeDestructor );
            }
        }
    }
}
