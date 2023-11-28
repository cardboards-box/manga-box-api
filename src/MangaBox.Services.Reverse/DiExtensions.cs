namespace MangaBox.Services.Reverse;

using GoogleVision;
using MatchApi;
using NsfwCheck;
using SauceNao;

public static class DiExtensions
{
    public static IDependencyResolver AddReverseImage(this IDependencyResolver builder)
    {
        return builder
            .Transient<IReverseSearchService, ReverseSearchService>()
            .Transient<IMatchApiService, MatchApiService>()
            .Transient<IMatchService, MatchService>()
            .Transient<ISauceNaoApiService, SauceNaoApiService>()
            .Transient<INsfwApiService, NsfwApiService>()
            .Transient<IGoogleVisionService, GoogleVisionService>()
            .Transient<IMatchIndexingService, MatchIndexingService>();
    }
}
