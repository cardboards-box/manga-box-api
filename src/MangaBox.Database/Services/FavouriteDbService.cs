namespace MangaBox.Database.Services;

public interface IFavouriteDbService
{
    Task<bool?> Favourite(string platformId, long mangaId);

    Task<bool> IsFavourite(string? platformId, long mangaId);
}

internal class FavouriteDbService : Orm<DbMangaFavourite>, IFavouriteDbService
{
    public FavouriteDbService(IOrmService orm) : base(orm) { }

    public async Task<bool> IsFavourite(string? platformId, long mangaId)
    {
        if (string.IsNullOrEmpty(platformId)) return false;
        const string QUERY = @"SELECT 1 FROM manga_favourites mf 
JOIN profiles p ON p.id = mf.profile_id
WHERE p.platform_id = :platformId AND mf.manga_id = :mangaId";

        var res = await _sql.Fetch<bool?>(QUERY, new { platformId, mangaId });
        return res ?? false;
    }

    public async Task<bool?> Favourite(string platformId, long mangaId)
    {
        const string QUERY = @"SELECT toggle_favourite(:platformId, :mangaId)";
        var res = await _sql.ExecuteScalar<int>(QUERY, new { platformId, mangaId });
        return res == -1 ? null : res == 1;
    }
}
