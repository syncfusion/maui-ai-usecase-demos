using System.Collections.ObjectModel;
using AIProcessDiagramDrafting.Models;
using Syncfusion.Maui.Diagram;

namespace AIProcessDiagramDrafting.Services;

public sealed class DiagramDraftBuilder
{
    public DiagramDraft Build(WorkflowResponse response)
    {
        var orderedNodes = response.Nodes.ToList();
        var nodeIndex = orderedNodes
            .Select((node, index) => new { node.Id, index })
            .ToDictionary(item => item.Id, item => item.index, StringComparer.Ordinal);

        var startNode = orderedNodes.First(node => node.Type == WorkflowNodeType.Start);
        var levels = BuildLevels(orderedNodes, response.Connectors, startNode.Id);

        var nodeSizes = orderedNodes.ToDictionary(node => node.Id, GetNodeSize, StringComparer.Ordinal);
        var layoutRows = levels
            .OrderBy(pair => pair.Value)
            .Select(pair => new
            {
                Nodes = pair
                    .Select(nodeId => orderedNodes.First((WorkflowNode node) => node.Id == nodeId))
                    .OrderBy((WorkflowNode node) => nodeIndex[node.Id])
                    .ToList()
            })
            .ToList();

        const double horizontalSpacing = 72;
        const double verticalSpacing = 132;
        const double leftMargin = 40;
        const double topMargin = 40;

        var rowWidths = layoutRows
            .Select(row => row.Nodes.Sum(node => nodeSizes[node.Id].Width) + Math.Max(0, row.Nodes.Count - 1) * horizontalSpacing)
            .ToList();

        var maxRowWidth = rowWidths.Count == 0 ? 0 : rowWidths.Max();

        var diagramNodes = new List<Node>(orderedNodes.Count);

        for (var rowIndex = 0; rowIndex < layoutRows.Count; rowIndex++)
        {
            var row = layoutRows[rowIndex];
            var rowWidth = rowWidths[rowIndex];
            var x = leftMargin + (maxRowWidth - rowWidth) / 2;
            var y = topMargin + rowIndex * verticalSpacing;

            foreach (var workflowNode in row.Nodes)
            {
                var size = nodeSizes[workflowNode.Id];
                var node = CreateNode(workflowNode, x + size.Width / 2, y + size.Height / 2, size.Width, size.Height);
                diagramNodes.Add(node);
                x += size.Width + horizontalSpacing;
            }
        }

        var diagramConnectors = new List<Connector>(response.Connectors.Count);
        foreach (var workflowConnector in response.Connectors)
        {
            var connector = new Connector
            {
                Id = workflowConnector.Id,
                SourceID = workflowConnector.SourceId,
                TargetID = workflowConnector.TargetId,
                Type = ConnectorSegmentType.Orthogonal
            };

            if (!string.IsNullOrWhiteSpace(workflowConnector.Label))
            {
                connector.Annotations = new ObservableCollection<PathAnnotation>
                {
                    new PathAnnotation(workflowConnector.Label)
                };
            }

            diagramConnectors.Add(connector);
        }

        return new DiagramDraft(diagramNodes, diagramConnectors);
    }

    private static Dictionary<string, int> BuildLevels(
        IReadOnlyList<WorkflowNode> orderedNodes,
        IEnumerable<WorkflowConnector> connectors,
        string startNodeId)
    {
        var levelByNodeId = orderedNodes.ToDictionary(node => node.Id, _ => int.MaxValue, StringComparer.Ordinal);
        var outgoingBySource = connectors
            .GroupBy(connector => connector.SourceId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        var queue = new Queue<string>();
        levelByNodeId[startNodeId] = 0;
        queue.Enqueue(startNodeId);

        while (queue.Count > 0)
        {
            var sourceId = queue.Dequeue();
            var nextLevel = levelByNodeId[sourceId] + 1;

            if (!outgoingBySource.TryGetValue(sourceId, out var outgoing))
            {
                continue;
            }

            foreach (var connector in outgoing)
            {
                if (levelByNodeId[connector.TargetId] > nextLevel)
                {
                    levelByNodeId[connector.TargetId] = nextLevel;
                    queue.Enqueue(connector.TargetId);
                }
            }
        }

        var fallbackLevel = levelByNodeId.Values.Where(level => level != int.MaxValue).DefaultIfEmpty(0).Max() + 1;
        foreach (var node in orderedNodes)
        {
            if (levelByNodeId[node.Id] == int.MaxValue)
            {
                levelByNodeId[node.Id] = fallbackLevel++;
            }
        }

        return levelByNodeId;
    }

    private static Node CreateNode(WorkflowNode workflowNode, double offsetX, double offsetY, double width, double height)
    {
        var node = new Node
        {
            Id = workflowNode.Id,
            OffsetX = offsetX,
            OffsetY = offsetY,
            Width = width,
            Height = height,
            Annotations = new ObservableCollection<ShapeAnnotation>
            {
                new ShapeAnnotation(workflowNode.Label)
            }
        };

        node.Shape = workflowNode.Type switch
        {
            WorkflowNodeType.Start => new FlowShape { Shape = NodeFlowShapes.Terminator },
            WorkflowNodeType.Process => new FlowShape { Shape = NodeFlowShapes.Process },
            WorkflowNodeType.Decision => new FlowShape { Shape = NodeFlowShapes.Decision },
            WorkflowNodeType.End => new FlowShape { Shape = NodeFlowShapes.Terminator },
            _ => new FlowShape { Shape = NodeFlowShapes.Process }
        };

        return node;
    }

    private static (double Width, double Height) GetNodeSize(WorkflowNode node)
        => node.Type switch
        {
            WorkflowNodeType.Start => (160, 64),
            WorkflowNodeType.Process => (220, 72),
            WorkflowNodeType.Decision => (200, 100),
            WorkflowNodeType.End => (160, 64),
            _ => (220, 72)
        };
}
