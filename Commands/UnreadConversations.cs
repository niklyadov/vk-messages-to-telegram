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
        public UnreadConversations(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        public override async Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            using var scope = ServiceScopeFactory.CreateScope();

            var accessManager = scope.ServiceProvider.GetService<Services.Telegram.AccessManager>();
            if (!accessManager.HasAccess(message.Chat.Id)) return;

            var vkConversations = scope.ServiceProvider.GetService<Services.Vk.ConversationReceiver>();
            var msg = string.Join("\n", vkConversations.GetUnreadedConversations());

            await botClient.SendTextMessageAsync(message.Chat, msg);
        }
    }
}