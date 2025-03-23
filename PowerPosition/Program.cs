using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Axpo;
using PowerPosition;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        var switchMappings = new Dictionary<string, string>
        {
            { "-i", "PositionServiceOptions:IntervalInSeconds" },
            { "--interval", "PositionServiceOptions:IntervalInSeconds" },
            { "-rl", "PositionServiceOptions:RetryLimitInSeconds" },
            { "--retryLimit", "PositionServiceOptions:RetryLimitInSeconds" },
            { "-rd", "PositionServiceOptions:RetryDelayInMilliseconds" },
            { "--retryDelay", "PositionServiceOptions:RetryDelayInMilliseconds" },
            { "-l", "PositionServiceOptions:Location" },
            { "--location", "PositionServiceOptions:Location" },
            { "-f", "PositionServiceOptions:OutputFilePath" },
            { "--outputFolder", "PositionServiceOptions:OutputFilePath" }
        };

        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddCommandLine(args, switchMappings);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<PositionServiceOptions>(context.Configuration.GetSection("PositionServiceOptions"));
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<PowerService>();
        services.AddSingleton<IPowerServiceWrapper, PowerServiceWrapper>();
        services.AddHostedService<PositionService>();
    })
    .Build();

await host.RunAsync();
