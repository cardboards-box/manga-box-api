namespace MangaBox.WebApi;

[ApiController]
public abstract class BaseController : ControllerBase
{
    public RequestValidator Validator => new();

    public string? PlatformId
    {
        get
        {
            var uid = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(uid) ? uid : null;
        }
    }

    [NonAction]
    public bool IsLoggedIn(out string id, out IActionResult errored)
    {
        var pid = PlatformId;
        id = pid ?? string.Empty;
        errored = Do(Requests.Unauthorized());
        return pid is not null;
    }

    [NonAction]
    public virtual IActionResult Do(RequestResult result)
    {
        return StatusCode((int)result.Code, result);
    }

    [NonAction]
    public virtual IActionResult DoOk() => Do(Requests.Ok());

    [NonAction]
    public virtual IActionResult DoOk<T>(T data) => Do(Requests.Ok(data));

    [NonAction]
    public virtual IActionResult DoUnauthorized(params string[] issues) => Do(Requests.Unauthorized(issues));

    [NonAction]
    public virtual IActionResult DoNotFound(params string[] resources) => Do(Requests.NotFound(resources));

    [NonAction]
    public virtual IActionResult DoBadRequest(params string[] issues) => Do(Requests.BadRequest(issues));

    [NonAction]
    public virtual IActionResult DoPotNotFound<T>(T? data, params string[] resources) => data is null ? DoNotFound(resources) : DoOk(data);
}

[Authorize]
public abstract class AuthedController : BaseController { }

[AdminAuthorize]
public abstract class AdminController : BaseController { }
