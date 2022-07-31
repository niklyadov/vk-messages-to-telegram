using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VkToTg.Attributes;

namespace VkToTg.Commands.Conversations
{
    [Command("conv", "View all conversations.")]
    public class ConversationsCommand : BaseConversationsCommand
    {
        public ConversationsCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            var accessMngr = ServiceProvider.GetService<Services.Telegram.AccessManager>();
            if (!accessMngr.HasAccess(message.Chat.Id)) return;

            await TelegramBotClient.SendChatActionAsync(message.Chat, ChatAction.Typing, cancellationToken);

            var currentPage = 1;

            var conversationsService = ServiceProvider.GetService<Services.Vk.ConversationReceiver>();
            var text = GetMessageText(conversationsService, currentPage);

            if (string.IsNullOrEmpty(text)) return;

            var inline = GetInline(currentPage);

            // Send message!
            await TelegramBotClient.SendTextMessageAsync(message.Chat, text, null, null, null, null, null, null, null, inline);
        }

        public override async Task OnCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var message = callbackQuery.Message;

            var accessMngr = ServiceProvider.GetService<Services.Telegram.AccessManager>();
            if (!accessMngr.HasAccess(message.Chat.Id)) return;
            var suffix = GetCommandSuffix(callbackQuery.Data);

            int currentPage;
            int.TryParse(suffix, out currentPage);

            var conversationsService = ServiceProvider.GetService<Services.Vk.ConversationReceiver>();
            var text = GetMessageText(conversationsService, currentPage);

            if (text == null) return;

            var inline = GetInline(currentPage);

            await TelegramBotClient.EditMessageTextAsync(message.Chat, message.MessageId, text, null, null, null, null, cancellationToken);
            await TelegramBotClient.EditMessageReplyMarkupAsync(message.Chat, message.MessageId, inline);
        }
    }
}