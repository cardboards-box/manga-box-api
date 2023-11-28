namespace MangaBox.WebApi.Controllers;

public class ExtendedController : BaseController
{
    private readonly IDbService _db;

    public ExtendedController(IDbService db)
    {
        _db = db;
    }

    [HttpGet, Route("extended/{id}"), Results<MangaWithChapters>, Results(404)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var manga = await _db.WithChapters.Get(id, PlatformId);
        return DoPotNotFound(manga, "Manga");
    }
}
