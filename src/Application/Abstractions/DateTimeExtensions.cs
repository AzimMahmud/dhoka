namespace Application.Abstractions;

public static class DateTimeExtensions
{
    public static string ToRelativeTime(DateTime dt)
    {
        TimeSpan span = DateTime.UtcNow - dt.ToUniversalTime();
        if (span.TotalDays >= 365)
        {
            int years = (int)(span.TotalDays / 365);
            return $"Created {years} year{(years>1?"s":"")} ago";
        }
        if (span.TotalDays >= 30)
        {
            int months = (int)(span.TotalDays / 30);
            return $"Created {months} month{(months>1?"s":"")} ago";
        }
        if (span.TotalDays >= 1)
        {
            int days = (int)span.TotalDays;
            return $"Created {days} day{(days>1?"s":"")} ago";
        }
        if (span.TotalHours >= 1)
        {
            int hours = (int)span.TotalHours;
            return $"Created {hours} hour{(hours>1?"s":"")} ago";
        }
        if (span.TotalMinutes >= 1)
        {
            int mins = (int)span.TotalMinutes;
            return $"Created {mins} min{(mins>1?"s":"")} ago";
        }
        int secs = (int)span.TotalSeconds;
        return $"Created {secs} sec{(secs>1?"s":"")} ago";
    }
}
