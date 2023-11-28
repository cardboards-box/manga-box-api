﻿namespace MangaBox.WebApi.ApiModels.Responses;

public class MangaProvider
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
