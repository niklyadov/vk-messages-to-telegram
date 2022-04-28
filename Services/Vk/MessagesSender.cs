using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using VkNet.Model.RequestParams;
using VkToTg.Models;

namespace VkToTg.Services.Vk
{
    public class MessagesSender : ApiService
    {
        public MessagesSender(IOptions<Configuration> configurationOptions, ILoggerFactory loggerFactory) 
            : base(configurationOptions, loggerFactory)
        {
        }

        public void SendMessage(string message, long peerId)
        {
            VkApi.Messages.Send(new MessagesSendParams()
            {
                PeerId = peerId,
                Message = message,
                RandomId = new Random().Next(int.MaxValue)
            });
        }
    }
}