namespace MangaBox.Sources;

public record class ResolvedManga(DbManga Manga, DbMangaChapter[] Chapters);
