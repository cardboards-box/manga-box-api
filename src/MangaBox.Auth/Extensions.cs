using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MangaBox;

using Auth;

public static class Extensions
{
    public static IDependencyResolver AddOAuth(this IDependencyResolver builder)
    {
        return builder
            .Transient<ITokenService, TokenService>()
            .Transient<IOAuthService, OAuthService>()
            .AddServices(async (services, config) =>
            {
                var pars = await JwtKeyUtil.GetParameters(config);
                services
                    .AddAuthentication(opt =>
                    {
                        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(opt =>
                    {
                        opt.SaveToken = true;
                        opt.RequireHttpsMetadata = false;
                        opt.TokenValidationParameters = pars;
                    });
            });
    }

    public static string? Claim(this ClaimsPrincipal principal, string claim)
    {
        return principal?.FindFirst(claim)?.Value;
    }

    public static string? Claim(this ControllerBase ctrl, string claim)
    {
        if (ctrl.User == null) return null;
        return ctrl.User.Claim(claim);
    }

    public static TokenUser? UserFromIdentity(this ControllerBase ctrl)
    {
        if (ctrl.User == null) return null;

        return ctrl.User.UserFromIdentity();
    }

    public static TokenUser? UserFromIdentity(this ClaimsPrincipal principal)
    {
        if (principal == null) return null;

        var getClaim = (string key) => principal.Claim(key) ?? "";

        var id = getClaim(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id)) return null;

        return new TokenUser
        {
            Id = id,
            Nickname = getClaim(ClaimTypes.Name),
            Email = getClaim(ClaimTypes.Email),
            Avatar = getClaim(ClaimTypes.UserData),
            Provider = getClaim(ClaimTypes.PrimarySid),
            ProviderId = getClaim(ClaimTypes.PrimaryGroupSid)
        };
    }
}