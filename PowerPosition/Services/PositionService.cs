using Axpo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PowerPosition;

public class PositionService(ILogger<PositionService> logger, IPowerServiceWrapper powerService, IOptions<PositionServiceOptions> options) 
    : BackgroundService
{
    private readonly ILogger<PositionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPowerServiceWrapper _powerService = powerService ?? throw new ArgumentNullException(nameof(powerService));
    private readonly PositionServiceOptions _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(options.Value.Location);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var requestDate = DateTime.UtcNow;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tradedDate = requestDate.Date.AddDays(1);
                var trades = await GetTradesWithRetry(tradedDate, requestDate);
                var positions = GetPositions(trades);
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

    internal IEnumerable<Position> GetPositions(IEnumerable<PowerTrade> trades)
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

    private async Task SavePositionsFile(DateTime tradedDate, DateTime requestDate, IEnumerable<Position> positions)
    {
        var fileName = $"PowerPosition_{tradedDate:yyyyMMdd}_{requestDate:yyyyMMddHHmm}.csv";
        var filePath = Path.Combine(_options.OutputFilePath, fileName);
        Directory.CreateDirectory(_options.OutputFilePath);

        using (var writer = new StreamWriter(filePath))
        {
            await writer.WriteLineAsync("Datetime;Volume");
            foreach (var position in positions)
            {
                await writer.WriteLineAsync($"{position.Date:yyyy-MM-ddTHH:mm:ssZ};{position.Volume}");
            }
        }

        _logger.LogInformation($"Positions saved to {filePath}");
    }
}