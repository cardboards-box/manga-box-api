namespace MangaBox.Core;

public static partial class Extensions
{
    public static async Task AddServices(this IServiceCollection services, IConfiguration config,
        Action<IDependencyResolver> configure)
    {
        var bob = new DependencyResolver();
        configure(bob);
        await bob.Build(services, config);
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

    public static string TrimStart(this string input, string trim)
    {
        if (input.StartsWith(trim))
            return input[trim.Length..];

        return input;
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

    /// <summary>
    /// Moves the given iterator until if finds a selector that doesn't match
    /// </summary>
    /// <typeparam name="T">The type of data to process</typeparam>
    /// <param name="data">The iterator to process</param>
    /// <param name="previous">The last item for via any previous MoveUntil reference</param>
    /// <param name="selectors">The different properties to check against</param>
    /// <returns>All of the items in the current grouping</returns>
    public static Grouping<T> MoveUntil<T>(this IEnumerator<T> data, T? previous, params Func<T, object?>[] selectors)
    {
        var items = new List<T>();

        //Add the previous item to the collection of items
        if (previous != null) items.Add(previous);

        //Keep moving through the iterator until EoC
        while (data.MoveNext())
        {
            //Get the current item
            var current = data.Current;
            //Get the last item
            var last = items.LastOrDefault();

            //No last item? Add current and skip to next item
            if (last == null)
            {
                items.Add(current);
                continue;
            }

            //Iterate through selectors until one matches
            for (var i = 0; i < selectors.Length; i++)
            {
                //Get the keys to check
                var selector = selectors[i];
                var fir = selector(last);
                var cur = selector(current);

                //Check if the keys are the same
                var isSame = (fir == null && cur == null) ||
                    (fir != null && fir.Equals(cur));

                //They are the same, move to next selector
                if (isSame) continue;

                //Break out of the check, returning the grouped items and the last item checked
                return new(items.ToArray(), current, i);
            }

            //All selectors are the same, add item to the collection
            items.Add(current);
        }

        //Reached EoC, return items, no last, and no selector index
        return new(items.ToArray(), default, -1);
    }

    /// <summary>
    /// Fetch an index via a predicate
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    /// <param name="data">The data to process</param>
    /// <param name="predicate">The predicate used to find the index</param>
    /// <returns>The index or -1</returns>
    public static int IndexOf<T>(this IEnumerable<T> data, Func<T, bool> predicate)
    {
        int index = 0;
        foreach (var item in data)
        {
            if (predicate(item))
                return index;

            index++;
        }

        return -1;
    }

    /// <summary>
    /// Fetch an index via a predicate (or null if not found)
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    /// <param name="data">The data to process</param>
    /// <param name="predicate">The predicate used to find the index</param>
    /// <returns>The index or null</returns>
    public static int? IndexOfNull<T>(this IEnumerable<T> data, Func<T, bool> predicate)
    {
        var idx = data.IndexOf(predicate);
        return idx == -1 ? null : idx;
    }

    public static TOut? Clone<TIn, TOut>(this TIn data) where TOut : TIn
    {
        var ser = JsonSerializer.Serialize(data);
        return JsonSerializer.Deserialize<TOut>(ser);
    }

    [GeneratedRegex("[^a-zA-Z0-9 ]")]
    private static partial Regex NonAlphaNumeric();
}

public record class Grouping<T>(T[] Items, T? Last, int Index);