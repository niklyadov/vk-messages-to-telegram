using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;

namespace VkToTg
{
    public class Program  // Hello !
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .UseSerilog((hostingContext, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
                .UseWindowsService()
                .Build()
                .Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {

                    IConfiguration configuration = hostContext.Configuration;

                    var appConfiguration = configuration.GetSection("AppConfiguration");
                    services.Configure<Models.Configuration>(appConfiguration);

                    #region Register & Configure Telegram Bot
                    
                    var botToken = appConfiguration
                        .GetSection("TelegramBot")
                        .GetSection("Token")
                        .Value;
                    
                    if (string.IsNullOrEmpty(botToken))
                    {
                        throw new System.Exception("Please, provide the telegram bot token in appsettigns.json");
                    }

                    services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
                    #endregion

                    services.AddSingleton<Services.Telegram.AccessManager>();
                    services.AddSingleton<Services.Telegram.CommandsManager>();
                    services.AddSingleton<Services.Telegram.UpdatesReceiver>();
                    services.AddSingleton<Services.Telegram.ErrorsReceiver>();

                    services.AddSingleton<Services.Vk.VkCacheService>();
                    services.AddSingleton<Services.Vk.ConversationReceiver>();
                    services.AddSingleton<Services.Vk.MessagesReceiver>();
                    services.AddSingleton<Services.Vk.MessagesSender>();

                    services.AddHostedService<Workers.TelegramBotWorker>();
                    services.AddHostedService<Workers.VkMessagesMonitoringWorker>();
                });
    }
}