namespace MangaBox.WebApi.Controllers;

public class ProgressController : BaseController
{
    private readonly IDbService _db;
    private readonly IPageService _page;

    public ProgressController(IDbService db, IPageService page)
    {
        _db = db;
        _page = page;
    }

    [HttpGet, Route("progress/{id}"), Results<DbMangaProgress>, Results(404)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var res = await _db.Progress.Fetch(id, PlatformId);
        return DoPotNotFound(res, "Progress");
    }

    [HttpDelete, Route("progress/{id}"), Results, Results(404), Results(401)]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        if (!IsLoggedIn(out var pid, out var err))
            return err;

        var profile = await _db.Profiles.Fetch(pid);
        if (profile == null) return DoUnauthorized();

        var manga = await _db.Manga.Fetch(id);
        if (manga == null) return DoNotFound("Manga");

        await _db.Progress.DeleteByManga(profile.Id, manga.Id);
        return DoOk();
    }

    [HttpPut, Route("progress"), Results, Results(401), Authorize]
    public async Task<IActionResult> Put([FromBody] ProgressRequest req)
    {
        if (!IsLoggedIn(out var pid, out var err))
            return err;

        return Do(await _page.Progress(pid, req));
    }
}
