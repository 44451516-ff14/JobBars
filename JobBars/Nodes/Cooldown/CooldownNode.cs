using FFXIVClientStructs.FFXIV.Component.GUI;
using JobBars.Data;
using JobBars.Helper;
using KamiToolKit.Classes;
using KamiToolKit;
using KamiToolKit.Nodes;

namespace JobBars.Nodes.Cooldown {
    public unsafe class CooldownNode : NodeBase<AtkResNode> {
        public static readonly ushort WIDTH = 30;
        public static readonly ushort HEIGHT = 30;

        private readonly TextNode Text;
        private readonly TextNode ChargesText; // 右下角显示充能次数
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
                TextureSize = new( 30, 30 ),
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
                FontSize = ( uint )JobBars.Configuration.CooldownsTextSize,
                LineSpacing = ( byte )HEIGHT,
                AlignmentType = (AlignmentType)52,
                TextColor = new( 1, 1, 1, 1 ),
                TextOutlineColor = new( 0, 0, 0, 1 ),
                TextId = 0,
                TextFlags = TextFlags.Glare,
                String = "",
            };

            // 充能次数文本节点（与倒计时文字相同位置）
            ChargesText = new TextNode() {
                NodeId = JobBars.NodeId++,
                Size = new( WIDTH, HEIGHT ),
                FontSize = ( uint )JobBars.Configuration.CooldownsChargesTextSize,
                LineSpacing = ( byte )HEIGHT,
                AlignmentType = (AlignmentType)52, // 与倒计时文字相同的对齐方式
                TextColor = new( 1, 1, 0.5f, 1 ), // 黄色
                TextOutlineColor = new( 0, 0, 0, 1 ),
                TextId = 0,
                TextFlags = TextFlags.Edge,
                String = "",
            };
            ChargesText.IsVisible = false;

            Icon.AttachNode( this, NodePosition.AsLastChild );
            Border.AttachNode( this, NodePosition.AsLastChild );
            Text.AttachNode( this, NodePosition.AsLastChild );
            ChargesText.AttachNode( this, NodePosition.AsLastChild );
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
            var baseSize = ( uint )JobBars.Configuration.CooldownsTextSize;
            Text.FontSize = baseSize;
            Text.String = text;
            Text.IsVisible = true;
        }

        public void SetCharges( int currentCharges, int maxCharges ) {
            // 更新字体大小以反映配置变化
            ChargesText.FontSize = ( uint )JobBars.Configuration.CooldownsChargesTextSize;
            
            if( maxCharges > 1 && currentCharges < maxCharges ) {
                ChargesText.String = currentCharges.ToString();
                ChargesText.IsVisible = true;
            }
            else {
                ChargesText.IsVisible = false;
            }
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
                ChargesText.Dispose();
                base.Dispose( disposing, isNativeDestructor );
            }
        }
    }
}
