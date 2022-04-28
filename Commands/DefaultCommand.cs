using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VkToTg.Attributes;
using VkToTg.Commands.Core;

namespace VkToTg.Commands
{
    [Command()]
    public class DefaultCommand : BaseCommand
    {
        public DefaultCommand (IServiceProvider serviceProvider) : base(serviceProvider)
        {
        } 

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            if (long.TryParse(message.Text.Substring(1), out long conversationId))
            {
                var accessMngr = ServiceProvider.GetService<Services.Telegram.AccessManager>();
                if (!accessMngr.HasAccess(message.Chat.Id)) return;

                var vkMessages = ServiceProvider.GetService<Services.Vk.MessagesReceiver>();
                vkMessages.SelectedConversationId = conversationId;

                await TelegramBotClient.SendTextMessageAsync(message.Chat, $"Selected chat: ({conversationId})");
                return;
            }

            await TelegramBotClient.SendTextMessageAsync(message.Chat, "Unknown command. Please, try again!");

        }
    }
}