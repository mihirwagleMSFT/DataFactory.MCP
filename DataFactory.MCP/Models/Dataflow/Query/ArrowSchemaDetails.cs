using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Dataflow.Query;

/// <summary>
/// Detailed Arrow schema information
/// </summary>
public class ArrowSchemaDetails
{
    /// <summary>
    /// Number of fields in the schema
    /// </summary>
    [JsonPropertyName("fieldCount")]
    public int FieldCount { get; set; }

    /// <summary>
    /// Column information
    /// </summary>
    [JsonPropertyName("columns")]
    public List<ArrowColumnDetails>? Columns { get; set; }
}