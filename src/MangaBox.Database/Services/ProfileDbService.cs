namespace MangaBox.Database.Services;

public interface IProfileDbService : IOrmMap<DbProfile>
{
    Task<DbProfile?> Fetch(string? platformId);

    Task UpdateSettings(string platformId, string settings);
}

internal class ProfileDbService : Orm<DbProfile>, IProfileDbService
{
    private static string? _fetchByPlatformId;

    public ProfileDbService(IOrmService orm) : base(orm) { }

    public Task<DbProfile?> Fetch(string? platformId)
    {
        if (string.IsNullOrEmpty(platformId)) return Task.FromResult<DbProfile?>(null);

        _fetchByPlatformId ??= Map.Select(t => t.With(a => a.PlatformId));
        return Fetch(_fetchByPlatformId, new { PlatformId = platformId });
    }

    public Task UpdateSettings(string platformId, string settings)
    {
        const string QUERY = "UPDATE profiles SET settings_blob = :settings WHERE platform_id = :platformId;";
        return _sql.Execute(QUERY, new { platformId, settings });
    }
}
