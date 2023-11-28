namespace MangaBox.Models;

[Composite]
public class MangaStats
{
    [JsonPropertyName("mangaId")]
    public long MangaId { get; set; }

    [JsonPropertyName("mangaChapterId")]
    public long MangaChapterId { get; set; }

    [JsonPropertyName("firstChapterId")]
    public long FirstChapterId { get; set; }

    [JsonPropertyName("progressChapterId")]
    public long? ProgressChapterId { get; set; }

    [JsonPropertyName("progressId")]
    public long? ProgressId { get; set; }

    [JsonPropertyName("maxChapterNum")]
    public long MaxChapterNum { get; set; }

    [JsonPropertyName("chapterNum")]
    public long ChapterNum { get; set; }

    [JsonPropertyName("pageCount")]
    public int PageCount { get; set; }

    [JsonPropertyName("chapterProgress")]
    public double ChapterProgress { get; set; }

    [JsonPropertyName("pageProgress")]
    public double PageProgress { get; set; }

    [JsonPropertyName("favourite")]
    public bool Favourite { get; set; } = false;

    [JsonPropertyName("bookmarks")]
    public int[] Bookmarks { get; set; } = Array.Empty<int>();

    [JsonPropertyName("hasBookmarks")]
    public bool HasBookmarks { get; set; } = false;

    [JsonPropertyName("profileId")]
    public long? ProfileId { get; set; }

    [JsonPropertyName("latestChapter")]
    public DateTime? LatestChapter { get; set; }

    [JsonPropertyName("progressRemoved")]
    public bool ProgressRemoved { get; set; } = false;

    [JsonPropertyName("completed")]
    public bool Completed { get; set; } = false;
}

