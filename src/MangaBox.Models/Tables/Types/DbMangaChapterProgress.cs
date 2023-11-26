namespace MangaBox.Models;

[Type("manga_chapter_progress")]
public class DbMangaChapterProgress
{
    [JsonPropertyName("chapterId")]
    public long ChapterId { get; set; }

    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; }

    public DbMangaChapterProgress() { }

    public DbMangaChapterProgress(long chapterId, int pageIndex)
    {
        ChapterId = chapterId;
        PageIndex = pageIndex;
    }
}
