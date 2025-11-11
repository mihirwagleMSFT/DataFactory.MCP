namespace DataFactory.MCP.Models.Arrow;

/// <summary>
/// Apache Arrow schema information
/// </summary>
public class ArrowSchemaInfo
{
    public int FieldCount { get; set; }
    public List<ArrowColumnInfo>? Columns { get; set; }
}