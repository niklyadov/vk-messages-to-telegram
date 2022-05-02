using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VkToTg.Models;

namespace VkToTg.Commands.Core
{
    public abstract class BaseCommand
    {
        protected IServiceProvider ServiceProvider { get; }
        protected ITelegramBotClient TelegramBotClient { get; }
        protected ILogger<BaseCommand> Logger { get; }
        protected TelegramBotConfiguration BotConfiguration { get; }
        protected VkConfiguration VkConfiguration { get; }

        public BaseCommand(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            TelegramBotClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
            Logger = serviceProvider.GetRequiredService< ILogger<BaseCommand> >();

            var configurationOptions = serviceProvider.GetService<IOptions<Configuration>>();
            BotConfiguration = configurationOptions.Value.TelegramBot;
            VkConfiguration = configurationOptions.Value.Vk;
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