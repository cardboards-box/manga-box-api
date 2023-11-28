namespace MangaBox.WebApi.Controllers;

public class AuthController : BaseController
{
    private readonly IOAuthService _auth;
    private readonly ITokenService _token;
    private readonly IDbService _db;

    public AuthController(
        IOAuthService auth,
        ITokenService token,
        IDbService db)
    {
        _auth = auth;
        _token = token;
        _db = db;
    }

    [HttpGet, Route("auth/{code}")]
    [Results<AuthUserResponse>, Results(401)]
    public async Task<IActionResult> Auth(string code)
    {
        var res = await _auth.ResolveCode(code);
        if (res == null || !string.IsNullOrEmpty(res.Error))
            return DoUnauthorized("Invalid Code");

        var profile = new DbProfile
        {
            Avatar = res.User.Avatar,
            Email = res.User.Email,
            PlatformId = res.User.Id,
            Username = res.User.Nickname,
            Provider = res.User.Provider,
            ProviderId = res.User.ProviderId,
        };
        await _db.Profiles.Upsert(profile);

        profile = await _db.Profiles.Fetch(res.User.Id);

        var roles = profile!.Admin ? new[] { "Admin" } : Array.Empty<string>();
        var token = await _token.GenerateToken(res, roles);

        var user = (AuthUserResponse.UserData)profile;

        return DoOk(new AuthUserResponse
        {
            User = user,
            Id = profile.Id,
            Token = token,
        });
    }

    [HttpGet, Route("auth"), Authorize]
    [Results<AuthUserResponse.UserData>, Results(401), Results(404)]
    public async Task<IActionResult> Me()
    {
        var user = this.UserFromIdentity();
        if (user == null) return DoUnauthorized();

        var profile = await _db.Profiles.Fetch(user.Id);
        if (profile == null) return DoNotFound();

        var data = (AuthUserResponse.UserData)profile;
        return DoOk(data);
    }

    [HttpPost, Route("auth/settings"), Authorize, Results, Results(401)]
    public async Task<IActionResult> Settings([FromBody] SettingsRequest request)
    {
        if (!IsLoggedIn(out var uid, out var res)) return res;

        await _db.Profiles.UpdateSettings(uid, 
            string.IsNullOrWhiteSpace(request.Settings) 
                ? "{}" : request.Settings);
        return DoOk();
    }
}