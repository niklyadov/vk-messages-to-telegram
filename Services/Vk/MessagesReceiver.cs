using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkToTg.Models;

namespace VkToTg.Services.Vk
{
    public class MessagesReceiver : ApiService
    {
        public long? SelectedConversationId { get; set; }
        public MessagesReceiver(IOptions<Configuration> configurationOptions, ILoggerFactory loggerFactory)
            : base(configurationOptions, loggerFactory)
        {
        }

        public async IAsyncEnumerable<MessageModel> GetMessagesAsync(long conversationId)
        {
            var messagesHistory = await VkApi.Messages.GetHistoryAsync(
                new VkNet.Model.RequestParams.MessagesGetHistoryParams 
                { 
                    PeerId = conversationId,
                    Count = 7
                });

            foreach (var message in messagesHistory.Messages.Reverse())
            {
                yield return GetMessageModel(message);
            }
        }

        public async IAsyncEnumerable<MessageModel> GetUnreadMessagesAsync(long conversationId)
        {
            var conversation = (await VkApi.Messages.GetConversationsByIdAsync(new List<long> 
            { 
                conversationId 
            })).Items.FirstOrDefault();

            if(conversation != null && conversation.InRead != conversation.LastMessageId)
            {
                var messagesHistory = await VkApi.Messages.GetHistoryAsync(
                    new VkNet.Model.RequestParams.MessagesGetHistoryParams
                    {
                        PeerId = conversationId,
                        StartMessageId = conversation.InRead,
                        Offset = -25,
                        Count = 25
                    });

                foreach (var message in messagesHistory.Messages.Reverse())
                {
                    yield return GetMessageModel(message);
                }
            }
        }

        private MessageModel GetMessageModel(Message message)
        {
            var messageModel = new MessageModel()
            {
                Text = $"{GetTitle(message)}\n {GetText(message)}"
            };

            if (message.Attachments != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    if (attachment.Type == typeof(Photo))
                    {
                        var photoAttachment = attachment.Instance as Photo;
 
                        if (photoAttachment.Sizes != null && photoAttachment.Sizes.Count > 0)
                        {
                            messageModel.PhotosLinks.Add(photoAttachment.Sizes.Last().Url);
                        }
                    }
                    else if (attachment.Type == typeof(Sticker))
                    {
                        var sticker = attachment.Instance as Sticker;

                        if (sticker.Images != null && sticker.Images.Count() > 0)
                        {
                            messageModel.PhotosLinks.Add(sticker.Images.First().Url);
                            messageModel.Text += "🏞 Sticker";
                        }
                    }
                    else if (attachment.Type == typeof(AudioMessage))
                    {
                        var audioMessage = attachment.Instance as AudioMessage;

                        messageModel.AudioMessageLink = audioMessage.LinkMp3;
                    }
                    else if (attachment.Type == typeof(Document))
                    {
                        var document = attachment.Instance as Document;

                        messageModel.DocumentsLinks.Add(new System.Uri(document.Uri));
                    }
                }
            }

            return messageModel;
        }

        private string GetTitle(Message message)
        {
            if (!message.FromId.HasValue)
            {
                return "UNKNOWN";
            }

            var user = VkApi.Users.Get(new List<long>() { message.FromId.Value }).First();

            var date = string.Empty;
            if(message.Date.HasValue)
            {
                var messageDate = message.Date.Value.ToLocalTime();

                date = $"{messageDate.ToShortTimeString()}";
                if(messageDate.DayOfYear != DateTime.Now.DayOfYear)
                {
                    date = $"{date} {messageDate.ToShortDateString()}";
                }
            }

            return $"{user.FirstName} {user.LastName} (/{message.FromId}) [{date}]";

        }

        private string GetText(Message message)
        {
            var text = message.Text;

            if (message.ForwardedMessages != null)
            {
                foreach (var forwardedMessage in message.ForwardedMessages)
                {
                    text += $"\n🔄{GetTitle(forwardedMessage)}: \n{GetText(forwardedMessage)}";
                }
            }

            if (message.ReplyMessage != null)
            {
                text += $"\n↩️{GetTitle(message.ReplyMessage)}: \n{GetText(message.ReplyMessage)}";
            }

            if (message.Attachments != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    if (attachment.Type == typeof(Wall))
                    {
                        var wall = attachment.Instance as Wall;
                        text += $"\n--- Wall\n{wall.Text}";

                    }
                    else if (attachment.Type == typeof(AudioMessage))
                    {
                        var audioMessage = attachment.Instance as AudioMessage;

                        text += $"\nAudio Message: \n{audioMessage.Transcript}";
                    }
                }
            }

            return text;
        }
    }
}
