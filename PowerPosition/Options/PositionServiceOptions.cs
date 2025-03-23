namespace PowerPosition;

/// <summary>
/// Options for configuring the PositionService.
/// </summary>
public class PositionServiceOptions
{
    /// <summary>
    /// Gets or sets the interval in seconds at which the service should run.
    /// </summary>
    public int IntervalInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the retry limit in seconds.
    /// </summary>
    public int RetryLimitInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the retry delay in milliseconds.
    /// </summary>
    public int RetryDelayInMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the location for the time zone.
    /// </summary>
    public required string Location { get; set; }

    /// <summary>
    /// Gets or sets the output file path for the CSV file.
    /// </summary>
    public required string OutputFilePath { get; set; }
}
