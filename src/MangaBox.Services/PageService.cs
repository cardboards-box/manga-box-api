namespace MangaBox.Services;

public interface IPageService
{
    Task<string[]> Get(long chapterId, bool refetch);
    Task<string[]> Get(DbMangaChapter? chapter, bool refetch);
    Task<string[]> Get(DbMangaChapter chapter, DbManga manga, bool refetch);
    Task<RequestResult> Progress(string platformId, ProgressRequest req);
}

internal class PageService : IPageService
{
    private readonly IImportService _import;
    private readonly IDbService _db;

    public PageService(IImportService import, IDbService db)
    {
        _import = import;
        _db = db;
    }

    public async Task<string[]> Get(long chapterId, bool refetch)
    {
        var chapter = await _db.Chapters.Fetch(chapterId);
        return await Get(chapter, refetch);
    }

    public async Task<string[]> Get(DbMangaChapter? chapter, bool refetch)
    {
        if (chapter == null) return Array.Empty<string>();
        if (chapter.Pages.Length > 0 && !refetch) return chapter.Pages;

        var manga = await _db.Manga.Fetch(chapter.MangaId);
        if (manga == null) return Array.Empty<string>();

        return await Get(chapter, manga, refetch);
    }

    public async Task<string[]> Get(DbMangaChapter chapter, DbManga manga, bool refetch)
    {
        if (chapter.Pages.Length > 0 && !refetch) return chapter.Pages;

        var pages = await _import.Pages(manga, chapter.Url);
        if (pages == null) return Array.Empty<string>();

        await _db.Chapters.SetPages(chapter.Id, pages);
        await _db.Extended.UpdateComputed();
        return pages;
    }

    public async Task<RequestResult> Progress(string platformId, ProgressRequest req)
    {
        var profile = await _db.Profiles.Fetch(platformId);
        if (profile is null) return Requests.Unauthorized();

        var (mid, chapters, page) = req;
        if (page is null)
        {
            chapters ??= Array.Empty<long>();
            var worked = await Read(mid.ToString(), profile, chapters);
            return worked ? Requests.Ok() : Requests.NotFound("Manga");
        }

        if (chapters is null || chapters.Length != 1)
            return Requests.BadRequest("Progress can only be set for 1 chapter and page at a time");

        await _db.Progress.Upsert(new DbMangaProgress
        {
            ProfileId = profile.Id,
            MangaId = mid,
            MangaChapterId = chapters[0],
            PageIndex = page.Value
        });
        return Requests.Ok();
    }

    public async Task<bool> Read(string id, DbProfile profile, params long[] chapters)
    {
        async Task<bool> DoUpdate(DbMangaProgress progress)
        {
            if (progress.Id == -1)
            {
                var res = await _db.Progress.Insert(progress) > 0;
                await _db.Extended.UpdateComputed();
                return res;
            }

            await _db.Progress.Update(progress);
            await _db.Extended.UpdateComputed();
            return true;
        }

        var manga = await _db.WithChapters.Get(id, profile.PlatformId);
        if (manga == null) return false;

        var progress = await _db.Progress.Fetch(id, profile.PlatformId) ?? new DbMangaProgress
        {
            Id = -1,
            ProfileId = profile.Id,
            MangaId = manga.Manga.Id
        };

        //Toggle full list of chapters
        if (chapters.Length == 0)
        {
            progress.Read = progress.Read.Length == 0 ? manga.Chapters.Select(t => new DbMangaChapterProgress
            {
                ChapterId = t.Id,
                PageIndex = t.Pages.Length - 1
            }).ToArray() : Array.Empty<DbMangaChapterProgress>();

            return await DoUpdate(progress);
        }

        //Toggle specific chapters
        var read = progress.Read.ToList();
        foreach (var chapter in chapters)
        {
            var chap = manga.Chapters.FirstOrDefault(t => t.Id == chapter);
            if (chap == null) continue;

            var index = chap.Pages.Length - 1 < 0 ? 0 : chap.Pages.Length - 1;

            var exists = read.FirstOrDefault(t => t.ChapterId == chapter);
            if (exists == null)
            {
                read.Add(new DbMangaChapterProgress(chap.Id, index));
                continue;
            }

            read.Remove(exists);
        }

        progress.Read = read.ToArray();
        return await DoUpdate(progress);
    }
}

public record class ProgressRequest(
    [property: JsonPropertyName("mangaId")] long MangaId,
    [property: JsonPropertyName("chapters")] long[]? Chapters,
    [property: JsonPropertyName("page")] int? Page);