using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PowerPosition;
using Axpo;

public class PositionServiceTests
{
    private readonly Mock<ILogger<PositionService>> _loggerMock;
    private readonly Mock<IPowerServiceWrapper> _powerServiceWrapperMock;
    private readonly Mock<IOptions<PositionServiceOptions>> _optionsMock;
    private readonly PositionServiceOptions _options;

    public PositionServiceTests()
    {
        _loggerMock = new Mock<ILogger<PositionService>>();
        _powerServiceWrapperMock = new Mock<IPowerServiceWrapper>();
        _options = new PositionServiceOptions
        {
            IntervalInSeconds = 60,
            RetryLimitInSeconds = 300,
            RetryDelayInMilliseconds = 1000,
            Location = "Europe/Berlin",
            OutputFilePath = "C:\\Temp"
        };
        _optionsMock = new Mock<IOptions<PositionServiceOptions>>();
        _optionsMock.Setup(o => o.Value).Returns(_options);
    }

    [Fact]
    public void GetPositions_ShouldReturnCorrectPositions()
    {
        // Arrange
        var positionService = new PositionService(_loggerMock.Object, _powerServiceWrapperMock.Object, _optionsMock.Object);
        var date = DateTime.UtcNow;
        var trades = new List<PowerTrade>
        {
            PowerTrade.Create(date, 24),
            PowerTrade.Create(date, 24),
        };
        for (int p = 0; p < 24; p++)
        {
            trades[0].Periods[p].SetVolume(100 * (p + 1));
            trades[1].Periods[p].SetVolume(100 * (24 - p));
        }
        // Act
        var positions = positionService.GetPositions(trades);

        // Assert
        Assert.NotNull(positions);
        Assert.Equal(24, positions.Count());
        foreach (var position in positions)
        {
            Assert.Equal(2500, position.Volume);
        }
    }

    [Fact]
    public async Task GetTradesWithRetry_ShouldRetry_OnException()
    {
        // Arrange
        var positionService = new PositionService(_loggerMock.Object, _powerServiceWrapperMock.Object, _optionsMock.Object);
        var tradedDate = DateTime.UtcNow;
        var requestDate = DateTime.UtcNow;

        _powerServiceWrapperMock.SetupSequence(p => p.GetTradesAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Test exception"))
            .ReturnsAsync([]);

        // Act
        var result = await positionService.GetTradesWithRetry(tradedDate, requestDate);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed. Retrying in")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
        Assert.NotNull(result);
    }
}
