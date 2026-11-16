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
        cancellationToken.ThrowIfCancellationRequested();

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

        if (TryBuildBranchingWorkflow(sentences, normalizedDescription, out var branchingResponse))
        {
            return Task.FromResult(branchingResponse);
        }

        return Task.FromResult(BuildLinearWorkflow(sentences, normalizedDescription));
    }

    private WorkflowResponse BuildLinearWorkflow(IReadOnlyList<string> sentences, string description)
    {
        var steps = BuildSteps(sentences, description);
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

        return new WorkflowResponse
        {
            Title = CreateTitle(sentences, description),
            Nodes = nodes,
            Connectors = connectors,
            Warnings = CreateWarnings(false)
        };
    }

    private bool TryBuildBranchingWorkflow(IReadOnlyList<string> sentences, string description, out WorkflowResponse response)
    {
        var decisionIndex = FindDecisionIndex(sentences);
        if (decisionIndex < 0)
        {
            response = new WorkflowResponse();
            return false;
        }

        var decisionSentence = sentences[decisionIndex];
        var preambleSentences = sentences.Take(decisionIndex).ToList();
        var preambleSteps = preambleSentences.Count == 0 ? new List<WorkflowStep>() : BuildSteps(preambleSentences, description);
        var maxPreambleSteps = Math.Max(0, WorkflowLimits.Nodes - 5);
        if (preambleSteps.Count > maxPreambleSteps)
        {
            var truncatedSteps = preambleSteps.Take(maxPreambleSteps - 1).ToList();
            truncatedSteps.Add(new WorkflowStep(
                preambleSteps[maxPreambleSteps - 1].Type,
                string.Join(" ", preambleSteps.Skip(maxPreambleSteps - 1).Select(step => step.Label))));
            preambleSteps = truncatedSteps;
        }

        var yesAction = ExtractBranchAction(decisionSentence);
        var noAction = ExtractBranchAction(FindBranchSentence(sentences, decisionIndex));

        if (string.IsNullOrWhiteSpace(yesAction))
        {
            yesAction = "Continue along the primary path";
        }

        if (string.IsNullOrWhiteSpace(noAction))
        {
            noAction = "Continue along the alternate path";
        }

        var nodes = new List<WorkflowNode>
        {
            new() { Id = "N1", Type = WorkflowNodeType.Start, Label = "Start" }
        };
        var connectors = new List<WorkflowConnector>();
        var nextNodeId = 2;
        var previousNodeId = "N1";

        foreach (var step in preambleSteps)
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

        var decisionNodeId = $"N{nextNodeId++}";
        nodes.Add(new WorkflowNode
        {
            Id = decisionNodeId,
            Type = WorkflowNodeType.Decision,
            Label = ExtractDecisionLabel(decisionSentence)
        });
        connectors.Add(new WorkflowConnector
        {
            Id = $"C{connectors.Count + 1}",
            SourceId = previousNodeId,
            TargetId = decisionNodeId,
            Label = string.Empty
        });

        var yesNodeId = $"N{nextNodeId++}";
        nodes.Add(new WorkflowNode
        {
            Id = yesNodeId,
            Type = WorkflowNodeType.Process,
            Label = yesAction
        });
        connectors.Add(new WorkflowConnector
        {
            Id = $"C{connectors.Count + 1}",
            SourceId = decisionNodeId,
            TargetId = yesNodeId,
            Label = "Yes"
        });

        var noNodeId = $"N{nextNodeId++}";
        nodes.Add(new WorkflowNode
        {
            Id = noNodeId,
            Type = WorkflowNodeType.Process,
            Label = noAction
        });
        connectors.Add(new WorkflowConnector
        {
            Id = $"C{connectors.Count + 1}",
            SourceId = decisionNodeId,
            TargetId = noNodeId,
            Label = "No"
        });

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
            SourceId = yesNodeId,
            TargetId = endNodeId,
            Label = string.Empty
        });
        connectors.Add(new WorkflowConnector
        {
            Id = $"C{connectors.Count + 1}",
            SourceId = noNodeId,
            TargetId = endNodeId,
            Label = string.Empty
        });

        response = new WorkflowResponse
        {
            Title = CreateTitle(sentences, description),
            Nodes = nodes,
            Connectors = connectors,
            Warnings = CreateWarnings(true)
        };

        return true;
    }

    private static int FindDecisionIndex(IReadOnlyList<string> sentences)
    {
        for (var index = 0; index < sentences.Count; index++)
        {
            if (LooksLikeDecision(sentences[index]))
            {
                return index;
            }
        }

        return -1;
    }

    private static string FindBranchSentence(IReadOnlyList<string> sentences, int decisionIndex)
    {
        for (var index = decisionIndex + 1; index < sentences.Count; index++)
        {
            var sentence = sentences[index];
            if (ContainsKeyword(sentence, "otherwise") || ContainsKeyword(sentence, "else"))
            {
                return sentence;
            }
        }

        return decisionIndex + 1 < sentences.Count ? sentences[decisionIndex + 1] : string.Empty;
    }

    private List<WorkflowWarning> CreateWarnings(bool usedBranchingHeuristics)
    {
        if (!string.Equals(mode, "Warning", StringComparison.OrdinalIgnoreCase))
        {
            return new List<WorkflowWarning>();
        }

        return new List<WorkflowWarning>
        {
            new()
            {
                Message = usedBranchingHeuristics
                    ? "This offline fallback infers branch labels from the description. Review the conditional paths carefully."
                    : "This workflow was generated from the current description and should be reviewed for accuracy."
            }
        };
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
        return lower.Contains(" if ") || lower.StartsWith("if ") || lower.Contains(" otherwise") || lower.Contains(" else ") || lower.StartsWith("when ") || lower.Contains(" when ");
    }

    private static bool ContainsKeyword(string text, string keyword)
        => text.Contains($" {keyword} ", StringComparison.OrdinalIgnoreCase)
            || text.StartsWith($"{keyword} ", StringComparison.OrdinalIgnoreCase)
            || text.Contains($", {keyword} ", StringComparison.OrdinalIgnoreCase);

    private static string ExtractDecisionLabel(string sentence)
    {
        var label = sentence.Trim();
        if (label.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
        {
            label = label[3..].Trim();
        }

        label = label.TrimEnd('.', '!', '?', ';', ':');
        var commaIndex = label.IndexOf(',');
        if (commaIndex >= 0)
        {
            label = label[..commaIndex].Trim();
        }

        if (label.Length == 0)
        {
            return "Decision";
        }

        return label.EndsWith('?') ? label : $"{label}?";
    }

    private static string ExtractBranchAction(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
        {
            return string.Empty;
        }

        var label = sentence.Trim();
        if (label.StartsWith("otherwise", StringComparison.OrdinalIgnoreCase))
        {
            label = label["otherwise".Length..].TrimStart(' ', ',', ':', ';');
        }
        else if (label.StartsWith("else", StringComparison.OrdinalIgnoreCase))
        {
            label = label["else".Length..].TrimStart(' ', ',', ':', ';');
        }

        label = label.TrimEnd('.', '!', '?', ';', ':');

        var commaIndex = label.IndexOf(',');
        if (commaIndex >= 0)
        {
            label = label[(commaIndex + 1)..].Trim();
        }
        else
        {
            var thenIndex = label.IndexOf(" then ", StringComparison.OrdinalIgnoreCase);
            if (thenIndex >= 0)
            {
                label = label[(thenIndex + 6)..].Trim();
            }
        }

        return NormalizeLabel(label);
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
