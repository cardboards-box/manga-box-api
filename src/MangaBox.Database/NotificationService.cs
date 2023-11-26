namespace MangaBox.Database;

public interface INotificationService
{
    IRedisQueue<T> CreateQueue<T>(string name, Func<IDbService, IOrmMap<T>> map)
        where T : DbObject;
}

internal class NotificationService : INotificationService
{
    private readonly IRedisService _redis;
    private readonly IDbService _db;
    private readonly ILogger _logger;

    public NotificationService(
        IRedisService redis,
        IDbService db,
        ILogger<NotificationService> logger)
    {
        _redis = redis;
        _db = db;
        _logger = logger;
    }

    public IRedisQueue<T> CreateQueue<T>(string name, Func<IDbService, IOrmMap<T>> map)
        where T : DbObject
    {
        return new RedisQueue<T>(_redis, _db, _logger, map, name);
    }
}
