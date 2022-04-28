using Microsoft.Extensions.DependencyInjection;
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
                        if(vkMsg.PhotoLink != null)
                        {
                            await BotClient.SendChatActionAsync(message.Chat, ChatAction.UploadPhoto, cancellationToken);
                            await BotClient.SendPhotoAsync(message.Chat, new InputOnlineFile(vkMsg.PhotoLink), vkMsg.Text);
                        }
                        else if (vkMsg.AudioMessageLink != null)
                        {
                            await BotClient.SendChatActionAsync(message.Chat, ChatAction.RecordVoice, cancellationToken);
                            await BotClient.SendAudioAsync(message.Chat, new InputOnlineFile(vkMsg.AudioMessageLink), vkMsg.Text);
                        }
                        //else if (vkMsg.VideoLink != null)
                        //{
                        //    await BotClient.SendChatActionAsync(message.Chat, ChatAction.RecordVideo, cancellationToken);
                        //    await BotClient.SendVideoAsync(message.Chat, new InputOnlineFile(vkMsg.VideoLink), null, null, null, vkMsg.Text);
                        //}
                        else if (vkMsg.DocumentLink != null)
                        {
                            await BotClient.SendChatActionAsync(message.Chat, ChatAction.UploadDocument, cancellationToken);
                            await BotClient.SendDocumentAsync(message.Chat, new InputOnlineFile(vkMsg.DocumentLink), null, vkMsg.Text);
                        }
                        else
                        {
                            await BotClient.SendChatActionAsync(message.Chat, ChatAction.Typing, cancellationToken);
                            await BotClient.SendTextMessageAsync(message.Chat, vkMsg.Text);
                        }
                    }

                } else
                {
                    await BotClient.SendTextMessageAsync(message.Chat, "No conversation selected");
                }
            }
        }
    }
}