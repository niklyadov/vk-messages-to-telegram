﻿using Microsoft.Extensions.DependencyInjection;
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
        public DefaultCommand (IServiceScopeFactory serviceScopeFactory, ITelegramBotClient botClient) : base(serviceScopeFactory, botClient)
        {
        } 

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            if (long.TryParse(message.Text.Substring(1), out long conversationId))
            {
                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var accessMngr = scope.ServiceProvider.GetService<Services.Telegram.AccessManager>();
                    if (!accessMngr.HasAccess(message.Chat.Id)) return;

                    var vkMessages = scope.ServiceProvider.GetService<Services.Vk.MessagesReceiver>();
                    vkMessages.SelectedConversationId = conversationId;


                    await BotClient.SendTextMessageAsync(message.Chat, $"Selected chat {conversationId}");
                    return;
                }

            }

            await BotClient.SendTextMessageAsync(message.Chat, "Unknown command. Please, try again!");

        }
    }
}