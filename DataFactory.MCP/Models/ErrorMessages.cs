namespace DataFactory.MCP.Models;

/// <summary>
/// Common error messages used throughout the application
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// Authentication error message when no valid token is available
    /// </summary>
    public const string AuthenticationRequired = "Valid authentication token is required. Please authenticate first.";

    /// <summary>
    /// Authentication error message for invalid token format
    /// </summary>
    public const string InvalidTokenFormat = "Invalid access token format.";

    /// <summary>
    /// Authentication error message when no authentication is found
    /// </summary>
    public const string NoAuthenticationFound = "No valid authentication found. Please authenticate first.";
}