namespace SharedKernel;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    string ToRelativeTime(DateTime dt);
}
