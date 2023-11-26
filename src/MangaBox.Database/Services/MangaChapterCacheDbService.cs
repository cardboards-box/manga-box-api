namespace MangaBox.Database.Services;

public interface IMangaChapterCacheDbService : IOrmMap<DbMangaChapterCache> { }

internal class MangaChapterCacheDbService : Orm<DbMangaChapterCache>, IMangaChapterCacheDbService
{
    public MangaChapterCacheDbService(IOrmService orm) : base(orm) { }
}
