using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace VkToTg.Services.Telegram
{
    public class UpdatesReceiver
    {
        private CommandsManager _commandsManager;
        private ILogger<UpdatesReceiver> _logger;

        public UpdatesReceiver (CommandsManager commandsManager, ILogger<UpdatesReceiver> logger)
        {
            _commandsManager = commandsManager;
            _logger = logger;
        }

        public async Task Handle(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message: 
                    await HandleMessages(botClient, update.Message, cancellationToken);
                    break;
            }
        }

        private async Task HandleMessages(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if(message.Text.StartsWith("/"))
            {
                var command = _commandsManager.GetCommandByName(message.Text.Substring(1).ToLower());

                try
                {
                    await command.Execute(botClient, message, cancellationToken);
                }
                catch (Exception ex)
                {

                    _logger.LogError($"Failed to execute command {ex.Message}");

                    await botClient.SendTextMessageAsync(message.Chat, "Something went wrong. See console.");
                }
            }
        }
    }
}