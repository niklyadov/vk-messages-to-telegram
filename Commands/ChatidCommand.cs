using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VkToTg.Attributes;
using VkToTg.Commands.Core;

namespace VkToTg.Commands
{
    [Command("chatid", "View current chat id.")]
    public class ChatidCommand : BaseCommand
    {
        public ChatidCommand(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        public override async Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(message.Chat, $"This chat id is: <code>{message.Chat.Id}</code>", 
                Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}