using Apache.Arrow;
using Apache.Arrow.Ipc;
using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Models.Arrow;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace DataFactory.MCP.Services;

/// <summary>
/// Service for reading and parsing Apache Arrow data streams
/// </summary>
public class ArrowDataReaderService : IArrowDataReaderService
{
    private const int MaxSampleSize = 10;
    private const int BatchSampleSize = 5;
    private const int MaxEstimatedRows = 1000;
    private static readonly string[] CommonColumns = { "RoleInstance", "ProcessName", "Message", "Timestamp", "Level", "Id" };
    private static readonly string[] CommonErrorMessages = {
        "Invalid workload hostname",
        "DataSource requested unhandled application property",
        "A generic MashupException was caught",
        "Unable to create a provider context"
    };

    private readonly ILogger<ArrowDataReaderService> _logger;

    public ArrowDataReaderService(ILogger<ArrowDataReaderService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Reads Apache Arrow stream and extracts metadata and formatted data
    /// </summary>
    /// <param name="arrowData">The Apache Arrow binary data</param>
    /// <param name="returnAllData">If true, returns all data; if false, returns sample data only</param>
    /// <returns>Formatted Arrow data information</returns>
    public Task<ArrowDataInfo> ReadArrowStreamAsync(byte[] arrowData, bool returnAllData = false)
    {
        return Task.Run(() => ReadArrowStream(arrowData, returnAllData));
    }

    /// <summary>
    /// Reads Apache Arrow stream and extracts metadata and formatted data (synchronous version)
    /// </summary>
    /// <param name="arrowData">The Apache Arrow binary data</param>
    /// <param name="returnAllData">If true, returns all data; if false, returns sample data only</param>
    /// <returns>Formatted Arrow data information</returns>
    public ArrowDataInfo ReadArrowStream(byte[] arrowData, bool returnAllData = false)
    {
        try
        {
            _logger.LogDebug("Starting Arrow stream processing for {DataSize} bytes", arrowData.Length);
            return ProcessArrowStream(arrowData, returnAllData);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Arrow parsing failed, falling back to text extraction");
            return ExtractBasicInfo(arrowData, ex);
        }
    }

    private ArrowDataInfo ProcessArrowStream(byte[] arrowData, bool returnAllData)
    {
        using var stream = new MemoryStream(arrowData);
        using var reader = new ArrowStreamReader(stream);

        var info = new ArrowDataInfo
        {
            Schema = reader.Schema != null ? ExtractSchemaInfo(reader.Schema) : null,
            Success = true
        };

        var batches = ReadAllBatches(reader, info);
        _logger.LogDebug("Read {BatchCount} batches with total {TotalRows} rows", batches.Count, info.TotalRows);

        if (returnAllData)
            info.AllData = ExtractAllData(batches, info.Schema?.Columns);
        else
            info.SampleData = ExtractSampleData(batches, info.Schema?.Columns);

        info.BatchCount = batches.Count;
        return info;
    }

    private static List<RecordBatch> ReadAllBatches(ArrowStreamReader reader, ArrowDataInfo info)
    {
        var batches = new List<RecordBatch>();
        while (reader.ReadNextRecordBatch() is { } batch)
        {
            batches.Add(batch);
            info.TotalRows += batch.Length;
        }
        return batches;
    }

    private static ArrowSchemaInfo ExtractSchemaInfo(Schema schema) => new()
    {
        FieldCount = schema.FieldsList.Count,
        Columns = schema.FieldsList.Select(field => new ArrowColumnInfo
        {
            Name = field.Name,
            DataType = field.DataType.TypeId.ToString(),
            IsNullable = field.IsNullable,
            Metadata = field.Metadata?.Keys.ToDictionary(k => k, k => field.Metadata[k]) ?? []
        }).ToList()
    };

    private static Dictionary<string, List<object>> ExtractAllData(List<RecordBatch> batches, List<ArrowColumnInfo>? columns)
    {
        if (columns == null) return [];

        var allData = columns.ToDictionary(col => col.Name, _ => new List<object>());

        foreach (var batch in batches)
        {
            ExtractBatchData(batch, columns, allData, batch.Length);
        }

        return allData;
    }

    private static Dictionary<string, List<object>> ExtractSampleData(List<RecordBatch> batches, List<ArrowColumnInfo>? columns)
    {
        if (columns == null) return [];

        var sampleData = columns.ToDictionary(col => col.Name, _ => new List<object>());

        foreach (var batch in batches.Take(3))
        {
            var sampleCount = Math.Min(BatchSampleSize, batch.Length);
            ExtractBatchData(batch, columns, sampleData, sampleCount);

            // Limit total samples per column
            foreach (var col in columns)
            {
                if (sampleData[col.Name].Count > MaxSampleSize)
                {
                    sampleData[col.Name] = sampleData[col.Name].Take(MaxSampleSize).ToList();
                }
            }
        }

        return sampleData;
    }

    private static void ExtractBatchData(RecordBatch batch, List<ArrowColumnInfo> columns, Dictionary<string, List<object>> data, int rowLimit)
    {
        for (int colIndex = 0; colIndex < Math.Min(batch.ColumnCount, columns.Count); colIndex++)
        {
            var column = batch.Column(colIndex);
            var columnName = columns[colIndex].Name;

            for (int rowIndex = 0; rowIndex < Math.Min(rowLimit, column.Length); rowIndex++)
            {
                try
                {
                    var value = ExtractValueFromArray(column, rowIndex);
                    data[columnName].Add(value ?? "");
                }
                catch
                {
                    data[columnName].Add("");
                }
            }
        }
    }

    private static object? ExtractValueFromArray(IArrowArray array, int index) =>
        array.IsNull(index) ? null : array switch
        {
            StringArray str => str.GetString(index),
            Int32Array i32 => i32.GetValue(index),
            Int64Array i64 => i64.GetValue(index),
            DoubleArray dbl => dbl.GetValue(index),
            BooleanArray bln => bln.GetValue(index),
            TimestampArray ts => ts.GetTimestamp(index)?.ToString("yyyy-MM-dd HH:mm:ss"),
            Decimal128Array dec => dec.GetValue(index)?.ToString(),
            FloatArray flt => flt.GetValue(index),
            _ => $"[{array.GetType().Name}] - value at index {index}"
        };

    private ArrowDataInfo ExtractBasicInfo(byte[] arrowData, Exception? ex = null)
    {
        var info = new ArrowDataInfo
        {
            Success = false,
            Error = ex?.Message,
            Schema = new ArrowSchemaInfo { Columns = [] },
            SampleData = []
        };

        try
        {
            var content = Encoding.UTF8.GetString(arrowData);
            var detectedColumns = DetectColumnsFromText(content, info.Schema.Columns);
            ExtractSampleDataFromText(content, info.SampleData);

            info.Schema.FieldCount = detectedColumns.Count;
            info.TotalRows = EstimateRowCount(content);
        }
        catch (Exception extractEx)
        {
            info.Error = $"Arrow parsing failed: {ex?.Message}. Text extraction failed: {extractEx.Message}";
        }

        return info;
    }

    private static List<string> DetectColumnsFromText(string content, List<ArrowColumnInfo> columns)
    {
        var detected = new List<string>();

        foreach (var column in CommonColumns.Where(content.Contains))
        {
            detected.Add(column);
            columns.Add(new ArrowColumnInfo
            {
                Name = column,
                DataType = "String",
                IsNullable = true
            });
        }

        return detected;
    }

    private static void ExtractSampleDataFromText(string content, Dictionary<string, List<object>> sampleData)
    {
        ExtractPatternMatches(content, sampleData, "RoleInstance", @"vmback_\d+");
        ExtractPatternMatches(content, sampleData, "ProcessName", @"Microsoft\.Mashup\.Web\.[A-Za-z.]+");

        var foundMessages = CommonErrorMessages.Where(content.Contains).Take(5).ToList();
        if (foundMessages.Count != 0)
        {
            sampleData["Message"] = foundMessages.Cast<object>().ToList();
        }
    }

    private static void ExtractPatternMatches(string content, Dictionary<string, List<object>> sampleData, string key, string pattern)
    {
        var matches = Regex.Matches(content, pattern);
        if (matches.Count > 0)
        {
            sampleData[key] = matches.Cast<Match>()
                .Select(m => (object)m.Value)
                .Distinct()
                .Take(5)
                .ToList();
        }
    }

    private static int EstimateRowCount(string content)
    {
        var matches = Regex.Matches(content, @"vmback_\d+");
        return Math.Min(matches.Count, MaxEstimatedRows);
    }
}