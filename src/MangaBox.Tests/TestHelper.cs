namespace MangaBox.Tests;

using Core;
using Sources;

public static class TestHelper
{
    private static Task<IServiceProvider> GenerateProvider(Action<IDependencyResolver> configure)
    {
        var services = new ServiceCollection();

        var bob = new DependencyResolver();
        configure(bob);

        bob.RegisterServices(services);
        return Task.FromResult<IServiceProvider>(services.BuildServiceProvider());
    }

    public static Task<IServiceProvider> ServiceProvider()
    {
        return GenerateProvider(c =>
        {
            c.AddSources();
        });
    }
}
