namespace MangaBox.Database.Base;

public abstract class CacheOrm<T> : Orm<T> where T : DbObject
{
    private static double _cacheTime = 5;
    private static CacheItem<T[]>? _cache;

    public virtual double CacheTime
    {
        get => _cache?.ExpireMinutes ?? _cacheTime;
        set
        {
            _cacheTime = value;
            ClearCache();
        }
    }

    protected CacheOrm(IOrmService orm) : base(orm) { }

    public void ClearCache() => _cache = null;

    public override async Task<T[]> Get()
    {
        _cache ??= new CacheItem<T[]>(base.Get, CacheTime);
        return await _cache.Get() ?? Array.Empty<T>();
    }

    public override Task<Guid> Upsert(T item)
    {
        ClearCache();
        return base.Upsert(item);
    }

    public override Task<int> Delete(Guid id)
    {
        ClearCache();
        return base.Delete(id);
    }

    public override Task<Guid> Insert(T item)
    {
        ClearCache();
        return base.Insert(item);
    }

    public override Task<int> Update(T item)
    {
        ClearCache();
        return base.Update(item);
    }
}
