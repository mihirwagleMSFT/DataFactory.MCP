using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Dataflow.Query;

/// <summary>
/// Detailed information about an Arrow column
/// </summary>
public class ArrowColumnDetails
{
    /// <summary>
    /// Column name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Column data type
    /// </summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Whether the column is nullable
    /// </summary>
    [JsonPropertyName("isNullable")]
    public bool IsNullable { get; set; }

    /// <summary>
    /// Column metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}