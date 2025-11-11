namespace DataFactory.MCP.Models.Arrow;

/// <summary>
/// Information about an Arrow column
/// </summary>
public class ArrowColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}