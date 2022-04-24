using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkToTg.Models;

namespace VkToTg.Services
{
    public class VkConversationReceiver : VkApiService
    {
        public VkConversationReceiver(IOptions<Configuration> configurationOptions)
            : base(configurationOptions)
        {
        }

        public ICollection<string> GetConversations ()
        {
            var conversations = VkApi.Messages.GetConversations(new GetConversationsParams()).Items;

            return conversations.Select(x =>
            {
                var title = GetConversationTitle(x);
                var peerId = x.Conversation.Peer.Id;
                var unreadSym = "?";

                return $"{title} {unreadSym} (/{peerId})";

            }).ToList();
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