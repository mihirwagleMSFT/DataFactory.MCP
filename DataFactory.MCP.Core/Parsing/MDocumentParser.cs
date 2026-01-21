using System.Text.RegularExpressions;

namespace DataFactory.MCP.Parsing;

/// <summary>
/// Parses M (Power Query) section documents to extract queries.
/// </summary>
public class MDocumentParser
{
    /// <summary>
    /// Parses an M document and extracts all shared queries.
    /// </summary>
    public List<ParsedQuery> ParseQueries(string document)
    {
        var queries = new List<ParsedQuery>();

        // Pattern to match: optional [Attribute] followed by shared QueryName = ... ;
        // This handles both simple names and #"quoted names"
        var pattern = @"(?:(\[[^\]]+\])\s*)?\bshared\s+(#""[^""]+""|\w+)\s*=\s*";
        var matches = Regex.Matches(document, pattern, RegexOptions.IgnoreCase);

        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var attribute = match.Groups[1].Success ? match.Groups[1].Value.Trim() : "";
            var queryName = match.Groups[2].Value;

            // Remove #"" wrapper if present for the name
            if (queryName.StartsWith("#\"") && queryName.EndsWith("\""))
            {
                queryName = queryName.Substring(2, queryName.Length - 3);
            }

            // Find the end of this query (next shared or end of document)
            var startIndex = match.Index + match.Length;
            int endIndex;

            if (i + 1 < matches.Count)
            {
                // Find the position just before the next attribute or 'shared'
                endIndex = matches[i + 1].Index;
                // Walk back to find the semicolon
                var searchArea = document.Substring(startIndex, endIndex - startIndex);
                var lastSemicolon = searchArea.LastIndexOf(';');
                if (lastSemicolon >= 0)
                {
                    endIndex = startIndex + lastSemicolon + 1;
                }
            }
            else
            {
                endIndex = document.Length;
            }

            var queryCode = document.Substring(startIndex, endIndex - startIndex).Trim();
            // Remove trailing semicolon for the code
            if (queryCode.EndsWith(";"))
            {
                queryCode = queryCode.Substring(0, queryCode.Length - 1).Trim();
            }

            queries.Add(new ParsedQuery
            {
                Name = queryName,
                Code = queryCode,
                Attribute = attribute
            });
        }

        return queries;
    }

    /// <summary>
    /// Extracts a table name from a user requirement string.
    /// </summary>
    public string? ExtractTableName(string text)
    {
        // Try to extract a table name from common patterns
        var patterns = new[]
        {
            @"(?:to|into|save to|load to|write to)\s+['""]?(\w+)['""]?",
            @"(\w+)\s+table",
            @"table\s+['""]?(\w+)['""]?"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups[1].Success)
            {
                return match.Groups[1].Value;
            }
        }

        return null;
    }
}

/// <summary>
/// Represents a parsed query from an M document.
/// </summary>
public class ParsedQuery
{
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Attribute { get; set; } = "";
}
