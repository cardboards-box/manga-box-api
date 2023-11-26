namespace MangaBox.Sources;

public static partial class MangaExtensions
{
    public static string IdFromUrl(string url)
    {
        return url.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
    }

    public static string GetHashId(this DbManga manga)
    {
        var regex = StripNonAlphaNumeric();
        return regex.Replace($"{manga.Provider} {manga.Title}", "").ToLower();
    }

    [GeneratedRegex("[^a-zA-Z0-9 ]")]
    private static partial Regex StripNonAlphaNumeric();
}
