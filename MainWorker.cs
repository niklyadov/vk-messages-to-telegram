using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using VkToTg.Models;

namespace VkToTg
{
    public class MainWorker : BackgroundService
    {
        private readonly ILogger<MainWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TelegramBotConfiguration _botConfiguration;
        private readonly VkAccountConfiguration _accountConfiguration;
        private readonly ITelegramBotClient _bot;


        public MainWorker(ILogger<MainWorker> logger, 
                          IServiceScopeFactory serviceScopeFactory, 
                          IOptions<Configuration> configurationOptions)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _botConfiguration = configurationOptions.Value.TelegramBot;
            _accountConfiguration = configurationOptions.Value.VkAccount;

            try
            {
                _bot = new TelegramBotClient(_botConfiguration.BotToken);
            } catch (Exception ex)
            {
                _logger.LogError($"Failed to create bot. {ex.Message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await RunBot(stoppingToken);

            _ = Task.Run(() => NewMessagesMonitoring(stoppingToken), stoppingToken);
        }

        protected async Task NewMessagesMonitoring(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var conversationsService = scope.ServiceProvider.GetRequiredService<Services.Vk.ConversationReceiver>();
            long lastUnreadedCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                long unreadedCount = conversationsService.GetUnreadedMessagesCount();
                if (unreadedCount != 0 && unreadedCount > lastUnreadedCount)
                {
                    await _bot.SendTextMessageAsync(_botConfiguration.AllowedChatId, $"You have {unreadedCount} unreaded messages (+ {unreadedCount - lastUnreadedCount})");
                }
                lastUnreadedCount = unreadedCount;

                await Task.Delay(_accountConfiguration.NewMessagesMonitoringInterval);
            }

        }

        protected async Task RunBot(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            if (_bot == null)
            {
                return;
            }

            _bot.StartReceiving(
                scope.ServiceProvider.GetRequiredService<Services.Telegram.UpdatesReceiver>().Handle,
                scope.ServiceProvider.GetRequiredService<Services.Telegram.ErrorsReceiver>().Handle,
                new ReceiverOptions { AllowedUpdates = { } }, // receive all update types
                stoppingToken
            );

            #region register commands list for telegram client

            var commandsManager = scope.ServiceProvider.GetRequiredService<Services.Telegram.CommandsManager>();
            var commandsAttribure = commandsManager.GetAllCommandsAttribute();

            await _bot.SetMyCommandsAsync(commandsAttribure
                .Where(a => !string.IsNullOrEmpty(a.Name) && !string.IsNullOrEmpty(a.Description))
                .Select(a => new BotCommand()
                {
                    Command = a.Name,
                    Description = a.Description
                })
            );


            #endregion

            _logger.LogInformation($"Bot {_bot.GetMeAsync().Result.FirstName} started");
        }
    }
}