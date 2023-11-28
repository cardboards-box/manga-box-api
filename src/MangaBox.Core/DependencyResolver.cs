using Npgsql;
using MangaDexSharp;
using Serilog;

namespace MangaBox.Core;

public interface IDependencyResolver
{
    IDependencyResolver AddServices(Action<IServiceCollection> services);

    IDependencyResolver AddServices(Action<IServiceCollection, IConfiguration> services);

    IDependencyResolver AddServices(Func<IServiceCollection, Task> services);

    IDependencyResolver AddServices(Func<IServiceCollection, IConfiguration, Task> services);

    IDependencyResolver Model<T>();

    IDependencyResolver Type<T>(string? name = null);

    IDependencyResolver JsonModel<T>(Func<T> @default);

    IDependencyResolver JsonModel<T>();

    IDependencyResolver Transient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    IDependencyResolver Singleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    IDependencyResolver Singleton<TService>(TService instance)
        where TService : class;
}

public class DependencyResolver : IDependencyResolver
{
    private readonly List<Func<IServiceCollection, IConfiguration, Task>> _services = new();
    private readonly List<Action<IConventionBuilder>> _conventions = new();
    private readonly List<Action<ITypeMapBuilder>> _dbMapping = new();
    private readonly List<Action<NpgsqlDataSourceBuilder>> _connections = new();

    public IDependencyResolver AddServices(Action<IServiceCollection> services)
    {
        return AddServices((s, _) => services(s));
    }

    public IDependencyResolver AddServices(Action<IServiceCollection, IConfiguration> services)
    {
        return AddServices((s, c) =>
        {
            services(s, c);
            return Task.CompletedTask;
        });
    }

    public IDependencyResolver AddServices(Func<IServiceCollection, Task> services)
    {
        return AddServices((s, _) => services(s));
    }

    public IDependencyResolver AddServices(Func<IServiceCollection, IConfiguration, Task> services)
    {
        _services.Add(services);
        return this;
    }

    public IDependencyResolver Model<T>()
    {
        _conventions.Add(x => x.Entity<T>());
        return this;
    }

    public IDependencyResolver Type<T>(string? name = null)
    {
        _connections.Add(x => x.MapComposite<T>(name));
        return this;
    }

    public IDependencyResolver JsonModel<T>(Func<T> @default)
    {
        _dbMapping.Add(x => x.DefaultJsonHandler(@default));
        return this;
    }

    public IDependencyResolver JsonModel<T>() => JsonModel<T?>(() => default);

    public IDependencyResolver Transient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddServices(x => x.AddTransient<TService, TImplementation>());
    }

    public IDependencyResolver Singleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddServices(x => x.AddSingleton<TService, TImplementation>());
    }

    public IDependencyResolver Singleton<TService>(TService instance)
        where TService : class
    {
        return AddServices(x => x.AddSingleton(instance));
    }

    public async Task RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services
            .AddJson(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            })
            .AddCardboardHttp()
            .AddFileCache()
            .AddMangaDex(string.Empty)
            .AddSerilog(c =>
            {
                c
                 .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Error)
                 .MinimumLevel.Override("Microsoft.Extensions.Http.DefaultHttpClientFactory", Serilog.Events.LogEventLevel.Error)
                 .WriteTo.Console()
                 .WriteTo.File(Path.Combine("logs", "log.txt"), rollingInterval: RollingInterval.Day)
                 .MinimumLevel.Debug();
            })
            .AddRedis();

        foreach (var action in _services)
            await action(services, config);
    }

    public void RegisterDatabase(IServiceCollection services, string scriptDir)
    {
        services
            .AddSqlService(c =>
            {
                c.ConfigureGeneration(a => a.WithCamelCaseChange())
                 .ConfigureTypes(a =>
                 {
                     var conv = a.CamelCase();
                     foreach (var convention in _conventions)
                         convention(conv);

                     foreach (var mapping in _dbMapping)
                         mapping(a);
                 });

                c.AddPostgres<SqlConfig>(a =>
                {
                    a.OnCreate(con =>
                    {
                        _connections.Each(act => act(con));
                        return Task.CompletedTask;
                    });
                    a.OnInit(con => new DatabaseDeploy(con, scriptDir).ExecuteScripts());
                });
            });
    }

    public Task Build(IServiceCollection services, IConfiguration config, string scriptDir = "Scripts")
    {
        RegisterDatabase(services, scriptDir);
        return RegisterServices(services, config);
    }
}
