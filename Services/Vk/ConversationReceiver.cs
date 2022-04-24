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
        public ConversationReceiver(IOptions<Configuration> configurationOptions, ILoggerFactory loggerFactory)
            : base(configurationOptions, loggerFactory)
        {
        }

        public ICollection<string> GetAllConversations ()
        {
            var conversations = VkApi.Messages.GetConversations(new GetConversationsParams()).Items;

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
            if(convAndLM.Conversation.ChatSettings != null)
            {
                return convAndLM.Conversation.ChatSettings.Title;
            }

            if(convAndLM.LastMessage.FromId.HasValue)
            {
                return VkApi.Users.Get(new List<long>() { convAndLM.Conversation.Peer.Id })
                    .Select(x => $"{x.FirstName} {x.LastName}").First();
            }

            return "UNKNOWN";
        }

    }
}