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
        public MessagesCommand(IServiceScopeFactory serviceScopeFactory, ITelegramBotClient botClient) : base(serviceScopeFactory, botClient)
        {
        }

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
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
                        await BotClient.SendTextMessageAsync(message.Chat, vkMsg);
                    }

                    return;
                }

                await BotClient.SendTextMessageAsync(message.Chat, "No conversation selected");
            }
        }
    }
}