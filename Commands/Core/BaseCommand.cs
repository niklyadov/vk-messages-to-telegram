using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace VkToTg.Commands.Core
{
    public abstract class BaseCommand
    {
        protected IServiceProvider ServiceProvider { get; }
        protected ITelegramBotClient TelegramBotClient { get; }

        public BaseCommand(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            TelegramBotClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
        }

        public virtual Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public static string ExtractCommandName(string inputMessage)
        {
            if (!inputMessage.StartsWith('/')) return null;

            inputMessage = inputMessage.Substring(1).ToLower();

            var spacePosition = inputMessage.IndexOf(' ');
            if (spacePosition != -1) return inputMessage.Substring(0, spacePosition);

            return inputMessage;
        }
    }
}