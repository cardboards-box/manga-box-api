namespace MangaBox.Core.Requesting;

/// <summary>
/// Represents a bad result
/// </summary>
public class ExceptionResult : RequestResult<string[]>
{
    /// <summary>
    /// Represents a bad result
    /// </summary>
    /// <param name="code">The status code of the request</param>
    /// <param name="data">The data result of the request</param>
    /// <param name="message">Any error or message associated with the result</param>
    public ExceptionResult(
        HttpStatusCode code,
        string[]? data = null,
        string? message = null) : base(
            code,
            data ?? Array.Empty<string>(),
            message ?? Requests.EXCEPTION)
    { }
}
