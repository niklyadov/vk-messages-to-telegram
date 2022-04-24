using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VkToTg.Attributes;
using VkToTg.Commands.Core;

namespace VkToTg.Commands
{
    [Command("msg", "View all messages in selected conversation.")]
    public class MessagesCommand : BaseCommand
    {
        public MessagesCommand(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        public override async Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var accessMngr = scope.ServiceProvider.GetService<Services.Telegram.AccessManager>();
                if (!accessMngr.HasAccess(message.Chat.Id)) return;

                var vkMessages = scope.ServiceProvider.GetService<Services.Vk.MessagesReceiver>();

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