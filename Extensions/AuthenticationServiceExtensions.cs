using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Services;
using DataFactory.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace DataFactory.MCP.Extensions;

/// <summary>
/// Extension methods for configuring authentication services
/// </summary>
public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Adds authentication services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddTransient<AuthenticationTool>();

        return services;
    }
}
