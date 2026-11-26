using System.Text.Json.Serialization;
using Syncfusion.Maui.Toolkit.Charts;

namespace SmartCircularChart;

public sealed class GroupingResponse
{
    [JsonPropertyName("groupMode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PieGroupMode GroupMode { get; set; } = PieGroupMode.Value;

    // null means "no grouping".
    [JsonPropertyName("groupTo")]
    public double? GroupTo { get; set; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;
}
