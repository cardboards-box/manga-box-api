namespace MangaBox.Core.Migrations;

public class Manifest
{
    [JsonPropertyName("workDir")]
    public string WorkDir { get; set; } = string.Empty;

    [JsonPropertyName("paths")]
    public string[] Paths { get; set; } = Array.Empty<string>();
}
