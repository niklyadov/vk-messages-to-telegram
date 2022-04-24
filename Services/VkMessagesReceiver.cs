using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model;
using VkToTg.Models;

namespace VkToTg.Services
{
    public class VkMessagesReceiver : VkApiService
    {
        public long? SelectedConversationId { get; set; }
        public VkMessagesReceiver(IOptions<Configuration> configurationOptions)
            : base(configurationOptions)
        {
        }

        public string GetTitle(Message message)
        {
            if(!message.FromId.HasValue)
            {
                return "UNKNOWN";
            }

            var user = VkApi.Users.Get(new List<long>() { message.FromId.Value }).First();

            return $"{user.FirstName} {user.LastName} (/{message.FromId})";

        }
        
        public string GetText(Message message)
        {
            var text = message.Text;


            if (message.Attachments != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    text += $"{attachment.Type.Name}";
                }
            }

            if (message.ForwardedMessages != null)
            {
                foreach (var forwardedMessage in message.ForwardedMessages)
                {
                    text += $"\t{GetTitle(forwardedMessage)}: \n{GetText(forwardedMessage)}";
                }
            }

            return text;
        }

        public async IAsyncEnumerable<string> GetMessagesAsync(long conversationId)
        {
            var messagesHistory = await VkApi.Messages.GetHistoryAsync(
                new VkNet.Model.RequestParams.MessagesGetHistoryParams 
                { 
                    PeerId = conversationId,
                    Count = 25
                });

            foreach (var item in messagesHistory.Messages.Reverse())
            {
                yield return $"{GetTitle(item)}: \n{GetText(item)}";
            }
        }
    }
}
