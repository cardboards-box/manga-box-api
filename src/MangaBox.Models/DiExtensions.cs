namespace MangaBox.Models;

public static class DiExtensions
{
    public static IDependencyResolver AddModels(this IDependencyResolver resolver)
    {
        RegisterModels(resolver);
        RegisterTypes(resolver);
        return resolver;
    }

    public static void RegisterModels(IDependencyResolver resolver)
    {
        var attributes = new[] { typeof(CompositeAttribute), typeof(TableAttribute) };

        var method = typeof(DependencyResolver).GetMethod(nameof(DependencyResolver.Model));
        if (method is null) return;

        var classes = typeof(DbManga).Assembly.GetTypes()
            .Where(x => 
                x.IsClass && 
                !x.IsAbstract && 
                attributes.Any(a => x.GetCustomAttribute(a) is not null));

        foreach(var item in classes)
        {
            method
                .MakeGenericMethod(item)
                .Invoke(resolver, null);
        }
    }

    public static void RegisterTypes(IDependencyResolver resolver)
    {
        var method = typeof(DependencyResolver).GetMethod(nameof(DependencyResolver.Type));
        if (method is null) return;

        var classes = typeof(DiExtensions).Assembly.GetTypes()
            .Where(x => x.IsClass && !x.IsAbstract)
            .Select(type => (type, attr: type.GetCustomAttribute<TypeAttribute>()))
            .Where(t => t.attr is not null)
            .Select(t => (t.type, attr: t.attr!));

        foreach(var (type, attr) in classes)
        {
            method
                .MakeGenericMethod(type)
                .Invoke(resolver, new object[] { attr.Name });
        }
    }
}