using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DiscordPrtScrShareApp
{
    class Program
    {

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(@"C:\TEMP\WorkerService\Log.txt")
                .CreateLogger();

            try
            {
                Log.Information("Starting up the service");

                if (args.Length < 3)
                {
                    Log.Information("Please provide all three parameters.");
                    Log.Information("Usage: String <DiscordBotToken> String <ChannelId> Int <VirtualKeyTrigger>");
                    return;
                }
                if (!int.TryParse(args[2], out var vkey))
                {
                    Log.Information("The third parameter must be an integer.");
                    Log.Information("Usage: String <DiscordBotToken> String <ChannelId> Int <VirtualKeyTrigger>");
                    return;
                }

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"There was a problem starting the service : {ex.Message}");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService(serviceProvider => new Worker(
                            serviceProvider.GetService<ILogger<Worker>>(), args
                        ));
                })
                .UseSerilog();
    }
}
