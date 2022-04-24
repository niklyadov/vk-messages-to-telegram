using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VkToTg.Extensions;

namespace VkToTg.Services
{
    public class TlgUpdatesReceiver
    {
        private IServiceScopeFactory _serviceScopeFactory;

        public TlgUpdatesReceiver (IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
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
            Commands.BaseCommand command;

            var commandName = message.Text.Substring(1).FirstLetterToUpper();
            var commandType = $"VkToTg.Commands.{commandName}Command";
            var objectType = Type.GetType(commandType);

            if (objectType != null)
            {
                command = Activator.CreateInstance(objectType, _serviceScopeFactory) as Commands.BaseCommand;
            }
            else
            {
                command = new Commands.DefaultCommand(_serviceScopeFactory);
            }
            
            try
            {
                await command.Execute(botClient, message, cancellationToken);
            } catch
            {
                await botClient.SendTextMessageAsync(message.Chat, "Something went wrong");
            }
        }
    }
}