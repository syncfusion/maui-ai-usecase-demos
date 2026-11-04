using AIProcessDiagramDrafting.Models;

namespace AIProcessDiagramDrafting.Services;

public sealed class WorkflowResponseValidator
{
    public bool Validate(WorkflowResponse response, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(response.Title))
        {
            errorMessage = "The workflow response does not include a title.";
            return false;
        }

        if (response.Nodes.Count == 0)
        {
            errorMessage = "The workflow response does not contain any nodes.";
            return false;
        }

        if (response.Nodes.Count > WorkflowLimits.Nodes)
        {
            errorMessage = $"The workflow response exceeds the node limit of {WorkflowLimits.Nodes}.";
            return false;
        }

        if (response.Connectors.Count > WorkflowLimits.Connectors)
        {
            errorMessage = $"The workflow response exceeds the connector limit of {WorkflowLimits.Connectors}.";
            return false;
        }

        var nodeIds = new HashSet<string>(StringComparer.Ordinal);
        var startNodeCount = 0;
        var endNodeCount = 0;
        var decisionNodeCount = 0;

        foreach (var node in response.Nodes)
        {
            if (string.IsNullOrWhiteSpace(node.Id))
            {
                errorMessage = "Every node must have an ID.";
                return false;
            }

            if (!nodeIds.Add(node.Id))
            {
                errorMessage = "Node IDs must be unique.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(node.Label))
            {
                errorMessage = $"Node '{node.Id}' must have a label.";
                return false;
            }

            if (node.Label.Length > WorkflowLimits.NodeLabel)
            {
                errorMessage = $"Node '{node.Id}' exceeds the label limit of {WorkflowLimits.NodeLabel} characters.";
                return false;
            }

            switch (node.Type)
            {
                case WorkflowNodeType.Start:
                    startNodeCount++;
                    break;
                case WorkflowNodeType.Process:
                    break;
                case WorkflowNodeType.Decision:
                    decisionNodeCount++;
                    break;
                case WorkflowNodeType.End:
                    endNodeCount++;
                    break;
                default:
                    errorMessage = $"Node '{node.Id}' uses an unsupported type.";
                    return false;
            }
        }

        if (startNodeCount != 1)
        {
            errorMessage = "The workflow response must contain exactly one Start node.";
            return false;
        }

        if (endNodeCount == 0)
        {
            errorMessage = "The workflow response must contain at least one End node.";
            return false;
        }

        if (decisionNodeCount > WorkflowLimits.DecisionNodes)
        {
            errorMessage = $"The workflow response exceeds the decision node limit of {WorkflowLimits.DecisionNodes}.";
            return false;
        }

        var connectorIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var connector in response.Connectors)
        {
            if (string.IsNullOrWhiteSpace(connector.Id))
            {
                errorMessage = "Every connector must have an ID.";
                return false;
            }

            if (!connectorIds.Add(connector.Id))
            {
                errorMessage = "Connector IDs must be unique.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(connector.SourceId) || string.IsNullOrWhiteSpace(connector.TargetId))
            {
                errorMessage = $"Connector '{connector.Id}' must reference valid source and target nodes.";
                return false;
            }

            if (!nodeIds.Contains(connector.SourceId) || !nodeIds.Contains(connector.TargetId))
            {
                errorMessage = $"Connector '{connector.Id}' references a missing node.";
                return false;
            }

            if (connector.SourceId == connector.TargetId)
            {
                errorMessage = $"Connector '{connector.Id}' cannot connect a node to itself.";
                return false;
            }

            if (connector.Label.Length > WorkflowLimits.ConnectorLabel)
            {
                errorMessage = $"Connector '{connector.Id}' exceeds the label limit of {WorkflowLimits.ConnectorLabel} characters.";
                return false;
            }
        }

        errorMessage = string.Empty;
        return true;
    }
}
