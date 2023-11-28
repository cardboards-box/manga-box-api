using Ionic.Zip;

namespace MangaBox.Services;

public interface IZipService
{
    Task<(MemoryStream stream, string name)?> Chapter(long id);
}

internal class ZipService : IZipService
{
    private readonly IDbService _db;
    private readonly IPageService _pages;
    private readonly IApiService _api;

    public ZipService(
        IDbService db, 
        IPageService pages, 
        IApiService api)
    {
        _db = db;
        _pages = pages;
        _api = api;
    }

    public async Task<(MemoryStream stream, string name)?> Chapter(long id)
    {
        var chapter = await _db.Chapters.Fetch(id);
        if (chapter == null) return null;

        var manga = await _db.Manga.Fetch(chapter.MangaId);
        if (manga == null) return null;

        var pages = await _pages.Get(chapter, false);
        if (pages.Length == 0) return null;

        using var zip = new ZipFile();

        for (var i = 0; i < pages.Length; i++)
        {
            var proxy = MangaHelper.ProxyUrlMangaPage(pages[i], referer: manga.Referer);
            var (stream, _, name, _) = await _api.GetData(proxy);
            zip.AddEntry($"{i}-{name}", stream);
        }

        var ms = new MemoryStream();
        zip.Save(ms);

        ms.Position = 0;
        return (ms, $"{manga.HashId}-{chapter.Ordinal}.zip");
    }
}
