using System;
using System.Collections.Generic;
using System.Linq;
using Toe.Scripting.Helpers;

namespace Toe.Scripting
{
    public class SimpleLayoutAlgorithm : ILayoutAlgorithm
    {
        public class LayoutInfo
        {
            public int Depth { get; set; }
            public int ClusterId { get; set; }
            public float Width { get; set; } = 100;
            public float Height { get; set; } = 100;
            public float X { get; set; }
            public float Y { get; set; }
        }
        public void ArrangeNodes(Script script)
        {
            var scriptHelper = new ScriptHelper<LayoutInfo>(script);

            foreach (var node in scriptHelper.Nodes)
            {
                node.Extra = new LayoutInfo();
            }

            if (script.Layout != null)
            {
                var nodesById = scriptHelper.Nodes.ToDictionary(_ => _.Id);
                foreach (var layoutNode in script.Layout.Nodes)
                {
                    NodeHelper<LayoutInfo> node;
                    if (nodesById.TryGetValue(layoutNode.NodeId, out node))
                    {
                        node.Extra.Width = Math.Max(100, layoutNode.Width);
                        node.Extra.Height = Math.Max(100, layoutNode.Height);
                    }
                }
            }
        

            int maxClusterId = 1;
            foreach (var node in scriptHelper.Nodes)
            {
                if (node.Extra.ClusterId == 0)
                {
                    MarkClusterNodes(node, maxClusterId);
                    ++maxClusterId;
                }
            }

            float clusterY = 0;
            foreach (var cluster in scriptHelper.Nodes.ToLookup(_ => _.Extra.ClusterId))
            {
                foreach (var node in cluster)
                {
                    EvaluateDepth(node);
                }
                foreach (var node in cluster.OrderByDescending(_=>_.Extra.Depth))
                {
                    MaximizeDepth(node);
                }

                float horisontalMargin = 100;
                float verticalMargin = 30;
                float x = 0;
                float nextCluster = clusterY;
                foreach (var nodes in cluster.ToLookup(_ => _.Extra.Depth).OrderBy(_=>_.Key))
                {
                    float y = clusterY;
                    float nextX = x;
                    foreach (var node in nodes)
                    {
                        node.Extra.X = x;
                        node.Extra.Y = y;
                        y += node.Extra.Height + verticalMargin;
                        nextX = Math.Max(nextX, x + node.Extra.Width + horisontalMargin);
                    }
                    nextCluster = Math.Max(nextCluster, y);
                    x = nextX;
                }

                clusterY = nextCluster;
            }
            script.Layout = new ScriptLayout { Nodes = scriptHelper.Nodes.Select(_ => new ScriptNodeLayout(){X = _.Extra.X, Y = _.Extra.Y, Width = _.Extra.Width, Height = _.Extra.Height, NodeId = _.Id}).ToList() };
        }
        private void MaximizeDepth(NodeHelper<LayoutInfo> node)
        {
            int minDepth = int.MaxValue;
            foreach (var pin in node.OutputPins.Concat(node.ExitPins).SelectMany(_ => _.Links))
            {
                if (minDepth > pin.To.Node.Extra.Depth)
                    minDepth = pin.To.Node.Extra.Depth;
            }

            if (minDepth != int.MaxValue)
            {
                node.Extra.Depth = minDepth - 1;
            }
        }
        private int EvaluateDepth(NodeHelper<LayoutInfo> node)
        {
            if (node.Extra.Depth > 0)
            {
                return node.Extra.Depth;
            }

            node.Extra.Depth = 1;
            foreach (var pin in node.InputPins.Concat(node.EnterPins).SelectMany(_=>_.Links))
            {
                var d = EvaluateDepth(pin.From.Node);
                if (d >= node.Extra.Depth)
                    node.Extra.Depth = d + 1;
            }

            return node.Extra.Depth;
        }

        private void MarkClusterNodes(NodeHelper<LayoutInfo> node, int clusterId)
        {
            node.Extra.ClusterId = clusterId;
            foreach (var link in node.EnumerateLinks())
            {
                if (link.From.Node.Extra.ClusterId == 0)
                    MarkClusterNodes(link.From.Node, clusterId);
                else if (link.From.Node.Extra.ClusterId != clusterId)
                    throw new InvalidOperationException();

                if (link.To.Node.Extra.ClusterId == 0)
                    MarkClusterNodes(link.To.Node, clusterId);
                else if (link.To.Node.Extra.ClusterId != clusterId)
                    throw new InvalidOperationException();
            }
        }
    }
}