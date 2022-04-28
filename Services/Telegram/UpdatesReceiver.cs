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
        private IServiceScopeFactory _serviceScopeFactory;
        private CommandsManager _commandsManager;
        private ILogger<UpdatesReceiver> _logger;
        private readonly TelegramBotConfiguration _configurationBot;

        public UpdatesReceiver (IServiceScopeFactory serviceScopeFactory, 
            CommandsManager commandsManager, ILogger<UpdatesReceiver> logger, IOptions<Configuration> configurationOptions)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _commandsManager = commandsManager;
            _logger = logger;
            _configurationBot = configurationOptions.Value.TelegramBot;
        }

        public async Task Handle(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

        private async Task HandleMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var commandName = BaseCommand.ExtractCommandName(message.Text);

            if (commandName == null) return;

            var command = _commandsManager.GetCommandImplementation(commandName, botClient);

            try
            {
                await command.OnMessage(message, cancellationToken);
            }
            catch (Exception ex)
            {

                _logger.LogError($"Failed to execute command {ex.Message}");

                await botClient.SendTextMessageAsync(message.Chat, "Something went wrong. See console.");
            }
        }

        private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var commandName = BaseCommand.ExtractCommandName(callbackQuery.Data);

            if (commandName == null) return;

            var command = _commandsManager.GetCommandImplementation(commandName, botClient);

            try
            {
                await command.OnCallbackQuery(callbackQuery, cancellationToken);
            }
            catch (Exception ex)
            {

                _logger.LogError($"Failed to execute command {ex.Message}");

                await botClient.SendTextMessageAsync(_configurationBot.AllowedChatId, "Something went wrong. See console.");
            }
        }
    }
}