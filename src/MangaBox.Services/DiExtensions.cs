namespace MangaBox.Services;

public static class DiExtensions
{
    public static IDependencyResolver AddServices(this IDependencyResolver resolver)
    {
        return resolver
            .Transient<IVolumeService, VolumeService>()
            .Transient<IPageService, PageService>()
            .Transient<IZipService, ZipService>()
            .Transient<IMangaImportService, MangaImportService>();
    }
}