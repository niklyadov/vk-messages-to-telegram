using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VkToTg.Attributes;
using VkToTg.Commands.Core;

namespace VkToTg.Commands
{
    [Command("conv", "View all conversations.")]
    public class ConversationsCommand : BaseCommand
    {
        public ConversationsCommand(IServiceScopeFactory serviceScopeFactor, ITelegramBotClient botClient) : base(serviceScopeFactor, botClient)
        {
        }

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            using var scope = ServiceScopeFactory.CreateScope();

            var accessMngr = scope.ServiceProvider.GetService<Services.Telegram.AccessManager>();
            if (!accessMngr.HasAccess(message.Chat.Id)) return;

            await BotClient.SendChatActionAsync(message.Chat, ChatAction.Typing, cancellationToken);

            var currentPage = 1;

            var conversationsService = scope.ServiceProvider.GetService<Services.Vk.ConversationReceiver>();
            var text = GetMessageText(conversationsService, currentPage);

            if (text == null) return;

            var inline = GetInline(currentPage);

            // Send message!
            await BotClient.SendTextMessageAsync(message.Chat, text, null, null, null, null, null, null, inline);
        }

        public override async Task OnCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var message = callbackQuery.Message;
            using var scope = ServiceScopeFactory.CreateScope();

            var accessMngr = scope.ServiceProvider.GetService<Services.Telegram.AccessManager>();
            if (!accessMngr.HasAccess(message.Chat.Id)) return;


            var currentPage = 1;
            var suffix = GetCommandSuffix(callbackQuery.Data);
            int.TryParse(suffix, out currentPage);

            var conversationsService = scope.ServiceProvider.GetService<Services.Vk.ConversationReceiver>();
            var text = GetMessageText(conversationsService, currentPage);

            if(text == null) return;

            var inline = GetInline(currentPage);

            await BotClient.EditMessageTextAsync(message.Chat, message.MessageId, text, null, null, null, null, cancellationToken);
            await BotClient.EditMessageReplyMarkupAsync(message.Chat, message.MessageId, inline);
        }

        private string GetMessageText(Services.Vk.ConversationReceiver conversations, int page = 1)
        {
            string text = null;

            try
            {
                text = string.Join("\n", conversations.GetAllConversations(page));
            }
            catch { }

            return text;
        }

        private InlineKeyboardMarkup GetInline(int currentPage = 1)
        {
            var buttons = new List<InlineKeyboardButton>();

            if (currentPage > 1)
            {
                buttons.Add(new InlineKeyboardButton("Prev")
                {
                    CallbackData = $"/conv {currentPage - 1}"
                });                    
            }

            buttons.Add(new InlineKeyboardButton("Next")
            {
                CallbackData = $"/conv {currentPage + 1}"
            });

            // Keyboard markup
            return new InlineKeyboardMarkup(buttons);
        }


        private string GetCommandSuffix(string fullMessage)
        {
            return fullMessage.Replace($"/{ExtractCommandName(fullMessage)}", " ");
        }
    }
}