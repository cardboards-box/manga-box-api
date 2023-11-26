namespace MangaBox.Tests;

using MangaBox.Sources;
using Sources.ThirdParty;

[TestClass]
public class SourcesTests
{
    private IImportService? _import;

    [TestInitialize]
    public async Task Setup()
    {
        var provider = await TestHelper.ServiceProvider();
        _import = provider.GetRequiredService<IImportService>();
    }

    [TestMethod]
    public async Task NHentaiTests()
    {
        const string URL = "https://nhentai.to/g/402922";
        var result = await _import!.Manga(URL);

        Assert.IsNotNull(result, "NH Test - Manga Fetch");
        var (manga, chapters) = result!;

        Assert.AreEqual("402922", manga.SourceId, "NH Test - Source Id");
        Assert.AreEqual("[Yowoshinobu] 4-nin no Echi-teki na Megane-tachi (Fate/Grand Order) [Chinese] [不咕鸟汉化组] [Digital]", manga.Title, "NH Test - Title");
        Assert.AreEqual("https://nhentai.to/g/402922", manga.Url, "NH Test - Url");
        Assert.AreEqual("https://cdn.dogehls.xyz/galleries/2219016/cover.jpg", manga.Cover, "NH Test - Cover");
        Assert.AreEqual(3, manga.Tags.Length, "NH Test - Tag Count");
        Assert.IsTrue(manga.Nsfw, "NH Test - Nsfw");

        Assert.IsNotNull(chapters, "NH Test - Chapters");
        Assert.IsTrue(chapters!.Length > 0, "NH Test - Chapter Count");
        Assert.AreEqual("Chapter 1", chapters.First().Title, "NH Test - Chapter Title");
        Assert.AreEqual(24, chapters.First().Pages.Length, "NH Test - Chapter Pages Count");
    }

    [TestMethod]
    public async Task MangaKatanaTests()
    {
        const string TITLE = "MANGA-KATANA";
        const string URL = "https://mangakatana.com/manga/magic-emperor.17922";
        var result = await _import!.Manga(URL);
        Assert.IsNotNull(result, $"{TITLE} - Manga Fetch");
        var (manga, chapters) = result!;

        Assert.AreEqual("magic-emperor.17922", manga.SourceId, $"{TITLE} - Source Id");
        Assert.AreEqual("Magic Emperor", manga.Title, $"{TITLE} - Title");
        Assert.AreEqual("https://mangakatana.com/manga/magic-emperor.17922", manga.Url, $"{TITLE} - Url");
        Assert.AreEqual("https://mangakatana.com/imgs/cover/04e/5a/f7731-l.jpg", manga.Cover, $"{TITLE} - Cover");
        Assert.AreEqual(7, manga.Tags.Length, $"{TITLE} - Tag Count");
        Assert.IsFalse(manga.Nsfw, $"{TITLE} - Nsfw");

        Assert.IsNotNull(chapters, $"{TITLE} - Chapters");
        Assert.IsTrue(chapters!.Length > 0, $"{TITLE} - Chapter Count");
        Assert.AreEqual("Chapter 1", chapters.First().Title, $"{TITLE} - Chapter Title");

        var pages = await _import!.Pages(manga, chapters.First().Url);
        Assert.IsNotNull(pages, $"{TITLE} - Chapter Pages");
        Assert.AreEqual(51, pages.Length, $"{TITLE} - Chapter Pages Count");
    }

