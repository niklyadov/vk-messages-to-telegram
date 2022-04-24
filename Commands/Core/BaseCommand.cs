using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace VkToTg.Commands.Core
{
    public abstract class BaseCommand
    {
        protected IServiceScopeFactory ServiceScopeFactory { get; }

        public BaseCommand(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }

        public virtual Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}