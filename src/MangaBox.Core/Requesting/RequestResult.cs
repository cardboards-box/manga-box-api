namespace MangaBox.Core.Requesting;

/// <summary>
/// Represents the result of a request
/// </summary>
public class RequestResult
{
    /// <summary>
    /// Whether or not the request was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success => (int)Code >= 200 && (int)Code < 300;

    /// <summary>
    /// The status code of the request
    /// </summary>
    [JsonPropertyName("code")]
    public HttpStatusCode Code { get; set; }

    /// <summary>
    /// Any error or message associated with the result
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>Represents the result of a request</summary>
    public RequestResult() { }

    /// <summary>Represents the result of a request</summary>
    /// <param name="code">The status code of the request</param>
    /// <param name="message">Any error or message associated with the result</param>
    public RequestResult(HttpStatusCode code, string? message = null)
    {
        Code = code;
        Message = message;
    }
}

/// <summary>
/// Represents the result of a request that returns data
/// </summary>
/// <typeparam name="T">The type of data</typeparam>
public class RequestResult<T> : RequestResult
{
    /// <summary>
    /// The data result of the request
    /// </summary>
    public T Data { get; set; }

    /// <summary>Represents the result of a request</summary>
    /// <param name="data">The data result of the request</param>
    public RequestResult(T data) : this(HttpStatusCode.OK, data) { }

    /// <summary>Represents the result of a request</summary>
    /// <param name="code">The status code of the request</param>
    /// <param name="data">The data result of the request</param>
    /// <param name="message">Any error or message associated with the result</param>
    public RequestResult(HttpStatusCode code, T data, string? message = null) : base(code, message)
    {
        Data = data;
    }
}
