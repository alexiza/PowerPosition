using Axpo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace PowerPosition;

/// <summary>
/// Background service for processing and saving power positions.
/// </summary>
public class PositionService(ILogger<PositionService> logger, IPowerServiceWrapper powerService, IOptions<PositionServiceOptions> options)
    : BackgroundService
{
    private readonly ILogger<PositionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPowerServiceWrapper _powerService = powerService ?? throw new ArgumentNullException(nameof(powerService));
    private readonly PositionServiceOptions _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(options.Value.Location);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var requestDate = DateTime.UtcNow;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tradedDate = requestDate.Date.AddDays(1);
                var trades = await GetTradesWithRetry(tradedDate, requestDate);
                var positions = CalculatePositions(trades);
                await SavePositionsFile(tradedDate, requestDate, positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the background service.");
            }
            requestDate += TimeSpan.FromSeconds(_options.IntervalInSeconds);
            var remaining = requestDate - DateTime.UtcNow;
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Gets the power trades with retry logic.
    /// </summary>
    /// <param name="tradedDate">The date of the trades.</param>
    /// <param name="requestDate">The request date.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the power trades.</returns>
    internal async Task<IEnumerable<PowerTrade>> GetTradesWithRetry(DateTime tradedDate, DateTime requestDate)
    {
        var delay = _options.RetryDelayInMilliseconds;
        var limitDate = requestDate.AddSeconds(_options.RetryLimitInSeconds);
        while (DateTime.UtcNow < limitDate)
        {
            try
            {
                return await _powerService.GetTradesAsync(tradedDate);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{MethodName} failed. Retrying in {Delay} ms...", nameof(_powerService.GetTradesAsync), delay);
                await Task.Delay(delay);
            }
        }
        throw new TimeoutException($"Failed to get trades for {requestDate:yyyy-MM-ddTHH:mm:ssZ} within the time limit.");
    }

    /// <summary>
    /// Calculates the positions from the power trades.
    /// </summary>
    /// <param name="trades">The power trades.</param>
    /// <returns>A collection of positions.</returns>
    internal IEnumerable<Position> CalculatePositions(IEnumerable<PowerTrade> trades)
    {
        Dictionary<DateTime, double> positions = [];
        foreach (var trade in trades)
        {
            var tradeDateLocal = DateTime.SpecifyKind(trade.Date, DateTimeKind.Unspecified);
            var tradeDateUtc = TimeZoneInfo.ConvertTimeToUtc(tradeDateLocal, _timeZoneInfo);

            foreach (var period in trade.Periods)
            {
                var periodDate = tradeDateUtc.AddHours(period.Period - 1);
                if (positions.ContainsKey(periodDate))
                {
                    positions[periodDate] += period.Volume;
                }
                else
                {
                    positions.Add(periodDate, period.Volume);
                }
            }
        }
        return positions.Select(v => new Position(v.Key, v.Value)).OrderBy(v => v.Date);
    }

    /// <summary>
    /// Saves the positions to a CSV file.
    /// </summary>
    /// <param name="tradedDate">The traded date.</param>
    /// <param name="requestDate">The request date.</param>
    /// <param name="positions">The positions to save.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SavePositionsFile(DateTime tradedDate, DateTime requestDate, IEnumerable<Position> positions)
    {
        var fileName = $"PowerPosition_{tradedDate:yyyyMMdd}_{requestDate:yyyyMMddHHmm}.csv";
        var filePath = Path.Combine(_options.OutputFilePath, fileName);
        Directory.CreateDirectory(_options.OutputFilePath);

        using var writer = new StreamWriter(filePath);
        await writer.WriteLineAsync("Datetime;Volume");
        foreach (var position in positions)
        {
            await writer.WriteLineAsync($"{position.Date:yyyy-MM-ddTHH:mm:ssZ};{position.Volume.ToString("F", CultureInfo.InvariantCulture)}");
        }

        _logger.LogInformation($"Positions saved to {filePath}");
    }
}
