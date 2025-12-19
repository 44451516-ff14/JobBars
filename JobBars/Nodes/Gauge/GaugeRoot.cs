using FFXIVClientStructs.FFXIV.Component.GUI;
using JobBars.Nodes.Gauge.Arrow;
using JobBars.Nodes.Gauge.Bar;
using JobBars.Nodes.Gauge.Diamond;
using KamiToolKit.Classes;
using KamiToolKit;
using System.Collections.Generic;

namespace JobBars.Nodes.Gauge {
    public unsafe class GaugeRoot : NodeBase<AtkResNode> {
        private static readonly int MAX_GAUGES = 7;

        public readonly List<BarNode> Bars = [];
        public readonly List<ArrowNode> Arrows = [];
        public readonly List<DiamondNode> Diamonds = [];

        public GaugeRoot() : base( NodeType.Res ) {
            NodeId = JobBars.NodeId++;
            Size = new( 256, 100 );

            for( var i = 0; i < MAX_GAUGES; i++ ) {
                Bars.Add( new BarNode() );
                Arrows.Add( new ArrowNode() );
                Diamonds.Add( new DiamondNode() );
            }

            foreach( var bar in Bars ) bar.AttachNode( this, NodePosition.AsLastChild );
            foreach( var arrow in Arrows ) arrow.AttachNode( this, NodePosition.AsLastChild );
            foreach( var diamond in Diamonds ) diamond.AttachNode( this, NodePosition.AsLastChild );
        }

        public void HideAll() {
            foreach( var item in Bars ) item.IsVisible = false;
            foreach( var item in Arrows ) item.IsVisible = false;
            foreach( var item in Diamonds ) item.IsVisible = false;
        }

        protected override void Dispose( bool disposing, bool isNativeDestructor ) {
            if( disposing ) {
                foreach( var item in Bars ) item.Dispose();
                foreach( var item in Arrows ) item.Dispose();
                foreach( var item in Diamonds ) item.Dispose();
                base.Dispose( disposing, isNativeDestructor );
            }
        }
    }
}
