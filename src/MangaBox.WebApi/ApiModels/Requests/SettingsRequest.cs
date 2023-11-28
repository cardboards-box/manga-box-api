namespace MangaBox.WebApi.ApiModels.Requests;

public class SettingsRequest
{
    [JsonPropertyName("settings")]
    public string Settings { get; set; } = string.Empty;
}
