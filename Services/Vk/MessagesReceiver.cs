using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        public MessageModel GetMessageModel(Message message)
        {
            var messageModel = new MessageModel()
            {
                Text = $"{GetTitle(message)}\n {GetText(message)}"
            };

            if(message.Attachments != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    if (attachment.Type == typeof(Photo))
                    {
                        var photoAttachment = attachment.Instance as Photo;

                        if (photoAttachment.Sizes != null && photoAttachment.Sizes.Count > 0)
                        {
                            messageModel.PhotoLink = photoAttachment.Sizes.Last().Url;
                        }
                    }
                    //else if (attachment.Type == typeof(Sticker))
                    //{
                    //    var sticker = attachment.Instance as Sticker;

                    //    messageModel.PhotoLink = sticker.;
                    //}
                    else if (attachment.Type == typeof(AudioMessage))
                    {
                        var audioMessage = attachment.Instance as AudioMessage;

                        messageModel.AudioMessageLink = audioMessage.LinkMp3;
                    }
                    //else if (attachment.Type == typeof(Video))
                    //{
                    //    var video = attachment.Instance as Video;

                    //    messageModel.VideoLink = video.UploadUrl;
                    //}
                    else if (attachment.Type == typeof(Document))
                    {
                        var document = attachment.Instance as Document;

                        messageModel.DocumentLink = new System.Uri(document.Uri);
                    }
                }
            }

            return messageModel;
        }

        public string GetTitle(Message message)
        {
            if (!message.FromId.HasValue)
            {
                return "UNKNOWN";
            }

            var user = VkApi.Users.Get(new List<long>() { message.FromId.Value }).First();

            return $"{user.FirstName} {user.LastName} (/{message.FromId})";

        }

        public string GetText(Message message)
        {
            var text = message.Text;

            if (message.ForwardedMessages != null)
            {
                foreach (var forwardedMessage in message.ForwardedMessages)
                {
                    text += $"\n🔄 {GetTitle(forwardedMessage)}: \n{GetText(forwardedMessage)}";
                }
            }

            if (message.ReplyMessage != null)
            {
                text += $"\n↩️ {GetTitle(message.ReplyMessage)}: \n{GetText(message.ReplyMessage)}";
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

        public async IAsyncEnumerable<MessageModel> GetMessagesAsync(long conversationId)
        {
            var messagesHistory = await VkApi.Messages.GetHistoryAsync(
                new VkNet.Model.RequestParams.MessagesGetHistoryParams 
                { 
                    PeerId = conversationId,
                    Count = 25
                });

            foreach (var message in messagesHistory.Messages.Reverse())
            {
                yield return GetMessageModel(message);
            }
        }
    }
}
