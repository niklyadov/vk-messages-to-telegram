using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VkToTg.Attributes;
using VkToTg.Commands.Core;

namespace VkToTg.Commands
{
    [Command("unreadconv", "View all conversations with unreaded messages.")]
    public class UnreadConversations : BaseCommand
    {
        public UnreadConversations(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            var accessManager = ServiceProvider.GetService<Services.Telegram.AccessManager>();
            if (!accessManager.HasAccess(message.Chat.Id)) return;


            await TelegramBotClient.SendChatActionAsync(message.Chat, ChatAction.Typing, cancellationToken);

            var vkConversations = ServiceProvider.GetService<Services.Vk.ConversationReceiver>();
            var msg = string.Join("\n", vkConversations.GetUnreadedConversations());

            await TelegramBotClient.SendTextMessageAsync(message.Chat, msg);
        }
    }
}