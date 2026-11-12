using System.Text.RegularExpressions;
using AIProcessDiagramDrafting.Models;

namespace AIProcessDiagramDrafting.Services;

public sealed class FakeWorkflowExtractionService : IWorkflowExtractionService
{
    private readonly string mode;

    public FakeWorkflowExtractionService(string? mode = null)
    {
        this.mode = string.IsNullOrWhiteSpace(mode) ? "Warning" : mode.Trim();
    }

    public Task<WorkflowResponse> ExtractAsync(string processDescription, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processDescription);

        if (string.Equals(mode, "Failure", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The fake workflow service is configured to fail.");
        }

        if (string.Equals(mode, "InvalidGraph", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new WorkflowResponse
            {
                Title = "Invalid draft",
                Nodes = new List<WorkflowNode>
                {
                    new() { Id = "N1", Type = WorkflowNodeType.Start, Label = "Start" },
                    new() { Id = "N1", Type = WorkflowNodeType.Process, Label = "Duplicate node id" },
                    new() { Id = "N2", Type = WorkflowNodeType.End, Label = "End" }
                },
                Connectors = new List<WorkflowConnector>
                {
                    new() { Id = "C1", SourceId = "N1", TargetId = "N2", Label = string.Empty },
                    new() { Id = "C1", SourceId = "N1", TargetId = "N1", Label = string.Empty }
                }
            });
        }

        var normalizedDescription = NormalizeWhitespace(processDescription);
        var sentences = SplitSentences(normalizedDescription);
        var steps = BuildSteps(sentences, normalizedDescription);
        var nodes = new List<WorkflowNode>
        {
            new() { Id = "N1", Type = WorkflowNodeType.Start, Label = "Start" }
        };
        var connectors = new List<WorkflowConnector>();
        var nextNodeId = 2;
        var previousNodeId = "N1";

        foreach (var step in steps)
        {
            var nodeId = $"N{nextNodeId++}";
            nodes.Add(new WorkflowNode
            {
                Id = nodeId,
                Type = step.Type,
                Label = step.Label
            });

            connectors.Add(new WorkflowConnector
            {
                Id = $"C{connectors.Count + 1}",
                SourceId = previousNodeId,
                TargetId = nodeId,
                Label = string.Empty
            });

            previousNodeId = nodeId;
        }

        var endNodeId = $"N{nextNodeId}";
        nodes.Add(new WorkflowNode
        {
            Id = endNodeId,
            Type = WorkflowNodeType.End,
            Label = "End"
        });
        connectors.Add(new WorkflowConnector
        {
            Id = $"C{connectors.Count + 1}",
            SourceId = previousNodeId,
            TargetId = endNodeId,
            Label = string.Empty
        });

        var warnings = string.Equals(mode, "Warning", StringComparison.OrdinalIgnoreCase)
            ? new List<WorkflowWarning>
            {
                new() { Message = "This workflow was generated from the current description and should be reviewed for accuracy." }
            }
            : new List<WorkflowWarning>();

        return Task.FromResult(new WorkflowResponse
        {
            Title = CreateTitle(sentences, normalizedDescription),
            Nodes = nodes,
            Connectors = connectors,
            Warnings = warnings
        });
    }

    private static string NormalizeWhitespace(string text)
        => Regex.Replace(text.Trim(), @"\s+", " ");

    private static List<string> SplitSentences(string text)
    {
        var parts = Regex.Split(text, @"(?<=[.!?])\s+|\r?\n+");
        var sentences = new List<string>();

        foreach (var part in parts)
        {
            var sentence = part.Trim().TrimEnd('.', '!', '?', ';', ':');
            if (!string.IsNullOrWhiteSpace(sentence))
            {
                sentences.Add(sentence);
            }
        }

        if (sentences.Count == 0)
        {
            sentences.Add(text);
        }

        return sentences;
    }

    private static List<WorkflowStep> BuildSteps(IReadOnlyList<string> sentences, string description)
    {
        var source = sentences.ToList();
        var maxStepCount = WorkflowLimits.Nodes - 2;

        if (source.Count > maxStepCount)
        {
            var truncated = source.Take(maxStepCount - 1).ToList();
            truncated.Add(string.Join(" ", source.Skip(maxStepCount - 1)));
            source = truncated;
        }

        var steps = new List<WorkflowStep>();
        var decisionCount = 0;
        foreach (var sentence in source)
        {
            var label = NormalizeLabel(sentence);
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            var isDecision = LooksLikeDecision(label) && decisionCount < WorkflowLimits.DecisionNodes;
            if (isDecision)
            {
                decisionCount++;
            }

            var type = isDecision ? WorkflowNodeType.Decision : WorkflowNodeType.Process;
            steps.Add(new WorkflowStep(type, label));
        }

        if (steps.Count == 0)
        {
            steps.Add(new WorkflowStep(WorkflowNodeType.Process, NormalizeLabel(description)));
        }

        return steps;
    }

    private static bool LooksLikeDecision(string text)
    {
        var lower = text.ToLowerInvariant();
        return lower.Contains(" if ") || lower.StartsWith("if ") || lower.Contains(" otherwise") || lower.Contains(" else ");
    }

    private static string CreateTitle(IReadOnlyList<string> sentences, string description)
    {
        var title = sentences.Count > 0 ? sentences[0] : description;
        title = NormalizeLabel(title);

        if (title.Length > 48)
        {
            title = title[..45].TrimEnd() + "...";
        }

        return string.IsNullOrWhiteSpace(title) ? "Generated workflow" : title;
    }

    private static string NormalizeLabel(string text)
    {
        var label = Regex.Replace(text, @"\s+", " ").Trim();
        if (label.Length <= WorkflowLimits.NodeLabel)
        {
            return label;
        }

        return label[..(WorkflowLimits.NodeLabel - 3)].TrimEnd() + "...";
    }

    private sealed record WorkflowStep(WorkflowNodeType Type, string Label);
}
