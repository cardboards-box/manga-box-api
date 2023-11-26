namespace MangaBox.Sources.Base;

public record class ResolvedManga(DbManga Manga, DbMangaChapter[] Chapters);
