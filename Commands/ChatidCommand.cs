using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace VkToTg.Commands
{
    public class ChatidCommand : BaseCommand
    {
        public ChatidCommand(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        public override async Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(message.Chat, $"{message.Chat.Id}");
        }
    }
}
