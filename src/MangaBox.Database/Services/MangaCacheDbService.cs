namespace MangaBox.Database.Services;

public interface IMangaCacheDbService : IOrmMap<DbMangaCache>
{
    Task<DbMangaCache[]> ByIds(string[] mangaIds);

    Task<DbMangaCache[]> BadCoverArt();
}

internal class MangaCacheDbService : Orm<DbMangaCache>, IMangaCacheDbService
{
    public MangaCacheDbService(IOrmService orm) : base(orm) { }

    public Task<DbMangaCache[]> ByIds(string[] mangaIds)
    {
        const string QUERY = @"SELECT
	DISTINCT
	*
FROM manga_cache
WHERE source_id = ANY(:mangaIds)";
        return Get(QUERY, new { mangaIds });
    }

    public Task<DbMangaCache[]> BadCoverArt()
    {
        const string QUERY = "SELECT * FROM manga_cache WHERE cover LIKE '%/';";
        return Get(QUERY);
    }
}
