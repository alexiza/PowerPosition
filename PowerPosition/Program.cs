using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
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
        services.AddSingleton<PowerService>();
        services.AddHostedService<PositionService>();
    })
    .Build();

await host.RunAsync();
