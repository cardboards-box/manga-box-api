namespace MangaBox.Core;

public static partial class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services,
        Action<IDependencyResolver> configure)
    {
        var bob = new DependencyResolver();
        configure(bob);
        bob.Build(services);
        return services;
    }

    public static bool EqualsIc(this string first, string second)
    {
        return first.Equals(second, StringComparison.InvariantCultureIgnoreCase);
    }

    public static IEnumerable<T> Flags<T>(this T value, bool onlyBits = false) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>();
        var ops = values.Where(x => value.HasFlag(x));

        if (onlyBits)
            ops = ops.Where(x => ((int)(object)x & ((int)(object)x - 1)) == 0);

        return ops;
    }

    public static IEnumerable<T> Flags<T>(this T value, Func<T, bool> predicate) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>();
        return values.Where(x => value.HasFlag(x) && predicate(x));
    }

    public static IEnumerable<T> AllFlags<T>(this T _) where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static string? StripNonAlphaNumeric(this string? input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return NonAlphaNumeric().Replace(input, string.Empty);
    }

    public static string StrJoin<T>(this IEnumerable<T> input, string joiner = " ")
    {
        return string.Join(joiner, input);
    }

    public static bool Expired(this DateTime date, int seconds)
    {
        return date.ToUniversalTime().AddSeconds(seconds) < DateTime.UtcNow;
    }

    public static bool OfType<T>(this object input)
    {
        return input.GetType().OfType<T>();
    }

    public static bool OfType<T>(this object input, out T? type)
    {
        if (!input.GetType().OfType<T>())
        {
            type = default;
            return false;
        }

        type = (T)input;
        return true;
    }

    public static bool OfType<T>(this Type type)
    {
        var inf = typeof(T);
        return inf.IsAssignableFrom(type);
    }

    public static string Parameterize(this Dictionary<string, string> pars)
    {
        return pars.Count == 0
            ? string.Empty
            : "?" + string.Join("&", pars.Select(x => $"{x.Key}={x.Value}"));
    }

    public static async IAsyncEnumerable<TOut> SelectManyAsync<TOut, TIn>(this IEnumerable<TIn> input, Func<TIn, IAsyncEnumerable<TOut>> selector)
    {
        foreach (var item in input)
            await foreach (var output in selector(item))
                yield return output;
    }

    public static IEnumerable<T[]> Split<T>(this IEnumerable<T> data, int count)
    {
        var total = (int)Math.Ceiling((decimal)data.Count() / count);
        var current = new List<T>();

        foreach (var item in data)
        {
            current.Add(item);

            if (current.Count == total)
            {
                yield return current.ToArray();
                current.Clear();
            }
        }

        if (current.Count > 0) yield return current.ToArray();
    }

    public static async Task<(MemoryStream stream, string path)> Download(this IApiService api, string url)
    {
        var result = await api.Create(url, "GET")
            .With(c =>
            {
                c.Headers.Add("Cache-Control", "no-cache");
                c.Headers.Add("Cache-Control", "no-store");
                c.Headers.Add("Cache-Control", "max-age=1");
                c.Headers.Add("Cache-Control", "s-maxage=1");
                c.Headers.Add("Pragma", "no-cache");
            })
            .Result() ?? throw new NullReferenceException("Http result was null for down: " + url);

        result.EnsureSuccessStatusCode();

        var headers = result.Content.Headers;
        var path = headers?.ContentDisposition?.FileName ?? headers?.ContentDisposition?.Parameters?.FirstOrDefault()?.Value ?? "";

        var io = new MemoryStream();
        using (var stream = await result.Content.ReadAsStreamAsync())
            await stream.CopyToAsync(io);

        io.Position = 0;
        return (io, path);
    }

    [GeneratedRegex("[^a-zA-Z0-9 ]")]
    private static partial Regex NonAlphaNumeric();
}
