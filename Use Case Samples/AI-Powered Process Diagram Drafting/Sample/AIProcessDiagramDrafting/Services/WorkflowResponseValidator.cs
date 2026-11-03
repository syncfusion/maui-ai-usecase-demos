using AIProcessDiagramDrafting.Models;

namespace AIProcessDiagramDrafting.Services;

public sealed class WorkflowResponseValidator
{
    public bool Validate(WorkflowResponse response, out string errorMessage)
    {
        if (response.Nodes.Count == 0)
        {
            errorMessage = "The workflow response does not contain any nodes.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
