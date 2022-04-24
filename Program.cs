using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VkToTg
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {

                    IConfiguration configuration = hostContext.Configuration;

                    services.Configure<Models.Configuration>(configuration.GetSection("AppConfiguration"));
                    services.AddSingleton<Services.TlgAccessManager>();
                    services.AddSingleton<Services.VkConversationReceiver>();
                    services.AddSingleton<Services.VkMessagesReceiver>();
                    services.AddSingleton<Services.TlgUpdatesReceiver>();
                    services.AddSingleton<Services.TlgErrorsReceiver>();
                    services.AddHostedService<MainWorker>();
                });
    }
}