using FFXIVClientStructs.FFXIV.Component.GUI;
using JobBars.Atk;
using KamiToolKit.Classes;
using KamiToolKit;
using KamiToolKit.Nodes;

namespace JobBars.Nodes.Gauge.Diamond {
    public unsafe class DiamondTick : NodeBase<AtkResNode> {
        public readonly SimpleImageNode Background;
        public readonly ResNode SelectedContainer;
        public readonly SimpleImageNode Selected;
        public readonly TextNode Text;

        private ElementColor TickColor = ColorConstants.NoColor;

        public DiamondTick() : base( NodeType.Res ) {
            NodeId = JobBars.NodeId++;
            Size = new( 32, 32 );

            Background = new SimpleImageNode() {
                NodeId = JobBars.NodeId++,
                Size = new( 32, 32 ),
                TextureCoordinates = new( 0, 0 ),
                TextureSize = new( 32, 32 ),
                NodeFlags = NodeFlags.Visible,
                WrapMode = WrapMode.None,
                ImageNodeFlags = 0,
            };
            Background.TexturePath = "ui/uld/JobHudSimple_StackA.tex";

            SelectedContainer = new ResNode() {
                NodeId = JobBars.NodeId++,
                Size = new( 32, 32 ),
                Origin = new( 16, 16 ),
                NodeFlags = NodeFlags.Visible,
            };

            Selected = new SimpleImageNode() {
                NodeId = JobBars.NodeId++,
                Size = new( 32, 32 ),
                Origin = new( 16, 16 ),
                TextureCoordinates = new( 32, 0 ),
                TextureSize = new( 32, 32 ),
                NodeFlags = NodeFlags.Visible,
                WrapMode = WrapMode.None,
                ImageNodeFlags = 0,
            };
            Selected.TexturePath = "ui/uld/JobHudSimple_StackA.tex";

            Text = new TextNode() {
                NodeId = JobBars.NodeId++,
                Position = new( 0, 20 ),
                Size = new( 32, 32 ),
                FontSize = 14,
                LineSpacing = 14,
                AlignmentType = (AlignmentType)4,
                NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorRight,
                TextColor = new( 1, 1, 1, 1 ),
                TextOutlineColor = new( 40f / 255f, 40f / 255f, 40f / 255f, 1 ),
                TextId = 0,
                TextFlags = TextFlags.Glare,
                String = "",
            };

            Selected.AttachNode( SelectedContainer, NodePosition.AsLastChild );
            Text.AttachNode( SelectedContainer, NodePosition.AsLastChild );

            Background.AttachNode( this, NodePosition.AsLastChild );
            SelectedContainer.AttachNode( this, NodePosition.AsLastChild );
        }

        public void SetColor( ElementColor color ) {
            TickColor = color;
            TickColor.SetColor( Selected );
        }

        public void Tick( float percent ) => TickColor.SetColorPulse( Selected, percent );

        protected override void Dispose( bool disposing, bool isNativeDestructor ) {
            if( disposing ) {
                Background.Dispose();
                SelectedContainer.Dispose();
                Selected.Dispose();
                Text.Dispose();
                base.Dispose( disposing, isNativeDestructor );
            }
        }
    }
}
