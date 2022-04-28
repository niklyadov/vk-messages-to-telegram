using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VkToTg.Commands.Core;
using VkToTg.Models;

namespace VkToTg.Services.Telegram
{
    public class UpdatesReceiver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UpdatesReceiver> _logger;
        private readonly TelegramBotConfiguration _configurationBot;
        private readonly CommandsManager _commandsManager;

        public UpdatesReceiver (ILogger<UpdatesReceiver> logger, IServiceProvider serviceProvider,
            IOptions<Configuration> configurationOptions)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configurationBot = configurationOptions.Value.TelegramBot;
            _commandsManager = serviceProvider.GetService<CommandsManager>();
        }

        public async Task Handle(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await HandleMessage(botClient, update.Message, cancellationToken);
                        break;
                    case UpdateType.CallbackQuery:
                        await HandleCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to execute command {ex.Message}");

                await botClient.SendTextMessageAsync(_configurationBot.AllowedChatId, "Something went wrong. See console.");
            }
        }

        private async Task HandleMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var commandName = BaseCommand.ExtractCommandName(message.Text);

            if (!string.IsNullOrEmpty(commandName))
            {
                var command = _commandsManager.GetCommandImplementation(commandName, botClient);

                await command.OnMessage(message, cancellationToken);

                return;
            }

            if (_configurationBot.AllowSendMessages)
            {
                var messagesSender = _serviceProvider.GetService<Vk.MessagesSender>();
                var messagesReceiver = _serviceProvider.GetService<Vk.MessagesReceiver>();

                if (messagesReceiver.SelectedConversationId.HasValue)
                {
                    messagesSender.SendMessage(message.Text, messagesReceiver.SelectedConversationId.Value);
                }
            }
        }

        private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var commandName = BaseCommand.ExtractCommandName(callbackQuery.Data);

            if (commandName == null) return;

            var command = _commandsManager.GetCommandImplementation(commandName, botClient);

            await command.OnCallbackQuery(callbackQuery, cancellationToken);
        }
    }
}