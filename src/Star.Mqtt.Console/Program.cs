using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Star.Mqtt.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(config => config.AddCommandLine(args))
                .ConfigureAppConfiguration(config => config.AddEnvironmentVariables("Star:"))
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services.AddSingleton<Auto.ISource, Auto.Source>();
                        services.AddOptions<Mqtt.Configuration>().Bind(hostContext.Configuration.GetSection("Mqtt"));
                        services.AddSingleton<Mqtt.ISource, Mqtt.Source>();

                        services.AddOptions<Configuration>().Bind(hostContext.Configuration.GetSection("Pi"));
                        services.AddHostedService<Service>();
                    })
                .ConfigureLogging((hostingContext, logging) => logging.AddConsole());

            await builder
                .UseConsoleLifetime()
                .Build()
                .RunAsync();
        }
    }
}
