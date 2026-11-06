namespace AIProcessDiagramDrafting.Models;

public sealed class WorkflowExample
{
    public WorkflowExample(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; }

    public string Description { get; }

    public override string ToString() => Name;
}