    [TestMethod]
    public async Task MangakakalotTests()
    {
        const string TITLE = "MANGAKAKALOT";
        const string URL = "https://ww5.mangakakalot.tv/manga/manga-tb953284";
        var result = await _import!.Manga(URL);
        Assert.IsNotNull(result, $"{TITLE} - Manga Fetch");
        var (manga, chapters) = result!;

        Assert.AreEqual("manga-tb953284", manga.SourceId, $"{TITLE} - Source Id");
        Assert.AreEqual("Kyokou Suiri", manga.Title, $"{TITLE} - Title");
        Assert.AreEqual("https://ww5.mangakakalot.tv/manga/manga-tb953284", manga.Url, $"{TITLE} - Url");
        Assert.AreEqual("https://ww4.mangakakalot.tv/mangaimage/manga-tb953284.jpg", manga.Cover, $"{TITLE} - Cover");
        Assert.AreEqual(5, manga.Tags.Length, $"{TITLE} - Tag Count");
        Assert.IsFalse(manga.Nsfw, $"{TITLE} - Nsfw");

        Assert.IsNotNull(chapters, $"{TITLE} - Chapters");
        Assert.IsTrue(chapters!.Length > 0, $"{TITLE} - Chapter Count");
        Assert.AreEqual("Vol.1 Chapter 1", chapters.First().Title, $"{TITLE} - Chapter Title");

        var pages = await _import!.Pages(manga, chapters.First().Url);
        Assert.IsNotNull(pages, $"{TITLE} - Chapter Pages");
        Assert.AreEqual(83, pages.Length, $"{TITLE} - Chapter Pages Count");
    }

    [TestMethod]
    public async Task MangakakalotComTests()
    {
        const string TITLE = "MANGAKAKALOT-COM";
        const string URL = "https://mangakakalot.com/read-rm5iu158524511364";
        var result = await _import!.Manga(URL);
        Assert.IsNotNull(result, $"{TITLE} - Manga Fetch");
        var (manga, chapters) = result!;

        Assert.AreEqual("rm5iu158524511364", manga.SourceId, $"{TITLE} - Source Id");
        Assert.AreEqual("Why Naitou", manga.Title, $"{TITLE} - Title");
        Assert.AreEqual("https://mangakakalot.com/read-rm5iu158524511364", manga.Url, $"{TITLE} - Url");
        Assert.AreEqual("https://avt.mkklcdnv6temp.com/10/d/16-1583493840.jpg", manga.Cover, $"{TITLE} - Cover");
        Assert.AreEqual(4, manga.Tags.Length, $"{TITLE} - Tag Count");
        Assert.IsFalse(manga.Nsfw, $"{TITLE} - Nsfw");

        Assert.IsNotNull(chapters, $"{TITLE} - Chapters");
        Assert.IsTrue(chapters!.Length > 0, $"{TITLE} - Chapter Count");
        Assert.AreEqual("Vol.1 chapter 1 : It's chapter 1", chapters.First().Title, $"{TITLE} - Chapter Title");

        var pages = await _import!.Pages(manga,chapters.First().Url);
        Assert.IsNotNull(pages, $"{TITLE} - Chapter Pages");
        Assert.AreEqual(20, pages.Length, $"{TITLE} - Chapter Pages Count");
    }

    [TestMethod]
    public async Task MangadexTests()
    {
        const string TITLE = "MANGA-DEX";
        const string URL = "https://mangadex.org/title/fc0a7b86-992e-4126-b30f-ca04811979bf/the-unrivaled-mememori-kun";
        var result = await _import!.Manga(URL);
        Assert.IsNotNull(result, $"{TITLE} - Manga Fetch");
        var (manga, chapters) = result!;

        Assert.AreEqual("fc0a7b86-992e-4126-b30f-ca04811979bf", manga.SourceId, $"{TITLE} - Source Id");
        Assert.AreEqual("The Unrivaled Mememori-kun", manga.Title, $"{TITLE} - Title");
        Assert.AreEqual("https://mangadex.org/title/fc0a7b86-992e-4126-b30f-ca04811979bf", manga.Url, $"{TITLE} - Url");
        Assert.AreEqual("https://mangadex.org/covers/fc0a7b86-992e-4126-b30f-ca04811979bf/8a5bee4b-935b-4aa7-9994-787e2f15b8a1.jpg", manga.Cover, $"{TITLE} - Cover");
        Assert.AreEqual(4, manga.Tags.Length, $"{TITLE} - Tag Count");
        Assert.IsFalse(manga.Nsfw, $"{TITLE} - Nsfw");

        Assert.IsNotNull(chapters, $"{TITLE} - Chapters");
        Assert.IsTrue(chapters!.Length > 0, $"{TITLE} - Chapter Count");
        Assert.AreEqual("When I Woke Up", chapters.First().Title, $"{TITLE} - Chapter Title");

        var pages = await _import!.Pages(manga, chapters.First().Url);
        Assert.IsNotNull(pages, $"{TITLE} - Chapter Pages");
        Assert.AreEqual(35, pages.Length, $"{TITLE} - Chapter Pages Count");
    }

