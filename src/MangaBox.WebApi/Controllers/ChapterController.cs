namespace MangaBox.WebApi.Controllers;

public class ChapterController : BaseController
{
    private readonly IDbService _db;
    private readonly IPageService _page;
    private readonly IZipService _zip;

    public ChapterController(
        IDbService db, 
        IPageService page,
        IZipService zip)
    {
        _db = db;
        _page = page;
        _zip = zip;
    }

    [HttpGet, Route("chapter/{id}"), Results<DbMangaChapter>, Results(404)]
    public async Task<IActionResult> Get([FromRoute] long id)
    {
        return DoPotNotFound(await _db.Chapters.Fetch(id), "Chapter");
    }

    [HttpGet, Route("chapter/{id}/pages"), Results<string[]>, Results(404)]
    public async Task<IActionResult> Pages([FromRoute] long id, [FromQuery] bool refresh = false)
    {
        var pages = await _page.Get(id, refresh);
        return pages.Length == 0 ? DoNotFound("Manga Pages") : DoOk(pages);
    }

    [HttpGet, Route("chapter/{id}/reset"), Results, Results(404)]
    public async Task<IActionResult> Reset([FromRoute] long id)
    {
        var pages = await _page.Get(id, true);
        return pages.Length == 0 ? DoNotFound("Chapter") : DoOk();
    }

    [HttpGet, Route("chapter/{id}/download")]
    public async Task<IActionResult> Download([FromRoute] long id)
    {
        var result = await _zip.Chapter(id);
        if (result == null) return NotFound();

        var (stream, name) = result.Value;
        return File(stream, "application/zip", name);
    }
}
