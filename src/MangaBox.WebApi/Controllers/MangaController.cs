namespace MangaBox.WebApi.Controllers;

public class MangaController : BaseController
{
    private readonly IDbService _db;
    private readonly IMangaImportService _import;

    public MangaController(
        IDbService db,
        IMangaImportService import)
    {
        _db = db;
        _import = import;
    }

    [HttpGet, Route("manga"), Results<PaginatedResult<DbManga>>, Results(400)]
    public async Task<IActionResult> Get([FromQuery] int page = DEFAULT_PAGE, [FromQuery] int size = DEFAULT_SIZE)
    {
        if (!Validator
            .GreaterThan(page, "page", 0)
            .Between(size, "size", 0, 1001)
            .IsValid(out var res))
            return Do(res);

        var data = await _db.Manga.Paginate(page, size);
        return DoOk(data);
    }

    [HttpGet, Route("manga/random"), Results<DbManga[]>, Results(400)]
    public async Task<IActionResult> Random([FromQuery] int count = DEFAULT_SIZE)
    {
        if (!Validator
            .Between(count, "count", 0, 1001)
            .IsValid(out var res))
            return Do(res);

        return DoOk(await _db.Manga.GetByRandom(count));
    }

    [HttpGet, Route("manga/providers"), Results<MangaProvider[]>]
    public IActionResult Providers()
    {
        var providers = _import
            .Sources
            .Select(x => new MangaProvider
            {
                Name = x.Provider,
                Url = x.HomeUrl,
            }).ToArray();

        return DoOk(providers);
    }

    [HttpGet, Route("manga/import"), Results<long>, Results(404)]
    public async Task<IActionResult> Import([FromQuery] string url)
    {
        return DoPotNotFound(await _import.Import(url), "Manga");
    }

    [HttpGet, Route("manga/{id}"), Results<DbManga>, Results(404)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        return DoPotNotFound(await _db.Manga.Fetch(id));
    }

    [HttpGet, Route("manga/{id}/refresh"), Results, Results(404)]
    public async Task<IActionResult> Refresh([FromRoute] string id)
    {
        var res = await _import.Refresh(id);
        return res ? DoOk() : DoNotFound("Manga");
    }

    [HttpGet, Route("manga/{id}/favourite"), Results<bool>, Results(401), Results(404)]
    public async Task<IActionResult> Favourite([FromRoute] long id)
    {
        if (!IsLoggedIn(out var pid, out var err))
            return err;

        return DoPotNotFound(await _db.Favourites.Favourite(pid, id), "Manga");
    }

    [HttpPut, Route("manga"), AdminAuthorize, Results, Results(401)]
    public async Task<IActionResult> SetDisplayTitle([FromBody] MangaUpdateRequest req)
    {
        if (req.Title is not null)
            await _db.Manga.SetDisplayTitle(req.Id, 
                string.IsNullOrWhiteSpace(req.Title) ? null : req.Title.Trim());

        if (req.Reset is not null)
            await _db.Manga.SetOrdinalReset(req.Id, req.Reset.Value);
        
        return DoOk();
    }
}