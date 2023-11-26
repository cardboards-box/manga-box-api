namespace MangaBox.Database.Base;

public interface IOrmService
{
    IQueryService Query { get; }
    ISqlService Sql { get; }
    IDbInterjectService? Interject { get; }
    IFakeUpsertQueryService FakeUpsert { get; }
    IOrmMapQueryable<T> For<T>() where T : DbObject;
}

public class OrmService : IOrmService
{
    public IQueryService Query { get; }
    public ISqlService Sql { get; }
    public IDbInterjectService? Interject { get; }
    public IFakeUpsertQueryService FakeUpsert { get; }

    public OrmService(
        IQueryService query,
        ISqlService sql,
        IFakeUpsertQueryService fakeUpsert,
        IDbInterjectService? interject = null)
    {
        Query = query;
        Sql = sql;
        Interject = interject;
        FakeUpsert = fakeUpsert;
    }

    public IOrmMapQueryable<T> For<T>() where T : DbObject => new OrmMap<T>(Query, Sql);
}