namespace MangaBox.Database.Base;

public interface IDbInterjectService
{
    Task Insert<T>(T entity);

    Task Update<T>(T entity);

    Task Upsert<T>(T entity);

    Task Delete<T>(long id);
}
