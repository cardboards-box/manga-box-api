namespace MangaBox.Database.Services;

public interface IProfileDbService
{
    Task<DbProfile?> Fetch(long id);

    Task<DbProfile?> Fetch(string? platformId);
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
}
