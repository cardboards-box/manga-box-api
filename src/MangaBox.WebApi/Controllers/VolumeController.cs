namespace MangaBox.WebApi.Controllers;

public class VolumeController : BaseController
{
    private readonly IVolumeService _volume;

    public VolumeController(IVolumeService volume)
    {
        _volume = volume;
    }

    [HttpGet, Route("volume/{id}"), Results<MangaData>, Results(404), Results(401)]
    public async Task<IActionResult> Get([FromRoute] string id, [FromQuery] string? sort = null, [FromQuery] bool asc = true)
    {
        var actSort = ChapterSortColumn.Ordinal;
        if (Enum.TryParse<ChapterSortColumn>(sort, true, out var res))
            actSort = res;

        var vol = await _volume.Get(id, PlatformId, actSort, asc);
        return DoPotNotFound(vol, "Volume");
    }
}
