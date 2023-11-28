namespace MangaBox.Database.Services;

public interface IMangaDbService : IOrmMap<DbManga>
{
    Task<DbManga?> Fetch(string id);

    Task<DbManga?> FetchByHashId(string hashId);

    Task<DbManga?> FetchBySourceId(string sourceId);

    Task<DbManga[]> GetByUpdated(int count);

    Task<DbManga[]> GetByRandom(int count);

    Task SetDisplayTitle(string id, string? title);

    Task SetOrdinalReset(string id, bool reset);
}

internal class MangaDbService : Orm<DbManga>, IMangaDbService
{
    private static string? _fetchByHashId;
    private static string? _fetchBySourceId;

    public MangaDbService(IOrmService orm) : base(orm) { }

    public Task<DbManga?> Fetch(string id)
    {
        return long.TryParse(id, out var lid)
            ? Fetch(lid)
            : FetchByHashId(id);
    }

    public Task<DbManga?> FetchByHashId(string hashId)
    {
        _fetchByHashId ??= Map.Select(t => t.With(a => a.HashId).Null(a => a.DeletedAt));
        return Fetch(_fetchByHashId, new { HashId = hashId });
    }

    public Task<DbManga?> FetchBySourceId(string sourceId)
    {
        _fetchBySourceId ??= Map.Select(t => t.With(a => a.SourceId).Null(a => a.DeletedAt));
        return Fetch(_fetchBySourceId, new { SourceId = sourceId });
    }

    public Task<DbManga[]> GetByUpdated(int count)
    {
        const string QUERY = "SELECT * FROM manga ORDER BY updated_at ASC LIMIT :count";
        return Get(QUERY, new { count });
    }

    public Task<DbManga[]> GetByRandom(int count)
    {
        const string QUERY = "SELECT * FROM manga ORDER BY random() LIMIT :count";
        return Get(QUERY, new { count });
    }

    public Task SetDisplayTitle(string id, string? title)
    {
        const string QUERY = @"UPDATE manga SET display_title = :title WHERE id = :id OR hash_id = :hashId";

        long? mid = null;
        string? hashId = id;
        if (long.TryParse(id, out var m))
        {
            hashId = null;
            mid = m;
        }

        return _sql.Execute(QUERY, new { id = mid, hashId, title });
    }

    public Task SetOrdinalReset(string id, bool reset)
    {
        const string QUERY = @"UPDATE manga SET ordinal_volume_reset = :reset WHERE id = :id OR hash_id = :hashId";

        long? mid = null;
        string? hashId = id;
        if (long.TryParse(id, out var m))
        {
            hashId = null;
            mid = m;
        }

        return _sql.Execute(QUERY, new { id = mid, hashId, reset });
    }
}
