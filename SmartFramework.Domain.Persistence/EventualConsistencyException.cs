using ErrorOr;

namespace SmartFramework.Domain.Persistence;

public class EventualConsistencyException : Exception
{
    public Error EventualConsistencyError { get; }
    public List<Error> UnderlyingErrors { get; }

    public EventualConsistencyException(Error eventualConsistencyError, List<Error>? underlyingErrors = null) : base(message: eventualConsistencyError.Description)
    {
        EventualConsistencyError = eventualConsistencyError;
        UnderlyingErrors = underlyingErrors ?? new();
    }
}

public static class EventualConsistencyError
{
    public const int EventualConsistencyType = 100;

    public static Error From(string code, string description)
    {
        return Error.Custom(EventualConsistencyType, code, description);
    }
}