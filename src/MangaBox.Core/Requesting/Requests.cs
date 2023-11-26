namespace MangaBox.Core.Requesting;

/// <summary>
/// A helper class for handling results and corresponding codes
/// </summary>
public static class Requests
{
    #region Generic response codes
    /// <summary>
    /// Request was fine and has data associated with it
    /// </summary>
    public const string OK = "ok";
    /// <summary>
    /// Request was fine but there was no content or change that happened
    /// </summary>
    public const string NO_CONTENT = "ok-no-content";
    /// <summary>
    /// Request resulted in a resource being created
    /// </summary>
    public const string CREATED = "resource-created";
    /// <summary>
    /// Request resulted in an exception being thrown
    /// </summary>
    public const string EXCEPTION = "unknown-error-occurred";
    /// <summary>
    /// Request was for a resource that does not exist
    /// </summary>
    public const string NOT_FOUND = "resource-not-found";
    /// <summary>
    /// User that made the request is not authorized to do so
    /// </summary>
    public const string UNAUTHORIZED = "not-authorized";
    /// <summary>
    /// The request contained invalid data
    /// </summary>
    public const string BAD_REQUEST = "user-input-invalid";
    /// <summary>
    /// The request resulted in conflicting data
    /// </summary>
    public const string CONFLICT = "conflicting-data";
    #endregion

    /// <summary>
    /// Result was successful and changes were made
    /// </summary>
    /// <param name="message">The message to use</param>
    /// <returns></returns>
    public static RequestResult Ok(string? message = null) => new(HttpStatusCode.OK, message ?? NO_CONTENT);

    /// <summary>
    /// Request was successful, changes were made, and there is data associated with the request
    /// </summary>
    /// <typeparam name="T">The type of data returned</typeparam>
    /// <param name="data">The data returned by the request</param>
    /// <returns></returns>
    public static RequestResult<T> Ok<T>(T data) => new(HttpStatusCode.OK, data, OK);

    /// <summary>
    /// Request resulted in an error and there are issues associated with it
    /// </summary>
    /// <param name="issues">The issues that occurred with the request</param>
    /// <returns></returns>
    public static ExceptionResult Exception(params string[] issues) => new(HttpStatusCode.InternalServerError, issues);

    /// <summary>
    /// Request resulted in an error and there are issues associated with it
    /// </summary>
    /// <param name="errors">The issues that occurred with the request</param>
    /// <returns></returns>
    public static ExceptionResult Exception(params Exception[] errors)
        => new(HttpStatusCode.InternalServerError, errors.Select(t => t.Message).ToArray());

    /// <summary>
    /// The requested resource was not found
    /// </summary>
    /// <param name="resources">The resource(s) that were requested</param>
    /// <returns></returns>
    public static ExceptionResult NotFound(params string[] resources) => new(HttpStatusCode.NotFound, resources, NOT_FOUND);

    /// <summary>
    /// The application was not authorized to make the request
    /// </summary>
    /// <param name="issues">The issues associated with the request</param>
    /// <returns></returns>
    public static ExceptionResult Unauthorized(params string[] issues) => new(HttpStatusCode.Unauthorized, issues, UNAUTHORIZED);

    /// <summary>
    /// The request contained invalid data
    /// </summary>
    /// <param name="validator">The validator that found the issues</param>
    /// <returns></returns>
    public static ExceptionResult BadRequest(RequestValidator validator) => BadRequest(validator.Issues);

    /// <summary>
    /// The request contained invalid data
    /// </summary>
    /// <param name="issues">The issues that were found with the data</param>
    /// <returns></returns>
    public static ExceptionResult BadRequest(params string[] issues) => new(HttpStatusCode.BadRequest, issues, BAD_REQUEST);

    /// <summary>
    /// The request resulted in a conflict
    /// </summary>
    /// <param name="issues">The issues that were found with the data</param>
    /// <returns></returns>
    public static ExceptionResult Conflict(params string[] issues) => new(HttpStatusCode.Conflict, issues, CONFLICT);

    /// <summary>
    /// Resource was created
    /// </summary>
    /// <returns></returns>
    public static RequestResult Created() => new(HttpStatusCode.Created, CREATED);

    /// <summary>
    /// Checks the given validator to ensure the request is valid
    /// </summary>
    /// <param name="validator">The validator</param>
    /// <param name="results">The request results</param>
    /// <returns>Whether the request was valid or not</returns>
    public static bool Validate(RequestValidator validator, out RequestResult results)
    {
        var issues = validator.Issues;
        if (issues.Length <= 0)
        {
            results = Ok();
            return true;
        }

        results = BadRequest(issues);
        return false;
    }
}
