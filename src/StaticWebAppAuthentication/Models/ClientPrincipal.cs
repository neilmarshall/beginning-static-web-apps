namespace StaticWebAppAuthentication.Models;

public record class ClientPrincipal
{
    public string IdentityProvider { get; set;} = string.Empty;
    public string UserId { get; set;} = string.Empty;
    public string UserDetails { get; set;} = string.Empty;
    public IEnumerable<string> UserRoles { get; set;} = Enumerable.Empty<string>();
}
