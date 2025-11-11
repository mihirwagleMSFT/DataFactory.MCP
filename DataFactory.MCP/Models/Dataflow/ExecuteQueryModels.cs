using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models.Dataflow;

/// <summary>
/// Execute Dataflow Query request payload
/// </summary>
public class ExecuteDataflowQueryRequest
{
    /// <summary>
    /// The name of the query to execute
    /// </summary>
    [JsonPropertyName("QueryName")]
    [Required(ErrorMessage = "Query name is required")]
    public string QueryName { get; set; } = string.Empty;

    /// <summary>
    /// The custom mashup document containing the M query logic
    /// </summary>
    [JsonPropertyName("customMashupDocument")]
    [Required(ErrorMessage = "Custom mashup document is required")]
    public string CustomMashupDocument { get; set; } = string.Empty;
}

/// <summary>
/// Execute Dataflow Query response
/// </summary>
public class ExecuteDataflowQueryResponse
{
    /// <summary>
    /// The raw Apache Arrow binary data response
    /// </summary>
    [JsonPropertyName("data")]
    public byte[]? Data { get; set; }

    /// <summary>
    /// The content type of the response
    /// </summary>
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    /// <summary>
    /// The size of the response in bytes
    /// </summary>
    [JsonPropertyName("contentLength")]
    public long ContentLength { get; set; }

    /// <summary>
    /// Indicates if the query execution was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the execution failed
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Additional metadata about the query execution
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Summary of extracted readable content from the Arrow data (for display purposes)
    /// </summary>
    [JsonPropertyName("summary")]
    public QueryResultSummary? Summary { get; set; }
}

/// <summary>
/// Summary of query result data extracted from Apache Arrow format
/// </summary>
public class QueryResultSummary
{
    /// <summary>
    /// Detected column names in the result set
    /// </summary>
    [JsonPropertyName("columns")]
    public List<string>? Columns { get; set; }

    /// <summary>
    /// Sample values extracted from the data (limited for display)
    /// </summary>
    [JsonPropertyName("sampleData")]
    public Dictionary<string, List<string>>? SampleData { get; set; }

    /// <summary>
    /// Number of rows in the result (if determinable)
    /// </summary>
    [JsonPropertyName("estimatedRowCount")]
    public int? EstimatedRowCount { get; set; }

    /// <summary>
    /// Format description
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "Apache Arrow";
}