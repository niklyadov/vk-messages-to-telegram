using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace VkToTg.Services.Telegram
{
    public class UpdatesReceiver
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;

        public UpdatesReceiver (IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<UpdatesReceiver>();
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
            Commands.Core.BaseCommand command = null;
            
            try
            {
                var commandType = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsDefined(typeof(Attributes.CommandAttribute)))
                .First(t => t.GetCustomAttributes<Attributes.CommandAttribute>()
                                .Count(a => a.V == message.Text.Substring(1).ToLower()) > 0);

                command = Activator.CreateInstance(commandType, _serviceScopeFactory) as Commands.Core.BaseCommand;
            } catch (Exception ex) when (command == null)
            {
                command = new Commands.DefaultCommand(_serviceScopeFactory);

                if(ex != null)
                {
                    _logger.LogError($"Failed to find command {ex.Message}.");
                }
            }
            
            try
            {
                await command.Execute(botClient, message, cancellationToken);
            } catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat, "Something went wrong. See console.");
                _logger.LogError($"Failed to execute command {ex.Message}");
            }
        }
    }
}