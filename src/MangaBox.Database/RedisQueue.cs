using StackExchange.Redis;

namespace MangaBox.Database;

public interface IRedisQueue<T> where T : DbObject
{
    Task<T> Insert(T obj);

    Task Update(T obj);

    Task Enqueue(T obj, bool update);
}

internal class RedisQueue<T> : IRedisQueue<T> where T : DbObject
{
    private readonly IRedisService _redis;
    private readonly IRedisList<T> _createQueue;
    private readonly IRedisList<T> _updateQueue;
    private readonly IDbService _db;
    private readonly Func<IDbService, IOrmMap<T>> _map;
    private readonly string _name;
    private readonly ILogger _logger;

    private IOrmMap<T> Map => _map(_db);

    public RedisQueue(
        IRedisService redis,
        IDbService db,
        ILogger logger,
        Func<IDbService, IOrmMap<T>> map,
        string name)
    {
        _redis = redis;
        _db = db;
        _name = name;
        _createQueue = _redis.List<T>($"{name}-create-queue");
        _updateQueue = _redis.List<T>($"{name}-update-queue");
        _map = map;
        _logger = logger;
    }

    public Task Create(long id) => _redis.Publish($"{_name}-created", new RedisValue(id.ToString()));
    public Task Update(long id) => _redis.Publish($"{_name}-updated", new RedisValue(id.ToString()));

    public async Task Enqueue(T obj, bool update)
    {
        await (update
            ? Enqueue(obj, _updateQueue, Update)
            : Enqueue(obj, _createQueue, Create));
    }

    public async Task Enqueue(T obj, IRedisList<T> queue, Func<long, Task> pub)
    {
        try
        {
            await queue.Append(obj);
            await pub(obj.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while publishing {_name} event: {Id}", _name, obj.Id);
        }
    }

    public async Task<T> Insert(T obj)
    {
        obj.Id = await Map.Insert(obj);
        await Enqueue(obj, false);
        return obj;
    }

    public async Task Update(T obj)
    {
        await Map.Update(obj);
        await Enqueue(obj, true);
    }
}