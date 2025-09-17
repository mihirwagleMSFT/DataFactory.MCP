using Xunit;
using DataFactory.MCP.Tools;
using DataFactory.MCP.Tests.Infrastructure;
using DataFactory.MCP.Models;
using System.Text.Json;

namespace DataFactory.MCP.Tests.Integration;

/// <summary>
/// Integration tests for ConnectionsTool that call the actual MCP tool methods
/// without mocking to verify real behavior
/// </summary>
public class ConnectionsToolIntegrationTests : IClassFixture<McpTestFixture>
{
    private readonly ConnectionsTool _connectionsTool;
    private readonly McpTestFixture _fixture;

    public ConnectionsToolIntegrationTests(McpTestFixture fixture)
    {
        _fixture = fixture;
        _connectionsTool = _fixture.GetService<ConnectionsTool>();
    }

    private static void AssertAuthenticationError(string result)
    {
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains($"Authentication error: {ErrorMessages.AuthenticationRequired}", result);
    }

    [Fact]
    public async Task ListConnectionsAsync_WithoutAuthentication_ShouldReturnAuthenticationError()
    {
        // Act
        var result = await _connectionsTool.ListConnectionsAsync();

        // Assert
        AssertAuthenticationError(result);
    }

    [Fact]
    public async Task ListConnectionsAsync_WithContinuationToken_ShouldHandleTokenParameter()
    {
        // Arrange
        var testToken = "test-continuation-token";

        // Act
        var result = await _connectionsTool.ListConnectionsAsync(testToken);

        // Assert
        AssertAuthenticationError(result);
    }

    [Fact]
    public async Task GetConnectionAsync_WithoutAuthentication_ShouldReturnAuthenticationError()
    {
        // Arrange
        var testConnectionId = "test-connection-id";

        // Act
        var result = await _connectionsTool.GetConnectionAsync(testConnectionId);

        // Assert
        AssertAuthenticationError(result);
    }

    [Fact]
    public async Task GetConnectionAsync_WithEmptyConnectionId_ShouldReturnValidationError()
    {
        // Act
        var result = await _connectionsTool.GetConnectionAsync("");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Connection ID is required.", result);
    }

    [Fact]
    public async Task GetConnectionAsync_WithNullConnectionId_ShouldReturnValidationError()
    {
        // Act
        var result = await _connectionsTool.GetConnectionAsync(null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Connection ID is required.", result);
    }

    [Fact]
    public async Task GetConnectionAsync_WithWhitespaceConnectionId_ShouldReturnValidationError()
    {
        // Act
        var result = await _connectionsTool.GetConnectionAsync("   ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Connection ID is required.", result);
    }

    [Theory]
    [InlineData("6952a7b2-aea3-414f-9d85-6c0fe5d34539")]
    [InlineData("f6a39b76-9816-4e4b-b93a-f42e405017b7")]
    [InlineData("invalid-guid")]
    [InlineData("test-connection-123")]
    public async Task GetConnectionAsync_WithValidConnectionId_ShouldHandleIdParameter(string connectionId)
    {
        // Act
        var result = await _connectionsTool.GetConnectionAsync(connectionId);

        // Assert
        AssertAuthenticationError(result);
    }

    [Fact]
    public void ConnectionsTool_ShouldBeRegisteredInDI()
    {
        // Assert
        Assert.NotNull(_connectionsTool);
        Assert.IsType<ConnectionsTool>(_connectionsTool);
    }

    [Fact]
    public async Task ListConnectionsAsync_Result_ShouldNotBeJson_WhenUnauthenticated()
    {
        // Act
        var result = await _connectionsTool.ListConnectionsAsync();

        // Assert
        Assert.NotNull(result);

        // When there's an authentication error, the result should be a plain error message, not JSON
        var isValidJson = false;
        try
        {
            JsonDocument.Parse(result);
            isValidJson = true;
        }
        catch (JsonException)
        {
            // Expected - should not be valid JSON when unauthenticated
        }

        Assert.False(isValidJson, $"Expected plain error message but got JSON: {result}");
    }
}