using Dan.Plugin.Arbt;
using Dan.Common.Interfaces;
using Dan.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureDanPluginDefaults()
    .ConfigureAppConfiguration((context, configuration) =>
    {
        // Add more configuration sources if necessary. ConfigureDanPluginDefaults will load environment variables, which includes
        // local.settings.json (if developing locally) and applications settings for the Azure Function
    })
    .ConfigureServices((context, services) =>
    {
        // Add any additional services here
        services.AddLogging();
        services.AddHttpClient();
        
        services.AddHttpClient("SafeHttpClient", client => { client.Timeout = new System.TimeSpan(0, 0, 30); });
        // This makes IOption<Settings> available in the DI container.
        var configurationRoot = context.Configuration;
        
        services.Configure<Settings>(
            configurationRoot.GetSection(nameof(Settings)));
    })
    .Build();

await host.RunAsync();
