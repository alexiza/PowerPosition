namespace PowerPosition;

public class PositionServiceOptions
{
    public int IntervalInSeconds { get; set; }
    public int RetryLimitInSeconds { get; set; }
    public int RetryDelayInMilliseconds { get; set; }
    public required string Location { get; set; }
}
