namespace MangaBox.Models;

[CompositeTuple]
public class MangaExtended
{
    [JsonPropertyName("manga")]
    public DbManga Manga { get; set; } = new();

    [JsonPropertyName("progress")]
    public DbMangaProgress? Progress { get; set; }

    [JsonPropertyName("chapter")]
    public DbMangaChapter Chapter { get; set; } = new();

    [JsonPropertyName("stats")]
    public MangaStats Stats { get; set; } = new();

    public MangaExtended() { }

    public MangaExtended(
        DbManga manga,
        DbMangaProgress progress,
        DbMangaChapter chapter,
        MangaStats stats)
    {
        Manga = manga;
        Progress = progress?.Id == 0 ? null : progress;
        Chapter = chapter;
        Stats = stats;
    }
}
