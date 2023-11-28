namespace MangaBox.Services;

using Core;
using Database;
using Models;

public interface IVolumeService
{
    Task<MangaData?> Get(string id, string? pid, ChapterSortColumn sort, bool asc);
}

internal class VolumeService : IVolumeService
{
    private readonly IDbService _db;

    public VolumeService(IDbService db)
    {
        _db = db;
    }

    public async Task<MangaWithChapters?> GetData(string id, string? pid)
    {
        //Ensure a valid ID was passed
        if (string.IsNullOrEmpty(id)) return null;

        //Check if a random manga was requested
        if (id.ToLower().Trim() == "random") return await _db.WithChapters.GetByRandom(pid);

        return await _db.WithChapters.Get(id, pid);
    }

    public async Task<MangaData?> Get(string id, string? pid, ChapterSortColumn sort, bool asc)
    {
        var manga = await GetData(id, pid);
        if (manga == null) return null;

        //Fetch progress, stats, and other authed stuff
        //Skip fetching if the user isn't logged in
        var ext = string.IsNullOrEmpty(pid) ? null : await _db.Extended.Fetch(manga.Manga.Id.ToString(), pid);

        //Create a clone of manga data with extra fields
        var output = manga.Clone<MangaWithChapters, MangaData>();
        if (output == null) return null;

        //Order the chapters by the given sorts
        var chapters = Ordered(manga.Chapters, sort, asc, ext?.Progress, output.Manga.OrdinalVolumeReset);
        //Sort the chapters into volume collections (impacted by sorts)
        output.Volumes = Volumize(chapters, ext?.Progress, ext?.Stats).ToArray();
        //Pass through progress stuff
        output.Chapter = ext?.Chapter ?? manga.Chapters.FirstOrDefault() ?? new();
        output.Progress = ext?.Progress;
        output.Stats = ext?.Stats;
        output.VolumeIndex = ext?.Chapter == null ? 0 : output.Volumes.IndexOfNull(t => t.InProgress) ?? 0;

        return output;
    }

    public static IEnumerable<DbMangaChapter> Ordered(IEnumerable<DbMangaChapter> chap, ChapterSortColumn sort, bool asc, DbMangaProgress? progress, bool reset)
    {
        var byOrdinalAsc = () => reset ? chap.OrderBy(t => t.Ordinal).OrderBy(t => t.Volume ?? 99999) : chap.OrderBy(t => t.Ordinal);
        var byOrdinalDesc = () => reset ? chap.OrderByDescending(t => t.Ordinal).OrderByDescending(t => t.Volume ?? 99999) : chap.OrderByDescending(t => t.Ordinal);

        return sort switch
        {
            ChapterSortColumn.Date => asc ? chap.OrderBy(t => t.CreatedAt) : chap.OrderByDescending(t => t.CreatedAt),
            ChapterSortColumn.Language => asc ? chap.OrderBy(t => t.Language) : chap.OrderByDescending(t => t.Language),
            ChapterSortColumn.Title => asc ? chap.OrderBy(t => t.Title) : chap.OrderByDescending(t => t.Title),
            ChapterSortColumn.Read => OrderByRead(chap, asc, progress, reset),
            _ => asc ? byOrdinalAsc() : byOrdinalDesc(),
        };
    }

    public static IEnumerable<DbMangaChapter> OrderByRead(IEnumerable<DbMangaChapter> chap, bool asc, DbMangaProgress? progress, bool reset)
    {
        if (progress == null) return Ordered(chap, ChapterSortColumn.Ordinal, asc, progress, reset);

        var progs = progress.Read.ToDictionary(t => t.ChapterId, t => t);

        return asc
            ? chap.OrderBy(t => progs.ContainsKey(t.Id))
            : chap.OrderByDescending(t => progs.ContainsKey(t.Id));
    }

    public static IEnumerable<Volume> Volumize(IEnumerable<DbMangaChapter> chapters, DbMangaProgress? progress, MangaStats? stats)
    {
        var iterator = chapters.GetEnumerator();

        //Setup tracking stuff
        DbMangaChapter? chapter = null;
        Volume? volume = null;

        var progs = (progress?.Read ?? Array.Empty<DbMangaChapterProgress>()).ToDictionary(t => t.ChapterId, t => t.PageIndex);

        static Volume postfix(Volume volume)
        {
            volume.Read = volume.Chapters.All(t => t.Read);
            volume.InProgress = !volume.Read && volume.Chapters.Any(t => t.Read);
            return volume;
        };

        while (true)
        {
            //Ensure its not the EoS
            if (chapter == null && !iterator.MoveNext()) break;
            //Get the current chapter
            chapter = iterator.Current;
            //Get all of the grouped versions
            var (versions, last, index) = iterator.MoveUntil(chapter, t => t.Volume, t => t.Ordinal);

            //Shouldn't happen unless something went very wrong.
            if (versions.Length == 0) break;

            var firstChap = versions.First();

            //New volume started, create the wrapping object
            volume ??= new Volume { Name = firstChap.Volume };

            var read = versions.Any(t => progs.ContainsKey(t.Id));
            //Check to see if the current chapter has been read
            int? idx = versions.IndexOfNull(t => t.Id == progress?.MangaChapterId);
            var chap = new VolumeChapter
            {
                Read = read,
                ReadIndex = idx,
                Progress = idx != null ? stats?.PageProgress : null,
                PageIndex = idx != null ? progress?.PageIndex : null,
                Versions = versions,
            };

            volume.Chapters.Add(chap);

            //New volume started, return the old one
            if (index == 0 && volume != null)
            {
                yield return postfix(volume);
                volume = null;
            }

            chapter = last;
        }

        if (volume != null) yield return postfix(volume);
    }
}
