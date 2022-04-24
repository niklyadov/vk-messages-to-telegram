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
                    services.AddSingleton<Services.Telegram.AccessManager>();
                    services.AddSingleton<Services.Telegram.UpdatesReceiver>();
                    services.AddSingleton<Services.Telegram.ErrorsReceiver>();
                    services.AddSingleton<Services.Vk.ConversationReceiver>();
                    services.AddSingleton<Services.Vk.MessagesReceiver>();
                    services.AddHostedService<MainWorker>();
                });
    }
}