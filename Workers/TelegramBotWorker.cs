using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace VkToTg.Workers
{
    public class TelegramBotWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IServiceProvider _serviceProvider;

        public TelegramBotWorker (ILogger<TelegramBotWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _telegramBotClient = serviceProvider.GetService<ITelegramBotClient>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramBotClient.StartReceiving(
                _serviceProvider.GetRequiredService<Services.Telegram.UpdatesReceiver>().Handle,
                _serviceProvider.GetRequiredService<Services.Telegram.ErrorsReceiver>().Handle,
                new ReceiverOptions { AllowedUpdates = { } }, // receive all update types
                stoppingToken
            );

            _logger.LogInformation($"Bot {_telegramBotClient.GetMeAsync().Result.FirstName} started");

            #region Register Commands List For Client

            var commandsManager = _serviceProvider.GetRequiredService<Services.Telegram.CommandsManager>();
            var commandsAttribure = commandsManager.GetAllCommandsAttribute();

            await _telegramBotClient.SetMyCommandsAsync(commandsAttribure
                .Where(a => !string.IsNullOrEmpty(a.Name) && !string.IsNullOrEmpty(a.Description))
                .Select(a => new BotCommand()
                {
                    Command = a.Name,
                    Description = a.Description
                })
            );

            #endregion
        }
    }
}