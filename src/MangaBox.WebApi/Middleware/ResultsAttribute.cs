using Microsoft.AspNetCore.Mvc;

namespace MangaBox.WebApi;

/// <summary>
/// Wraps the <see cref="ProducesResponseTypeAttribute"/> for the <see cref="RequestResult"/> types
/// </summary>
public class ResultsAttribute : ProducesResponseTypeAttribute
{
    /// <summary></summary>
    public ResultsAttribute() : this(200) { }

    /// <summary></summary>
    /// <param name="statusCode"></param>
    public ResultsAttribute(int statusCode) : base(typeof(RequestResult), statusCode) { }
}

/// <summary>
/// Wraps the <see cref="ProducesResponseTypeAttribute"/> for the <see cref="RequestResult{T}"/> types
/// </summary>
/// <typeparam name="T"></typeparam>
public class ResultsAttribute<T> : ProducesResponseTypeAttribute
{
    /// <summary></summary>
    public ResultsAttribute() : this(200) { }

    /// <summary></summary>
    /// <param name="statusCode"></param>
    public ResultsAttribute(int statusCode) : base(typeof(RequestResult<T>), statusCode) { }
}

/// <summary>
/// Wraps the <see cref="ProducesResponseTypeAttribute"/> for the <see cref="ExceptionResult"/> types
/// </summary>
public class ExceptionResultsAttribute : ProducesResponseTypeAttribute
{
    /// <summary></summary>
    /// <param name="code"></param>
    public ExceptionResultsAttribute(int code) : base(typeof(ExceptionResult), code) { }

    /// <summary></summary>
    public ExceptionResultsAttribute() : base(typeof(ExceptionResult), 500) { }
}

