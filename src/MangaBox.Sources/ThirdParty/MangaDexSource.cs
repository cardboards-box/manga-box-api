﻿using MangaDexSharp;
using MManga = MangaDexSharp.Manga;

namespace MangaBox.Sources.ThirdParty;

public interface IMangaDexSource : IMangaSource { }

public class MangaDexSource : IMangaDexSource
{
    public const string DEFAULT_LANG = "en";
    public const string MANGA_DEX_PROVIDER = "mangadex";
    public const string MANGA_DEX_HOME_URL = "https://mangadex.org";
    public string HomeUrl => MANGA_DEX_HOME_URL;
    public string Provider => MANGA_DEX_PROVIDER;

    private readonly IMangaDex _mangadex;

    public MangaDexSource(IMangaDex mangadex)
    {
        _mangadex = mangadex;
    }

    public async Task<string[]> Pages(string url)
    {
        var id = MangaExtensions.IdFromUrl(url);
        var pages = await _mangadex.Pages.Pages(id);
        if (pages == null)
            return Array.Empty<string>();

        return pages.Images;
    }

    public async Task<ResolvedManga?> Manga(string url)
    {
        var id = IdFromUrl(url);
        var data = await _mangadex.Manga.Get(id, new[]
        {
            MangaIncludes.cover_art,
            MangaIncludes.author,
            MangaIncludes.artist,
            MangaIncludes.scanlation_group,
            MangaIncludes.tag,
            MangaIncludes.chapter
        });

        if (data == null || data.Data == null) return null;

        var manga = data.Data;

        var chapters = await GetChapters(id, DEFAULT_LANG)
            .OrderBy(t => t.Ordinal)
            .ToArrayAsync();

        var output = Convert(manga);
        return new ResolvedManga(output, chapters);
    }

    public bool Match(string url) => url.StartsWith(HomeUrl);

    public async IAsyncEnumerable<DbMangaChapter> GetChapters(string id, params string[] languages)
    {
        var filter = new MangaFeedFilter { TranslatedLanguage = languages };
        while (true)
        {
            var chapters = await _mangadex.Manga.Feed(id, filter);
            if (chapters == null) yield break;

            var sortedChapters = chapters
                .Data
                .GroupBy(t => t.Attributes.Chapter + t.Attributes.Volume)
                .Select(t => t.PreferedOrFirst(t => t.Attributes.TranslatedLanguage == DEFAULT_LANG))
                .Where(t => t != null)
                .Select(t => Convert(t!))
                .OrderBy(t => t.Volume)
                .OrderBy(t => t.Ordinal);

            foreach (var chap in sortedChapters)
                yield return chap;

            int current = chapters.Offset + chapters.Limit;
            if (chapters.Total <= current) yield break;

            filter.Offset = current;
        }
    }

    public static string IdFromUrl(string url)
    {
        var parts = url.Replace("https://mangadex.org/", "").Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? parts.Last() : parts.Skip(1).First();
    }

