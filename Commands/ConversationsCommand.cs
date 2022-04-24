using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace VkToTg.Commands
{
    public class ConversationsCommand : BaseCommand
    {
        public ConversationsCommand(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        public override async Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var accessMngr = scope.ServiceProvider.GetService<Services.TlgAccessManager>();
                if (!accessMngr.HasAccess(message.Chat.Id)) return;

                var vkConversations = scope.ServiceProvider.GetService<Services.VkConversationReceiver>();

                var msg = string.Join("\n", vkConversations.GetConversations());

                await botClient.SendTextMessageAsync(message.Chat, msg);
            }
        }
    }
}
