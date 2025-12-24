using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit;
using KamiToolKit.Nodes;

namespace JobBars.Nodes.Highlight {
    public unsafe class HighlightNode : NodeBase<AtkResNode> {
        private readonly SimpleNineGridNode Highlight;

        public HighlightNode() : base( NodeType.Res ) {
            NodeId = JobBars.NodeId++;
            NodeFlags = NodeFlags.AnchorLeft | NodeFlags.AnchorTop;
            Size = new( 100, 100 );

            Highlight = new SimpleNineGridNode() {
                NodeId = JobBars.NodeId++,
                Size = new( 320, 48 ),
                Position = new( 52, 18 ),
                TextureCoordinates = new( 112, 0 ),
                TextureSize = new( 48, 48 ),
                Offsets = new( 20, 20, 20, 20 ),
                NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop,
                MultiplyColor = new( 150f / 100f, 100f / 100f, 50 / 100f ),
            };
            Highlight.TexturePath = "ui/uld/PartyListTargetBase.tex";

            Highlight.AttachNode( this, NodePosition.AsLastChild );
        }

        protected override void Dispose( bool disposing, bool isNativeDestructor ) {
            if( disposing ) {
                Highlight.Dispose();
                base.Dispose( disposing, isNativeDestructor );
            }
        }
    }
}
