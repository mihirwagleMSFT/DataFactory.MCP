using Xunit;
using DataFactory.MCP.Tools;
using DataFactory.MCP.Tests.Infrastructure;

namespace DataFactory.MCP.Tests.Integration;

/// <summary>
/// Integration tests for AuthenticationTool that call the actual MCP tool methods
/// without mocking to verify real behavior
/// </summary>
public class AuthenticationToolIntegrationTests : IClassFixture<McpTestFixture>
{
    private readonly AuthenticationTool _authTool;
    private readonly McpTestFixture _fixture;

    public AuthenticationToolIntegrationTests(McpTestFixture fixture)
    {
        _fixture = fixture;
        _authTool = _fixture.GetService<AuthenticationTool>();
    }

    [Fact]
    public void GetAuthenticationStatus_ShouldReturnValidResponse()
    {
        // Act
        var result = _authTool.GetAuthenticationStatus();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Should contain either authentication status information or an error message
        Assert.Equal("Not authenticated. Please authenticate using interactive login or service principal.", result);
    }

    [Fact]
    public async Task GetAccessTokenAsync_WithoutAuthentication_ShouldReturnErrorMessage()
    {
        // Act
        var result = await _authTool.GetAccessTokenAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Should return an error since we're not authenticated
        Assert.Equal("No valid authentication found. Please authenticate first.", result);
    }

    [Fact]
    public async Task SignOutAsync_ShouldReturnValidResponse()
    {
        // Act
        var result = await _authTool.SignOutAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Should return either success message or error
        Assert.True(
            result.Contains("Successfully signed out user") ||
            result.Contains("No active authentication session found.")
        );
    }

    [Fact]
    public async Task AuthenticateServicePrincipalAsync_WithRealCredentials_ShouldSucceed()
    {
        // Arrange - using real service principal credentials from environment
        var realAppId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "#{AZURE_CLIENT_ID}#";
        var realSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? "#{AZURE_CLIENT_SECRET}#";
        var realTenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? "#{AZURE_TENANT_ID}#";

        // Act
        var result = await _authTool.AuthenticateServicePrincipalAsync(realAppId, realSecret, realTenantId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        Assert.Equal($"Service principal authentication completed successfully for application: {realAppId}", result);
    }

}