    public static T Convert<T>(MManga manga) where T : DbManga, new()
    {
        static string DetermineTitle(MManga manga)
        {
            var title = manga.Attributes.Title.PreferedOrFirst(t => t.Key.ToLower() == DEFAULT_LANG);
            if (title.Key.ToLower() == DEFAULT_LANG) return title.Value;

            var prefered = manga.Attributes.AltTitles.FirstOrDefault(t => t.ContainsKey(DEFAULT_LANG));
            if (prefered != null)
                return prefered.PreferedOrFirst(t => t.Key.ToLower() == DEFAULT_LANG).Value;

            return title.Value;
        }

        static IEnumerable<DbMangaAttribute> GetMangaAttributes(MManga? manga)
        {
            if (manga == null) yield break;

            if (manga.Attributes.ContentRating != null)
                yield return new("Content Rating", manga.Attributes.ContentRating?.ToString() ?? "");

            if (!string.IsNullOrEmpty(manga.Attributes.OriginalLanguage))
                yield return new("Original Language", manga.Attributes.OriginalLanguage);

            if (manga.Attributes.Status != null)
                yield return new("Status", manga.Attributes.Status?.ToString() ?? "");

            if (!string.IsNullOrEmpty(manga.Attributes.State))
                yield return new("Publication State", manga.Attributes.State);

            foreach (var rel in manga.Relationships)
            {
                switch (rel)
                {
                    case PersonRelationship person:
                        yield return new(person.Type == "author" ? "Author" : "Artist", person.Attributes.Name);
                        break;
                    case ScanlationGroup group:
                        yield return new("Scanlation Group", group.Attributes.Name);
                        break;
                }
            }
        }

        var id = manga.Id;
        var coverFile = (manga
            .Relationships
            .FirstOrDefault(t => t is CoverArtRelationship) as CoverArtRelationship
        )?.Attributes?.FileName;
        var coverUrl = $"{MANGA_DEX_HOME_URL}/covers/{id}/{coverFile}";

        var title = DetermineTitle(manga);
        var nsfwRatings = new[] { "erotica", "suggestive", "pornographic" };

        var output = new T
        {
            Title = title,
            SourceId = id,
            Provider = MANGA_DEX_PROVIDER,
            Url = $"{MANGA_DEX_HOME_URL}/title/{id}",
            Cover = coverUrl,
            Description = manga.Attributes.Description.PreferedOrFirst(t => t.Key == DEFAULT_LANG).Value,
            AltTitles = manga.Attributes.AltTitles.SelectMany(t => t.Values).Distinct().ToArray(),
            Tags = manga
                .Attributes
                .Tags
                .Select(t =>
                    t.Attributes
                     .Name
                     .PreferedOrFirst(t => t.Key == DEFAULT_LANG)
                     .Value).ToArray(),
            Nsfw = nsfwRatings.Contains(manga.Attributes.ContentRating?.ToString() ?? ""),
            Attributes = GetMangaAttributes(manga).ToArray(),
            SourceCreated = manga.Attributes.CreatedAt,
            OrdinalVolumeReset = manga.Attributes.ChapterNumbersResetOnNewVolume,
        };
        output.HashId = output.GetHashId();
        return output;
    }

    public static DbManga Convert(MManga manga) => Convert<DbManga>(manga);

    public static DbMangaCache ConvertCache(MManga manga) => Convert<DbMangaCache>(manga);

    public static T Convert<T>(Chapter chapter) where T : DbMangaChapter, new()
    {
        static IEnumerable<DbMangaAttribute> GetChapterAttributes(Chapter? chapter)
        {
            if (chapter == null) yield break;

            yield return new("Translated Language", chapter.Attributes.TranslatedLanguage);

            if (!string.IsNullOrEmpty(chapter.Attributes.Uploader))
                yield return new("Uploader", chapter.Attributes.Uploader);

            foreach (var relationship in chapter.Relationships)
            {
                switch (relationship)
                {
                    case PersonRelationship per:
                        yield return new(per.Type == "author" ? "Author" : "Artist", per.Attributes.Name);
                        break;
                    case ScanlationGroup grp:
                        if (!string.IsNullOrEmpty(grp.Attributes.Name))
                            yield return new("Scanlation Group", grp.Attributes.Name);
                        if (!string.IsNullOrEmpty(grp.Attributes.Website))
                            yield return new("Scanlation Link", grp.Attributes.Website);
                        if (!string.IsNullOrEmpty(grp.Attributes.Twitter))
                            yield return new("Scanlation Twitter", grp.Attributes.Twitter);
                        if (!string.IsNullOrEmpty(grp.Attributes.Discord))
                            yield return new("Scanlation Discord", grp.Attributes.Discord);
                        break;
                }
            }
        }

        return new T
        {
            Title = chapter?.Attributes.Title ?? string.Empty,
            Url = $"{MANGA_DEX_HOME_URL}/chapter/{chapter?.Id}",
            SourceId = chapter?.Id ?? string.Empty,
            Ordinal = double.TryParse(chapter?.Attributes.Chapter, out var a) ? a : 0,
            Volume = double.TryParse(chapter?.Attributes.Volume, out var b) ? b : null,
            ExternalUrl = chapter?.Attributes.ExternalUrl,
            Attributes = GetChapterAttributes(chapter).ToArray(),
            Language = DEFAULT_LANG,
        };
    }

    public static DbMangaChapter Convert(Chapter chapter) => Convert<DbMangaChapter>(chapter);

    public static DbMangaChapterCache ConvertCache(Chapter chapter) => Convert<DbMangaChapterCache>(chapter);
}
