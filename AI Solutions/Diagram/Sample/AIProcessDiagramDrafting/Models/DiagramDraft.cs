using Syncfusion.Maui.Diagram;

namespace AIProcessDiagramDrafting.Models;

public sealed class DiagramDraft
{
    public DiagramDraft(IReadOnlyList<Node> nodes, IReadOnlyList<Connector> connectors)
    {
        Nodes = nodes;
        Connectors = connectors;
    }

    public IReadOnlyList<Node> Nodes { get; }

    public IReadOnlyList<Connector> Connectors { get; }
}
