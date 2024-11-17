using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using StaticWebAppAuthentication.Models;

namespace StaticWebAppAuthentication.Client;

public class StaticWebAppsAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient httpClient;

    public StaticWebAppsAuthenticationStateProvider(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));

        this.httpClient = httpClient;
    }

    private async Task<ClientPrincipal> GetClientPrincipal()
    {
        var data = await httpClient.GetFromJsonAsync<AuthenticationData>("/.auth/me");
        var clientPrincipal = data?.ClientPrincipal ?? new ClientPrincipal();
        return clientPrincipal;
    }

    public static ClaimsPrincipal GetClaimsFromClientClaimsPrincipal(ClientPrincipal principal)
    {
        principal.UserRoles = principal.UserRoles?.Except(["anonymous"], StringComparer.CurrentCultureIgnoreCase) ?? [];

        if (!principal.UserRoles.Any())
            return new ClaimsPrincipal();

        ClaimsIdentity identity = AdaptToClaimsIdentity(principal);

        return new ClaimsPrincipal(identity);
    }

    private static ClaimsIdentity AdaptToClaimsIdentity(ClientPrincipal principal)
    {
        var identity = new ClaimsIdentity(principal.IdentityProvider);
        
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
        identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
        identity.AddClaims(principal.UserRoles!.Select(r => new Claim(ClaimTypes.Role, r)));

        return identity;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var clientPrincipal = await GetClientPrincipal();
            var claimsPrincipal = GetClaimsFromClientClaimsPrincipal(clientPrincipal);
            return new AuthenticationState(claimsPrincipal);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }
}
