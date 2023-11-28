using Microsoft.Extensions.Configuration;

namespace MangaBox.Tests;

using Core;
using Sources;

public static class TestHelper
{
    private static async Task<IServiceProvider> GenerateProvider(Action<IDependencyResolver> configure)
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        var bob = new DependencyResolver();
        configure(bob);

        await bob.RegisterServices(services, config);
        return services.BuildServiceProvider();
    }

    public static Task<IServiceProvider> ServiceProvider()
    {
        return GenerateProvider(c =>
        {
            c.AddSources();
        });
    }
}
