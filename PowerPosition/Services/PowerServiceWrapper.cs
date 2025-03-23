using Axpo;

namespace PowerPosition;

public interface IPowerServiceWrapper
{
    Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date);
}

public class PowerServiceWrapper(PowerService powerService) : IPowerServiceWrapper
{
    private readonly PowerService _powerService = powerService;

    public Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date)
    {
        return _powerService.GetTradesAsync(date);
    }
}
