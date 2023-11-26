﻿namespace MangaBox.Database.Services;

public interface IMangaDbService : IOrmMap<DbManga>
{
    /// <summary>
    /// Fetch <see cref="DbManga"/> by either the <see cref="DbManga.Id"/> or <see cref="DbManga.HashId"/>
    /// </summary>
    /// <param name="id">The <see cref="DbManga.Id"/> or <see cref="DbManga.HashId"/></param>
    /// <returns></returns>
    Task<DbManga?> Fetch(string id);

    /// <summary>
    /// Fetch <see cref="DbManga"/> by the <see cref="DbManga.HashId"/>
    /// </summary>
    /// <param name="hashId">The <see cref="DbManga.HashId"/></param>
    /// <returns></returns>
    Task<DbManga?> FetchByHashId(string hashId);

    /// <summary>
    /// Fetch <see cref="DbManga"/> by the <see cref="DbManga.SourceId"/>
    /// </summary>
    /// <param name="sourceId">The <see cref="DbManga.SourceId"/></param>
    /// <returns></returns>
    Task<DbManga?> FetchBySourceId(string sourceId);

    /// <summary>
    /// Gets the top <paramref name="count"/> <see cref="DbManga"/> by <see cref="DbManga.UpdatedAt"/>
    /// </summary>
    /// <param name="count">The top <paramref name="count"/></param>
    /// <returns></returns>
    Task<DbManga[]> GetByUpdated(int count);

    /// <summary>
    /// Gets the top <paramref name="count"/> <see cref="DbManga"/> randomly
    /// </summary>
    /// <param name="count"The top <paramref name="count"/></param>
    /// <returns></returns>
    Task<DbManga[]> GetByRandom(int count);
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
}
