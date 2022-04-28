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
        public ChatidCommand(IServiceScopeFactory serviceScopeFactory, ITelegramBotClient botClient) : base(serviceScopeFactory, botClient)
        {
        }

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            await BotClient.SendTextMessageAsync(message.Chat, $"This chat id is: <code>{message.Chat.Id}</code>", 
                Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}