    [TestMethod]
    public async Task MangaClashTests()
    {
        const string TITLE = "MANGA-CLASH";
        const string URL = "https://mangaclash.com/manga/the-former-hero-wants-to-live-peacefully/";
        var result = await _import!.Manga(URL);
        Assert.IsNotNull(result, $"{TITLE} - Manga Fetch");
        var (manga, chapters) = result!;

        Assert.AreEqual("the-former-hero-wants-to-live-peacefully", manga.SourceId, $"{TITLE} - Source Id");
        Assert.AreEqual("The Former Hero Wants To Live Peacefully", manga.Title, $"{TITLE} - Title");
        Assert.AreEqual("https://mangaclash.com/manga/the-former-hero-wants-to-live-peacefully/", manga.Url, $"{TITLE} - Url");
        Assert.AreEqual("https://mangaclash.com/wp-content/uploads/2022/07/The-Former-Hero-Wants-To-Live-Peacefully.jpg", manga.Cover, $"{TITLE} - Cover");
        Assert.AreEqual(2, manga.Tags.Length, $"{TITLE} - Tag Count");
        Assert.IsFalse(manga.Nsfw, $"{TITLE} - Nsfw");

        Assert.IsNotNull(chapters, $"{TITLE} - Chapters");
        Assert.IsTrue(chapters!.Length > 0, $"{TITLE} - Chapter Count");
        Assert.AreEqual("Chapter 1", chapters.First().Title, $"{TITLE} - Chapter Title");

        var pages = await _import!.Pages(manga, chapters.First().Url);
        Assert.IsNotNull(pages, $"{TITLE} - Chapter Pages");
        Assert.AreEqual(27, pages.Length, $"{TITLE} - Chapter Pages Count");
    }

    [TestMethod]
    public async Task DarkscansTests()
    {
        const string TITLE = "DARK-SCANS";
        const string URL = "https://dark-scan.com/manga/yuusha-party-o-oida-sareta-kiyou-binbou/";
        var result = await _import!.Manga(URL);

        Assert.IsNotNull(result, $"{TITLE} - Manga Fetch");
        var (manga, chapters) = result!;

        Assert.AreEqual("yuusha-party-o-oida-sareta-kiyou-binbou", manga.SourceId, $"{TITLE} - Source Id");
        Assert.AreEqual("Yuusha Party o Oida Sareta Kiyou Binbou", manga.Title, $"{TITLE} - Title");
        Assert.AreEqual("https://dark-scan.com/manga/yuusha-party-o-oida-sareta-kiyou-binbou/", manga.Url, $"{TITLE} - Url");
        Assert.AreEqual("https://dark-scan.com/wp-content/uploads/2023/10/6321087582de2-193x278-1.jpg", manga.Cover, $"{TITLE} - Cover");
        Assert.AreEqual(4, manga.Tags.Length, $"{TITLE} - Tag Count");
        Assert.IsFalse(manga.Nsfw, $"{TITLE} - Nsfw");

        Assert.IsNotNull(chapters, $"{TITLE} - Chapters");
        Assert.IsTrue(chapters!.Length > 0, $"{TITLE} - Chapter Count");
        Assert.AreEqual("Chapter 1", chapters.First().Title, $"{TITLE} - Chapter Title");

        var pages = await _import!.Pages(manga, chapters.First().Url);
        Assert.IsNotNull(pages, $"{TITLE} - Chapter Pages");
        Assert.AreEqual(42, pages.Length, $"{TITLE} - Chapter Pages Count");
    }
}