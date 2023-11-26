namespace MangaBox.Sources.ThirdParty;

public interface IDarkScansSource : IMangaSource { }

internal class DarkScansSource : IDarkScansSource
{
    public string HomeUrl => "https://dark-scan.com/";

    public string MangaBaseUri => $"{HomeUrl}manga/";

    public string Provider => "dark-scans";

    private readonly IApiService _api;

    public DarkScansSource(IApiService api)
    {
        _api = api;
    }

    public async Task<ResolvedManga?> Manga(string url)
    {
        var doc = await _api.GetHtml(url);
        if (doc == null) return null;

        var manga = new DbManga
        {
            Title = doc.Attribute("//meta[@property='og:title']", "content") ?? "",
            SourceId = MangaExtensions.IdFromUrl(url),
            Provider = Provider,
            Url = url,
            Cover = doc.Attribute("//meta[@property='og:image']", "content") ?? ""
        };

        var postContent = doc.DocumentNode.SelectNodes("//div[@class='post-content_item']");

        foreach (var div in postContent)
        {
            var clone = div.Copy();
            var title = clone.InnerText("//h5")?.Trim().ToLower();
            var content = clone.SelectSingleNode("//div[@class='summary-content']");
            if (string.IsNullOrEmpty(title)) continue;

            if (title.Contains("alternative"))
            {
                manga.AltTitles = content.InnerText.Trim().Split(';', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();
                continue;
            }

            if (title.Contains("genre"))
            {
                manga.Tags = content.SelectNodes("//a[@rel='tag']").Select(t => t.InnerText.Trim()).ToArray();
                continue;
            }
        }

        manga.Description = doc.InnerHtml("//div[@class='summary__content show-more']") ?? "";
        var chapters = await GetChapters(url);

        return new(manga, chapters);
    }

    public async Task<DbMangaChapter[]> GetChapters(string url)
    {
        //https://dark-scan.com/manga/yuusha-party-o-oida-sareta-kiyou-binbou/ajax/chapters/
        url = url.TrimEnd('/') + "/ajax/chapters";
        var doc = await _api.GetHtml(url, c =>
        {
            c.Method = HttpMethod.Post;
        });
        if (doc == null) return Array.Empty<DbMangaChapter>();

        var output = new List<DbMangaChapter>();
        var chapters = doc.DocumentNode.SelectNodes("//li[contains(@class, 'wp-manga-chapter')]/a");
        int i = chapters.Count;
        foreach (var chap in chapters)
        {
            i--;
            var href = chap.GetAttributeValue("href", "");
            var name = chap.InnerText;

            output.Add(new DbMangaChapter
            {
                Title = name.Trim(),
                Url = href.Trim(),
                SourceId = href.Trim('/').Split('/').Last(),
                Ordinal = i
            });
        }
        return output.OrderBy(t => t.Ordinal).ToArray();
    }

    public bool Match(string url) => url.ToLower().StartsWith(MangaBaseUri);

    public async Task<string[]> Pages(string url)
    {
        var doc = await _api.GetHtml(url.TrimEnd('/') + "?style=list");
        return doc is null 
            ? Array.Empty<string>() 
            : doc.DocumentNode
                .SelectNodes("//img[@class='wp-manga-chapter-img']")
                .Select(t => t.GetAttributeValue("src", "").Trim('\n', '\t', '\r'))
                .ToArray();
    }
}
