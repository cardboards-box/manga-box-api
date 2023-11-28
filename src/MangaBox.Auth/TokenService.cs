namespace MangaBox.Auth;

public interface ITokenService
{
    Task<TokenResult> ParseToken(string token);

    Task<string> GenerateToken(TokenResponse resp, params string[] roles);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public Task<TokenValidationParameters> GetParameters()
    {
        return JwtKeyUtil.GetParameters(_config);
    }

    public async Task<TokenResult> ParseToken(string token)
    {
        var validationParams = await GetParameters();

        var handler = new JwtSecurityTokenHandler();

        var principals = handler.ValidateToken(token, validationParams, out var securityToken);

        return new(principals, securityToken);
    }

    public async Task<string> GenerateToken(TokenResponse resp, params string[] roles)
    {
        var pars = await GetParameters();

        return new JwtToken(pars.IssuerSigningKey)
            .SetAudience(pars.ValidAudience)
            .SetIssuer(pars.ValidIssuer)
            .AddClaim(ClaimTypes.NameIdentifier, resp.User.Id)
            .AddClaim(ClaimTypes.Name, resp.User.Nickname)
            .AddClaim(ClaimTypes.Email, resp.User.Email)
            .AddClaim(ClaimTypes.UserData, resp.User.Avatar)
            .AddClaim(ClaimTypes.PrimarySid, resp.Provider)
            .AddClaim(ClaimTypes.PrimaryGroupSid, resp.User.ProviderId)
            .AddClaim(roles.Select(t => new Claim(ClaimTypes.Role, t)).ToArray())
            .Write();
    }
}

public record class TokenResult(ClaimsPrincipal Principal, SecurityToken Token);
