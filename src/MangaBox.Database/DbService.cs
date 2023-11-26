namespace MangaBox.Database;

using Services;

public interface IDbService
{
    IBookmarkDbService Bookmarks { get; }

    IChapterDbService Chapters { get; }

    IExtendedDbService Extended { get; }

    IFavouriteDbService Favourites { get; }

    IMangaDbService Manga { get; }

    IProfileDbService Profiles { get; }

    IProgressDbService Progress { get; }

    ISearchDbService Search { get; }

    IWithChaptersDbService WithChapters { get; }

    ICacheDbService Cache { get; }

    IMangaCacheDbService MangaCache { get; }

    IMangaChapterCacheDbService MangaChapterCache { get; }
}

public class DbService : IDbService
{
    public IBookmarkDbService Bookmarks { get; }

    public IChapterDbService Chapters { get; }

    public IExtendedDbService Extended { get; }

    public IFavouriteDbService Favourites { get; }

    public IMangaDbService Manga { get; }

    public IProfileDbService Profiles { get; }

    public IProgressDbService Progress { get; }

    public ISearchDbService Search { get; }

    public IWithChaptersDbService WithChapters { get; }

    public ICacheDbService Cache { get; }

    public IMangaCacheDbService MangaCache { get; }

    public IMangaChapterCacheDbService MangaChapterCache { get; }

    public DbService(
        IBookmarkDbService bookmarks,
        IChapterDbService chapters,
        IExtendedDbService extended,
        IFavouriteDbService favourites,
        IMangaDbService manga,
        IProfileDbService profiles,
        IProgressDbService progress,
        ISearchDbService search,
        IWithChaptersDbService withChapters,
        ICacheDbService cache,
        IMangaCacheDbService mangaCache,
        IMangaChapterCacheDbService mangaChapterCache)
    {
        Bookmarks = bookmarks;
        Chapters = chapters;
        Extended = extended;
        Favourites = favourites;
        Manga = manga;
        Profiles = profiles;
        Progress = progress;
        Search = search;
        WithChapters = withChapters;
        Cache = cache;
        MangaCache = mangaCache;
        MangaChapterCache = mangaChapterCache;
    }
}