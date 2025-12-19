using FFXIVClientStructs.FFXIV.Component.GUI;
using JobBars.Atk;
using KamiToolKit.Classes;
using KamiToolKit;
using KamiToolKit.Nodes;

namespace JobBars.Nodes.Gauge.Arrow {
    public unsafe class ArrowTick : NodeBase<AtkResNode> {
        public readonly SimpleImageNode Background;
        public readonly ResNode SelectedContainer;
        public readonly SimpleImageNode Selected;

        private ElementColor TickColor = ColorConstants.NoColor;

        public ArrowTick() : base( NodeType.Res ) {
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
            Background.TexturePath = "ui/uld/JobHudSimple_StackB.tex";

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
            Selected.TexturePath = "ui/uld/JobHudSimple_StackB.tex";

            Selected.AttachNode( SelectedContainer, NodePosition.AsLastChild );

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
                base.Dispose( disposing, isNativeDestructor );
            }
        }
    }
}
