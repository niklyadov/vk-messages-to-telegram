using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VkToTg.Attributes;
using VkToTg.Commands.Core;

namespace VkToTg.Commands
{
    [Command("conv", "View all conversations.")]
    public class ConversationsCommand : BaseCommand
    {
        public ConversationsCommand(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        public override async Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var accessMngr = scope.ServiceProvider.GetService<Services.Telegram.AccessManager>();
                if (!accessMngr.HasAccess(message.Chat.Id)) return;

                var vkConversations = scope.ServiceProvider.GetService<Services.Vk.ConversationReceiver>();

                var msg = string.Join("\n", vkConversations.GetAllConversations());

                await botClient.SendTextMessageAsync(message.Chat, msg);
            }
        }
    }
}