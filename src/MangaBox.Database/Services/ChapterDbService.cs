namespace MangaBox.Database.Services;

public interface IChapterDbService : IOrmMap<DbMangaChapter>
{
    Task SetPages(long id, string[] pages);
}

internal class ChapterDbService : Orm<DbMangaChapter>, IChapterDbService
{
    public ChapterDbService(IOrmService orm) : base(orm) { }

    public Task SetPages(long id, string[] pages)
    {
        const string QUERY = "UPDATE manga_chapter SET pages = :pages WHERE id = :id";
        return Execute(QUERY, new { id, pages });
    }
}
