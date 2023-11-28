namespace MangaBox.WebApi.Controllers;

public class SearchController : BaseController
{
    private readonly IDbService _db;
    private readonly IReverseSearchService _reverse;

    public SearchController(
        IDbService db, 
        IReverseSearchService reverse)
    {
        _db = db;
        _reverse = reverse;
    }

    [HttpPost, Route("search"), Results<PaginatedResult<MangaExtended>>]
    public async Task<IActionResult> Search([FromBody] MangaFilter filter)
    {
        var data = await _db.Search.Search(filter, PlatformId);
        return DoOk(data);
    }

    [HttpGet, Route("search/{id}"), Results<MangaExtended>, Results(404)]
    public async Task<IActionResult> GetExtended([FromRoute] string id)
    {
        var res = await _db.Extended.Fetch(id, PlatformId);
        return DoPotNotFound(res, "Progress Extended");
    }

    [HttpGet, Route("search/touched"), Results<PaginatedResult<MangaExtended>>]
    public async Task<IActionResult> Touched([FromQuery] int page = DEFAULT_PAGE, 
        [FromQuery] int size = DEFAULT_SIZE, [FromQuery] string? type = null)
    {
        if (!Validator
            .GreaterThan(page, "page", 0)
            .Between(size, "size", 0, 1000)
            .IsValid(out var res))
            return Do(res);

        if (!Enum.TryParse<TouchedState>(type, true, out var touchedType))
            touchedType = TouchedState.All;

        var search = new MangaFilter
        {
            Page = page,
            Size = size,
            State = touchedType
        };

        var data = await _db.Search.Search(search, PlatformId);
        return DoOk(data);
    }

    [HttpGet, Route("search/filters"), Results<Filter[]>]
    public async Task<IActionResult> Filters()
    {
        var data = await _db.Search.Filters();
        return DoOk(data);
    }

    [HttpPost, Route("search/image"), Results<ImageSearchResults>]
    public async Task<IActionResult> ImageSearch(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var lookup = await _reverse.Search(ms, file.FileName);
        return Ok(lookup);
    }

    [HttpGet, Route("search/image"), Results<ImageSearchResults>]
    public async Task<IActionResult> ImageSearch([FromQuery] string path)
    {
        var lookup = await _reverse.Search(path);
        return Ok(lookup);
    }

    [HttpGet, Route("search/since/{date}"), Results<PaginatedResult<MangaExtended>>]
    public async Task<IActionResult> Since([FromRoute] DateTime date, 
        [FromQuery] int page = DEFAULT_PAGE, [FromQuery] int size = DEFAULT_SIZE)
    {
        if (!Validator
            .GreaterThan(page, "page", 0)
            .Between(size, "size", 0, 1000)
            .IsValid(out var res))
            return Do(res);

        var data = await _db.Extended.Since(PlatformId, date, page, size);
        return DoOk(data);
    }
}
