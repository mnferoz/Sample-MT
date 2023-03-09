using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample_MT.Components.Consumers;
using Sample_MT.Service;
using System.Diagnostics;

var isService = !(Debugger.IsAttached || args.Contains("Console"));

var builder = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", true);
        config.AddEnvironmentVariables();

        if (args != null)
            config.AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
            cfg.AddBus(ConfigureBus);
        });

        services.AddHostedService<MassTransitConsoleHostedService>();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        logging.AddConsole();
    });

if (!isService)
    await builder.UseWindowsService().Build().RunAsync();
else
    await builder.RunConsoleAsync();


static IBusControl ConfigureBus(IServiceProvider provider)
{
    return Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.ConfigureEndpoints((IBusRegistrationContext)provider);
    });
}
