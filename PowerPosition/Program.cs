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
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddCommandLine(args);
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
