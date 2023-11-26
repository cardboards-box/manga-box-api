namespace MangaBox.Core;

public static class Utilities
{
    private static readonly Random _rnd = new();

    public static string RandomSuffix(int length = 10)
    {
        var chars = "abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Range(0, length).Select(t => chars[_rnd.Next(chars.Length)]).ToArray());
    }
}