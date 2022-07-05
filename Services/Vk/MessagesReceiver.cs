using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkToTg.Extensions;
using VkToTg.Models;

namespace VkToTg.Services.Vk
{
    public class MessagesReceiver : ApiService
    {
        private readonly VkCacheService _cache;
        public long? SelectedConversationId { get; set; }
        public MessagesReceiver(IOptions<Configuration> configurationOptions, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(configurationOptions, loggerFactory)
        {
            _cache = serviceProvider.GetRequiredService<VkCacheService>();
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
                Text = $"{GetMessageTitle(message)}\n {GetMessageText(message)}"
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
                            // select biggest photo
                            var photosPixelsUrl = photoAttachment.Sizes.Select(ps => (ps.Width * ps.Height, ps.Url)).ToList();
                                photosPixelsUrl.Sort((x, y) => y.Item1.CompareTo(x.Item1));

                            messageModel.PhotosLinks.Add(photosPixelsUrl.First().Url);
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

                        messageModel.DocumentsLinks.Add(new Uri(document.Uri));
                    }
                }
            }

            return messageModel;
        }

        private string GetMessageTitle(Message message)
        {
            if (!message.FromId.HasValue)
            {
                return "UNKNOWN";
            }

            var senderUserId = message.FromId.Value;
            var senderUserName = string.Empty;
            if(_cache.CacheUserNames.ContainsKey(senderUserId))
            {
                senderUserName = _cache.CacheUserNames[senderUserId];
            } else
            {
                senderUserName = VkApi.Users.Get(new List<long>() { senderUserId })
                    .Select(user => $"{user.FirstName} {user.LastName}")
                    .First();

                _cache.CacheUserNames.Add(senderUserId, senderUserName);
            }

            var messageTitle = $"{senderUserName} (/{message.FromId})";

            if(message.Date.HasValue)
            {
                messageTitle = $"[{message.Date.Value.ToShortDateTimeString()}] {messageTitle}";
            }

            return messageTitle;
        }

        private string GetMessageText(Message message)
        {
            var text = message.Text;

            if (message.ForwardedMessages != null)
            {
                foreach (var forwardedMessage in message.ForwardedMessages)
                {
                    text += $"\n🔄{GetMessageTitle(forwardedMessage)}: \n{GetMessageText(forwardedMessage)}";
                }
            }

            if (message.ReplyMessage != null)
            {
                text += $"\n↩️{GetMessageTitle(message.ReplyMessage)}: \n{GetMessageText(message.ReplyMessage)}";
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