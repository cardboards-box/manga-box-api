namespace MangaBox.Services;

public interface IMangaImportService
{
    IMangaSource[] Sources { get; }

    Task<long?> Import(string url);

    Task<bool> Refresh(string id);
}

internal class MangaImportService : IMangaImportService
{
    private readonly IImportService _import;
    private readonly IDbService _db;

    public IMangaSource[] Sources => _import.Sources;

    public MangaImportService(
        IImportService import, 
        IDbService db)
    {
        _import = import;
        _db = db;
    }

    public async Task<long?> Import(string url)
    {
        var res = await _import.Manga(url);
        if (res is null) return null;

        var (manga, chapters) = res;
        manga.Id = await _db.Manga.Upsert(manga);

        foreach (var chapter in chapters)
        {
            chapter.MangaId = manga.Id;
            chapter.Id = await _db.Chapters.Upsert(chapter);
        }

        await _db.Extended.UpdateComputed();
        return manga.Id;
    }

    public async Task<bool> Refresh(string id)
    {
        var manga = await _db.Manga.Fetch(id);
        if (manga == null) return false;

        var output = await Import(manga.Url);
        return output is not null;
    }
}
