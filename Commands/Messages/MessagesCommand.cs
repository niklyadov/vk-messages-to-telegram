using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VkToTg.Attributes;
using VkToTg.Services.Telegram;
using VkToTg.Services.Vk;

namespace VkToTg.Commands.Messages
{
    [Command("msg", "View all messages in selected conversation.")]
    public class MessagesCommand : BaseMessagesCommand
    {
        public MessagesCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            var accessMngr = ServiceProvider.GetService<AccessManager>();
            if (!accessMngr.HasAccess(message.Chat.Id)) return;

            var vkMessages = ServiceProvider.GetService<MessagesReceiver>();
            if (vkMessages.SelectedConversationId.HasValue)
            {
                var messages = vkMessages.GetMessagesAsync(vkMessages.SelectedConversationId.Value);
                await PrintMesssagesAync(messages, cancellationToken);
            }
            else
            {
                await TelegramBotClient.SendTextMessageAsync(message.Chat, "No conversation selected");
            }
        }
    }
}