using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using VkToTg.Models;

namespace VkToTg
{
    public class MainWorker : BackgroundService
    {
        private readonly ILogger<MainWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TelegramBotConfiguration _configuration;
        private readonly ITelegramBotClient _bot;


        public MainWorker(ILogger<MainWorker> logger, 
                          IServiceScopeFactory serviceScopeFactory, 
                          IOptions<Configuration> configurationOptions)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configurationOptions.Value.TelegramBot;

            try
            {
                _bot = new TelegramBotClient(_configuration.BotToken);
            } catch (Exception ex)
            {
                _logger.LogError($"Failed to create bot. {ex.Message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            if(_bot == null)
            {
                return;
            }

            _bot.StartReceiving(
                scope.ServiceProvider.GetRequiredService<Services.Telegram.UpdatesReceiver>().Handle,
                scope.ServiceProvider.GetRequiredService<Services.Telegram.ErrorsReceiver>().Handle,
                new ReceiverOptions { AllowedUpdates = { } }, // receive all update types
                stoppingToken
            );

            _logger.LogInformation($"Bot {_bot.GetMeAsync().Result.FirstName} started");
        }
    }
}