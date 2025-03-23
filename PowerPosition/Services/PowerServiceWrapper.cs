using Axpo;

namespace PowerPosition;

/// <summary>
/// Interface for wrapping the PowerService to retrieve power trades.
/// </summary>
public interface IPowerServiceWrapper
{
    /// <summary>
    /// Asynchronously gets the power trades for a specified date.
    /// </summary>
    /// <param name="date">The date for which to retrieve the power trades.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the power trades.</returns>
    Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date);
}

/// <summary>
/// Implementation of the IPowerServiceWrapper interface.
/// </summary>
/// <param name="powerService">An Axpo.PowerService instance.</param>
public class PowerServiceWrapper(PowerService powerService) : IPowerServiceWrapper
{
    private readonly PowerService _powerService = powerService;

    /// <inheritdoc />
    public async Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date)
    {
        return await _powerService.GetTradesAsync(date);
    }
}
