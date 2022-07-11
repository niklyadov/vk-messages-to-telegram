using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model;
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

            if (conversation != null && conversation.InRead != conversation.LastMessageId)
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
            var messageModel = new MessageModel();

            messageModel.Title = GetMessageTitle(message);
            messageModel.Message = message.Text;

            messageModel.AppendAllAttachments(message.Attachments);

            if (message.ForwardedMessages != null)
            {
                foreach (var forwardedMessage in message.ForwardedMessages)
                {
                    messageModel.Message += $"\n🔄{GetMessageTitle(forwardedMessage)}: \n{forwardedMessage.Text}";

                    messageModel.AppendAllAttachments(forwardedMessage.Attachments);
                }
            }

            if (message.ReplyMessage != null)
            {
                var replyMessage = message.ReplyMessage;
                messageModel.Message += $"\n↩️{GetMessageTitle(message.ReplyMessage)}: \n{replyMessage.Text}";

                // attachments in reply message is neccessary or not?
                //messageModel.AppendAllAttachments(replyMessage.Attachments);
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
            if (_cache.CacheUserNames.ContainsKey(senderUserId))
            {
                senderUserName = _cache.CacheUserNames[senderUserId];
            }
            else
            {
                senderUserName = VkApi.Users.Get(new List<long>() { senderUserId })
                    .Select(user => $"{user.FirstName} {user.LastName}")
                    .First();

                _cache.CacheUserNames.Add(senderUserId, senderUserName);
            }

            var messageTitle = $"{senderUserName} (/{message.FromId})";

            if (message.Date.HasValue)
            {
                var messageDate = message.Date.Value.ToLocalTime();
                messageTitle = $"[{messageDate.ToShortDateTimeString()}] {messageTitle}";
            }

            return messageTitle;
        }
    }
}
