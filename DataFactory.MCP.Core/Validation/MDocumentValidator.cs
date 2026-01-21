using System.Text.RegularExpressions;

namespace DataFactory.MCP.Validation;

/// <summary>
/// Validates M (Power Query) section documents for syntax and structure.
/// </summary>
public class MDocumentValidator
{
    /// <summary>
    /// Validates an M document and returns the result.
    /// </summary>
    public MDocumentValidationResult Validate(string document)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var suggestions = new List<string>();

        var trimmed = document.Trim();

        // Check for Gen2 FastCopy pattern
        var isGen2 = trimmed.Contains("[StagingDefinition") && trimmed.Contains("FastCopy");

        // Check section declaration (may have attribute before it in Gen2)
        if (!trimmed.Contains("section "))
        {
            errors.Add("Document must contain a section declaration (e.g., 'section Section1;')");
        }
        else if (!Regex.IsMatch(trimmed, @"section\s+\w+\s*;"))
        {
            errors.Add("Section declaration must end with semicolon (e.g., 'section Section1;')");
        }

        // Check for shared queries
        if (!Regex.IsMatch(document, @"\bshared\s+", RegexOptions.IgnoreCase))
        {
            errors.Add("Document must contain at least one 'shared' query declaration");
        }

        // Check for balanced brackets
        ValidateBracketBalance(document, '(', ')', "parentheses", errors);
        ValidateBracketBalance(document, '{', '}', "braces", errors);
        ValidateBracketBalance(document, '[', ']', "square brackets", errors);

        // Check for let...in structure
        var letCount = Regex.Matches(document, @"\blet\b", RegexOptions.IgnoreCase).Count;
        var inCount = Regex.Matches(document, @"\bin\b", RegexOptions.IgnoreCase).Count;
        if (letCount != inCount)
        {
            warnings.Add($"Mismatched let/in keywords: {letCount} 'let', {inCount} 'in'. This may be intentional for simple expressions.");
        }

        // Pattern-specific validations
        if (isGen2)
        {
            ValidateGen2Pattern(document, warnings, suggestions);
        }
        else if (HasDestination(document))
        {
            ValidateGen1Pattern(document, suggestions);
        }

        return new MDocumentValidationResult
        {
            IsValid = errors.Count == 0,
            IsGen2 = isGen2,
            Errors = errors.ToArray(),
            Warnings = warnings.ToArray(),
            Suggestions = suggestions.ToArray()
        };
    }

    private static void ValidateBracketBalance(string document, char open, char close, string name, List<string> errors)
    {
        var openCount = document.Count(c => c == open);
        var closeCount = document.Count(c => c == close);
        if (openCount != closeCount)
        {
            errors.Add($"Unbalanced {name}: {openCount} opening, {closeCount} closing");
        }
    }

    private static bool HasDestination(string document)
    {
        return document.Contains("Lakehouse.Contents") || document.Contains("Warehouse.Contents");
    }

    private static void ValidateGen2Pattern(string document, List<string> warnings, List<string> suggestions)
    {
        if (!document.Contains("[DataDestinations"))
        {
            warnings.Add("Gen2 FastCopy pattern detected but no [DataDestinations] attribute found on source query");
        }
        if (!document.Contains("_DataDestination"))
        {
            suggestions.Add("Gen2 pattern typically uses '{QueryName}_DataDestination' naming convention for destination queries");
        }
        if (!document.Contains("HierarchicalNavigation"))
        {
            suggestions.Add("Gen2 Lakehouse destination should use: Lakehouse.Contents([HierarchicalNavigation = null, CreateNavigationProperties = false, EnableFolding = false])");
        }
    }

    private static void ValidateGen1Pattern(string document, List<string> suggestions)
    {
        if (!document.Contains("DefaultModelStorage") && !document.Contains("[DataDestinations"))
        {
            suggestions.Add("Consider using Gen2 FastCopy pattern with [StagingDefinition = [Kind = \"FastCopy\"]] or add 'DefaultModelStorage' query for Gen1 write operations");
        }
        if (!document.Contains("Pipeline.ExecuteAction") && !document.Contains("[DataDestinations"))
        {
            suggestions.Add("For Gen1: Add a write query using 'Pipeline.ExecuteAction'. For Gen2: Add [DataDestinations] attribute to source query");
        }
        if (!document.Contains("[Staging") && !document.Contains("[StagingDefinition"))
        {
            suggestions.Add("For Gen1: Write queries should have [Staging = \"DefaultModelStorage\"] attribute. For Gen2: Use [StagingDefinition = [Kind = \"FastCopy\"]] at section level");
        }
    }
}

/// <summary>
/// Result of M document validation.
/// </summary>
public class MDocumentValidationResult
{
    public bool IsValid { get; set; }
    public bool IsGen2 { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();
    public string[] Warnings { get; set; } = Array.Empty<string>();
    public string[] Suggestions { get; set; } = Array.Empty<string>();
}
