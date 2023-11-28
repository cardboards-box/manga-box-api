namespace MangaBox.WebApi.Controllers;

public class BookmarkController : AuthedController
{
    private readonly IDbService _db;

    public BookmarkController(IDbService db)
    {
        _db = db;
    }

    [HttpGet, Route("bookmark/{id}"), Results<DbMangaBookmark[]>, Results(401)]
    public async Task<IActionResult> Bookmarks([FromRoute] long id)
    {
        var bookmarks = await _db.Bookmarks.Bookmarks(id, PlatformId);
        return DoOk(bookmarks);
    }

    [HttpPut, Route("bookmark"), Results, Results(401)]
    public async Task<IActionResult> Bookmark([FromBody] BookmarkRequest req)
    {
        await _db.Bookmarks.Bookmark(req.MangaId, req.ChapterId, req.Pages, PlatformId);
        return DoOk();
    }
}

public record class BookmarkRequest(
    [property: JsonPropertyName("mangaId")] long MangaId,
    [property: JsonPropertyName("chapterId")] long ChapterId,
    [property: JsonPropertyName("pages")] int[] Pages);
