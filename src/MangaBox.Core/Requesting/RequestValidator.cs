namespace MangaBox.Core.Requesting;

/// <summary>
/// Validates a request
/// </summary>
public class RequestValidator
{
    /// <summary>
    /// The validation methods
    /// </summary>
    private readonly List<Validator> validators = new();

    /// <summary>
    /// All of the issues that occurred during validation
    /// </summary>
    public string[] Issues => GetIssues().ToArray();

    /// <summary>
    /// Whether or not the request was valid
    /// </summary>
    public bool Valid => !Issues.Any();

    /// <summary>
    /// Adds the given validation to the validator
    /// </summary>
    /// <param name="valid">The validation function</param>
    /// <param name="message">The message to return if the validation function failed</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator Add(Func<bool> valid, string message)
    {
        validators.Add(new Validator(valid, message));
        return this;
    }

    /// <summary>
    /// Validates the given value to ensure it's not null, empty, or whitespace
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator NotNull(string? value, string property)
    {
        validators.Add(new Validator(() => !string.IsNullOrWhiteSpace(value), $"{property} cannot be null or empty."));
        return this;
    }

    /// <summary>
    /// Validates the given value to ensure it matches one of the given options
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <param name="options">The options the value should be one of</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator OneOf(string? value, string property, params string[] options)
    {
        validators.Add(new Validator(() => !string.IsNullOrWhiteSpace(value) && options.Contains(value), $"{property} must be one of {string.Join(", ", options)}"));
        return this;
    }

    /// <summary>
    /// Validates the given value is greater than the max value
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <param name="min">The minimum value</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator GreaterThan(int value, string property, int min)
    {
        validators.Add(new Validator(() => value > min, $"{property} must be greater than {min}"));
        return this;
    }

    /// <summary>
    /// Validates the given value is less than the max value
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <param name="max">The maximum value</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator LessThan(int value, string property, int max)
    {
        validators.Add(new Validator(() => value < max, $"{property} must be less than {max}"));
        return this;
    }

    /// <summary>
    /// Validates the given value is between than the min and max values
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator Between(int value, string property, int min, int max)
    {
        validators.Add(new Validator(() => value > min && value < max, $"{property} must be greater than {min} but less than {max}"));
        return this;
    }

    /// <summary>
    /// Validates the given value is greater than the max value
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <param name="min">The minimum value</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator GreaterThan(double value, string property, double min)
    {
        validators.Add(new Validator(() => value > min, $"{property} must be greater than {min}"));
        return this;
    }

    /// <summary>
    /// Validates the given value is less than the max value
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <param name="max">The maximum value</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator LessThan(double value, string property, double max)
    {
        validators.Add(new Validator(() => value < max, $"{property} must be less than {max}"));
        return this;
    }

    /// <summary>
    /// Validates the given value is between than the min and max values
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator Between(double value, string property, double min, double max)
    {
        validators.Add(new Validator(() => value > min && value < max, $"{property} must be greater than {min} but less than {max}"));
        return this;
    }

    /// <summary>
    /// Validates the given value is a valid GUID
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator IsGuid(string? value, string property)
    {
        validators.Add(new Validator(() => !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out _), $"{property} must be a valid GUID"));
        return this;
    }

    /// <summary>
    /// Gets all of the issues that occurred in the validation
    /// </summary>
    /// <returns>All of the issues that were found</returns>
    public IEnumerable<string> GetIssues()
    {
        foreach (var validator in validators)
            if (!validator.Valid())
                yield return validator.Message;
    }

    /// <summary>
    /// Whether or not the request is valid
    /// </summary>
    /// <param name="result">The request results to return if the request isn't valid</param>
    /// <returns>Whether or not the request is valid</returns>
    public bool IsValid(out RequestResult result)
    {
        var issues = Issues;

        result = Requests.BadRequest(issues);
        return issues.Length == 0;
    }

    /// <summary>
    /// Represents a validation function
    /// </summary>
    /// <param name="Valid">The validator function</param>
    /// <param name="Message">The message to return if the validation function failed</param>
    private record class Validator(Func<bool> Valid, string Message);
}
