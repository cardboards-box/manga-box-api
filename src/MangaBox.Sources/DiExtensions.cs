namespace MangaBox.Sources;

using ThirdParty;

public static class DiExtensions
{
    public static IDependencyResolver AddSources(this IDependencyResolver resolver)
    {
        return resolver
            .Transient<IImportService, ImportService>()

            .Transient<INhentaiSource, NHentaiSource>()
            .Transient<IMangaKatanaSource, MangaKatanaSource>()
            .Transient<IMangakakalotTvSource, MangakakalotTvSource>()
            .Transient<IMangakakalotComSource, MangakakalotComSource>()
            .Transient<IMangakakalotComAltSource, MangakakalotComAltSource>()
            .Transient<IMangaDexSource, MangaDexSource>()
            .Transient<IMangaClashSource, MangaClashSource>()
            .Transient<IDarkScansSource, DarkScansSource>();
    }
}