using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkToTg.Models;

namespace VkToTg.Services.Vk
{
    public class ConversationReceiver : ApiService
    {
        private int _countPerPage = 7;
        private readonly VkCacheService _cache;
        private readonly ILogger _logger;

        public ConversationReceiver(IOptions<Configuration> configurationOptions, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(configurationOptions, loggerFactory)
        {
            _cache = serviceProvider.GetRequiredService<VkCacheService>();
            _logger = loggerFactory.CreateLogger<ConversationReceiver>();
        }

        public ICollection<string> GetAllConversations(int page = 1)
        {
            var conversations = VkApi.Messages.GetConversations(new GetConversationsParams()
            {
                Count = (ulong?)_countPerPage,
                Offset = (ulong?)(_countPerPage * (page - 1)),
            }).Items;

            return conversations.Select(x => FormatConversation(x)).ToList();
        }

        public ICollection<string> GetUnreadedConversations(int page = 1)
        {
            var conversations = VkApi.Messages.GetConversations(new GetConversationsParams()
            {
                Count = (ulong?)_countPerPage,
                Offset = (ulong?)(_countPerPage * (page - 1)),
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

            if (convAndLM.Conversation.ChatSettings != null)
            {
                return convAndLM.Conversation.ChatSettings.Title;
            }

            if (_cache.CacheUserNames.ContainsKey(peerId))
            {
                return _cache.CacheUserNames[peerId];
            }

            try
            {
                var userName = VkApi.Users.Get(new List<long>() { peerId })
                    .Select(user => $"{user.FirstName} {user.LastName}")
                    .First();

                _cache.CacheUserNames.Add(peerId, userName);

                return userName;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return "UNKNOWN";
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