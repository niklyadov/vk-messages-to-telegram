using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using VkToTg.Models;
using VkToTg.Services.Vk;

namespace VkToTg.Workers
{
    public class VkMessagesMonitoringWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConversationReceiver _conversationReceiver;
        private readonly TelegramBotConfiguration _botConfiguration;
        private readonly VkAccountConfiguration _vkConfiguration;

        public VkMessagesMonitoringWorker(ILogger<VkMessagesMonitoringWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _telegramBotClient = serviceProvider.GetService<ITelegramBotClient>();
            _conversationReceiver = serviceProvider.GetService<ConversationReceiver>();

            var configurationOptions = serviceProvider.GetService<IOptions<Configuration>>();
            _botConfiguration = configurationOptions.Value.TelegramBot;
            _vkConfiguration = configurationOptions.Value.VkAccount;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Vk Messages Monitoring worker.");

            long lastUnreadedCount = -1;

            while (!stoppingToken.IsCancellationRequested)
            {
                long unreadedCount = _conversationReceiver.GetUnreadedMessagesCount();
                if (unreadedCount != 0 && lastUnreadedCount != -1 && unreadedCount > lastUnreadedCount)
                {
                    await _telegramBotClient.SendTextMessageAsync(_botConfiguration.AllowedChatId, 
                        $"You have {unreadedCount} unreaded messages (+ {unreadedCount - lastUnreadedCount})");
                }
                lastUnreadedCount = unreadedCount;

                await Task.Delay(_vkConfiguration.NewMessagesMonitoringIntervalInMinutes * 1000 * 60);
            }
        }
    }
}
