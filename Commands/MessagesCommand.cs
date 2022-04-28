using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using VkToTg.Attributes;
using VkToTg.Commands.Core;

namespace VkToTg.Commands
{
    [Command("msg", "View all messages in selected conversation.")]
    public class MessagesCommand : BaseCommand
    {
        public MessagesCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            var accessMngr = ServiceProvider.GetService<Services.Telegram.AccessManager>();
            if (!accessMngr.HasAccess(message.Chat.Id)) return;

            var vkMessages = ServiceProvider.GetService<Services.Vk.MessagesReceiver>();

            if (vkMessages.SelectedConversationId.HasValue)
            {
                await foreach (var vkMsg in vkMessages.GetMessagesAsync(vkMessages.SelectedConversationId.Value))
                {
                    if (vkMsg.PhotoLink != null)
                    {
                        await TelegramBotClient.SendChatActionAsync(message.Chat, ChatAction.UploadPhoto, cancellationToken);
                        await TelegramBotClient.SendPhotoAsync(message.Chat, new InputOnlineFile(vkMsg.PhotoLink), vkMsg.Text);
                    }
                    else if (vkMsg.AudioMessageLink != null)
                    {
                        await TelegramBotClient.SendChatActionAsync(message.Chat, ChatAction.RecordVoice, cancellationToken);
                        await TelegramBotClient.SendAudioAsync(message.Chat, new InputOnlineFile(vkMsg.AudioMessageLink), vkMsg.Text);
                    }
                    //else if (vkMsg.VideoLink != null)
                    //{
                    //    await BotClient.SendChatActionAsync(message.Chat, ChatAction.RecordVideo, cancellationToken);
                    //    await BotClient.SendVideoAsync(message.Chat, new InputOnlineFile(vkMsg.VideoLink), null, null, null, vkMsg.Text);
                    //}
                    else if (vkMsg.DocumentLink != null)
                    {
                        await TelegramBotClient.SendChatActionAsync(message.Chat, ChatAction.UploadDocument, cancellationToken);
                        await TelegramBotClient.SendDocumentAsync(message.Chat, new InputOnlineFile(vkMsg.DocumentLink), null, vkMsg.Text);
                    }
                    else
                    {
                        await TelegramBotClient.SendChatActionAsync(message.Chat, ChatAction.Typing, cancellationToken);
                        await TelegramBotClient.SendTextMessageAsync(message.Chat, vkMsg.Text);
                    }
                }

            }
            else
            {
                await TelegramBotClient.SendTextMessageAsync(message.Chat, "No conversation selected");
            }
        }
    }
}