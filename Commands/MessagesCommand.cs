using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace VkToTg.Commands
{
    public class MessagesCommand : BaseCommand
    {
        public MessagesCommand(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        public override async Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var accessMngr = scope.ServiceProvider.GetService<Services.TlgAccessManager>();
                if (!accessMngr.HasAccess(message.Chat.Id)) return;

                var vkMessages = scope.ServiceProvider.GetService<Services.VkMessagesReceiver>();

                if (vkMessages.SelectedConversationId.HasValue)
                {
                    await foreach (var vkMsg in vkMessages.GetMessagesAsync(vkMessages.SelectedConversationId.Value))
                    {
                        await botClient.SendTextMessageAsync(message.Chat, vkMsg);
                    }

                    return;
                }

                await botClient.SendTextMessageAsync(message.Chat, "No conversation selected");
            }
        }
    }
}
