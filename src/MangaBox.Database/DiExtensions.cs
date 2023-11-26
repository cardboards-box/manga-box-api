namespace MangaBox.Database;

using Services;

public static class DiExtensions
{
    public static IDependencyResolver AddDatabase(this IDependencyResolver resolver)
    {
        return resolver
            //Register all database services
            .Transient<IMangaDbService, MangaDbService>()
            .Transient<IChapterDbService, ChapterDbService>()
            .Transient<IWithChaptersDbService, WithChaptersDbService>()
            .Transient<IExtendedDbService, ExtendedDbService>()
            .Transient<ISearchDbService, SearchDbService>()
            .Transient<IBookmarkDbService, BookmarkDbService>()
            .Transient<IFavouriteDbService, FavouriteDbService>()
            .Transient<IProgressDbService, ProgressDbService>()
            .Transient<IProfileDbService, ProfileDbService>()
            //Register all manga cache related database services
            .Transient<ICacheDbService, CacheDbService>()
            .Transient<IMangaCacheDbService, MangaCacheDbService>()
            .Transient<IMangaChapterCacheDbService, MangaChapterCacheDbService>()
            //Register all roll-up database services      
            .Transient<IDbService, DbService>()
            .Transient<IOrmService, OrmService>()
            .Transient<IFakeUpsertQueryService, FakeUpsertQueryService>()
            .Transient<INotificationService, NotificationService>();
    }
}
