using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace StaticWebAppAuthentication.Client;

public static class StaticWebAppAuthenticationExtensions
{
    public static IServiceCollection AddStaticWebsAuthentication(this IServiceCollection services) =>
        services
            .AddAuthorizationCore()
            .AddScoped<AuthenticationStateProvider, StaticWebAppsAuthenticationStateProvider>();
}
