using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using VkToTg.Commands.Core;

namespace VkToTg.Commands.Conversations
{
    public class BaseConversationsCommand : BaseCommand
    {
        public BaseConversationsCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected string GetMessageText(Services.Vk.ConversationReceiver conversations, int page = 1)
        {
            string text = null;

            try
            {
                text = string.Join("\n", conversations.GetAllConversations(page));
            }
            catch { }

            return text;
        }

        protected string GetMessageUnreadText(Services.Vk.ConversationReceiver conversations, int page = 1)
        {
            string text = null;

            try
            {
                text = string.Join("\n", conversations.GetUnreadedConversations(page));
            }
            catch { }

            return text;
        }

        protected InlineKeyboardMarkup GetInline(int currentPage = 1, string command = "conv")
        {
            var buttons = new List<InlineKeyboardButton>();

            if (currentPage > 1)
            {
                buttons.Add(new InlineKeyboardButton("Prev")
                {
                    CallbackData = $"/{command} {currentPage - 1}"
                });
            }

            buttons.Add(new InlineKeyboardButton("Next")
            {
                CallbackData = $"/{command} {currentPage + 1}"
            });

            // Keyboard markup
            return new InlineKeyboardMarkup(buttons);
        }


        protected string GetCommandSuffix(string fullMessage)
        {
            return fullMessage.Replace($"/{ExtractCommandName(fullMessage)}", " ");
        }
    }
}
