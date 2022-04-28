using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Enums.SafetyEnums;
using VkToTg.Models;

namespace VkToTg.Services.Vk
{
    public class ConversationReceiver : ApiService
    {
        private int _countPerPage = 7;

        public ConversationReceiver(IOptions<Configuration> configurationOptions, ILoggerFactory loggerFactory)
            : base(configurationOptions, loggerFactory)
        {
        }

        Dictionary<long, string> CacheConversationsTitles = new Dictionary<long, string>();
        public ICollection<string> GetAllConversations(int page = 1)
        {
            var conversations = VkApi.Messages.GetConversations(new GetConversationsParams() 
            {
                Count = (ulong?)_countPerPage,
                Offset = (ulong?)(_countPerPage * (page-1)),
            }).Items;

            return conversations.Select(x => FormatConversation(x) ).ToList();
        }

        public ICollection<string> GetUnreadedConversations()
        {
            var conversations = VkApi.Messages.GetConversations(new GetConversationsParams()
            {
                Filter = GetConversationFilter.Unread
            }).Items;

            return conversations.Select(x => FormatConversation(x)).ToList();
        }

        private string FormatConversation(ConversationAndLastMessage convAndLM)
        {
            var title = GetConversationTitle(convAndLM);
            var peerId = convAndLM.Conversation.Peer.Id;
            var unreadCount = convAndLM.Conversation.UnreadCount ?? 0;

            return $"{title} {unreadCount} (/{peerId})";
        }

        private string GetConversationTitle(ConversationAndLastMessage convAndLM)
        {
            var peerId = convAndLM.Conversation.Peer.Id;

            if (CacheConversationsTitles.ContainsKey(peerId))
            {
                return CacheConversationsTitles[peerId];
            }

            string title = "UNKNOWN";


            if(convAndLM.Conversation.ChatSettings != null)
            {
                title = convAndLM.Conversation.ChatSettings.Title;
            } else if(convAndLM.LastMessage.FromId.HasValue)
            {
                title = VkApi.Users.Get(new List<long>() { convAndLM.Conversation.Peer.Id })
                    .Select(x => $"{x.FirstName} {x.LastName}").First();
            }


            CacheConversationsTitles.Add(peerId, title);

            return title;

        }

        public long GetUnreadedMessagesCount()
        {
            var conversations = VkApi.Messages.GetConversations(new GetConversationsParams()
            {
                Filter = GetConversationFilter.Unread
            }).Items;

            return (int)conversations.Sum(x => x.Conversation.UnreadCount);
        }
    }
}