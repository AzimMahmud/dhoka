namespace SharedKernel;

public static class RetryHelper
{
    /// <summary>
    /// Retries the provided async operation up to maxAttempts times, with exponential backoff.
    /// If all attempts fail, re‐throws the last exception.
    /// </summary>
    public static async Task RetryOnExceptionAsync(
        Func<Task> operation,
        int maxAttempts = 3,
        int initialDelayMs = 200,
        CancellationToken cancellationToken = default)
    {
        if (maxAttempts <= 0)
        {
            throw new ArgumentException("maxAttempts must be > 0");
        }

        int attempt = 0;
        int delay   = initialDelayMs;
        Exception lastEx = null!;

        while (attempt < maxAttempts)
        {
            try
            {
                await operation();
                return; // success!
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                lastEx = ex;
                attempt++;

                if (attempt >= maxAttempts)
                {
                    break;
                }

                await Task.Delay(delay, cancellationToken);
                delay = Math.Min(delay * 2, 10_000); // cap at 10s
            }
        }

        // if we get here, all attempts failed
        throw lastEx!;
    }
}
