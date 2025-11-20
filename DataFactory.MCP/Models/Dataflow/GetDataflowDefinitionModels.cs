using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Dataflow;

/// <summary>
/// Decoded dataflow definition for easier access
/// </summary>
public class DecodedDataflowDefinition
{
    /// <summary>
    /// Decoded queryMetadata.json content as structured object
    /// </summary>
    public JsonElement? QueryMetadata { get; set; }

    /// <summary>
    /// Decoded mashup.pq content (Power Query M code)
    /// </summary>
    public string? MashupQuery { get; set; }

    /// <summary>
    /// Decoded .platform content as structured object
    /// </summary>
    public JsonElement? PlatformMetadata { get; set; }

    /// <summary>
    /// Raw definition parts (Base64 encoded)
    /// </summary>
    public List<DataflowDefinitionPart> RawParts { get; set; } = new();
}

/// <summary>
/// Request model for updating dataflow definition
/// </summary>
public class UpdateDataflowDefinitionRequest
{
    /// <summary>
    /// The definition to update
    /// </summary>
    [JsonPropertyName("definition")]
    public DataflowDefinition Definition { get; set; } = new();
}

/// <summary>
/// Response model for dataflow definition update operations
/// </summary>
public class UpdateDataflowDefinitionResponse
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The updated dataflow ID
    /// </summary>
    public string? DataflowId { get; set; }

    /// <summary>
    /// The workspace ID where the dataflow was updated
    /// </summary>
    public string? WorkspaceId { get; set; }
}