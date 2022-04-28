using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VkToTg.Attributes;
using VkToTg.Commands.Core;

namespace VkToTg.Commands
{
    [Command("unreadconv", "View all conversations with unreaded messages.")]
    public class UnreadConversations : BaseCommand
    {
        public UnreadConversations(IServiceScopeFactory serviceScopeFactory, ITelegramBotClient botClient) : base(serviceScopeFactory, botClient)
        {
        }

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            using var scope = ServiceScopeFactory.CreateScope();

            var accessManager = scope.ServiceProvider.GetService<Services.Telegram.AccessManager>();
            if (!accessManager.HasAccess(message.Chat.Id)) return;

            var vkConversations = scope.ServiceProvider.GetService<Services.Vk.ConversationReceiver>();
            var msg = string.Join("\n", vkConversations.GetUnreadedConversations());

            await BotClient.SendTextMessageAsync(message.Chat, msg);
        }
    }
}