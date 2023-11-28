namespace MangaBox.WebApi.ApiModels.Requests;

public class MangaUpdateRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("reset")]
    public bool? Reset { get; set; }
}
