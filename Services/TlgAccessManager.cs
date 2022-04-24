using Microsoft.Extensions.Options;
using VkToTg.Models;

namespace VkToTg.Services
{
    public class TlgAccessManager
    {
        public TelegramBotConfiguration _configuration;
        public TlgAccessManager(IOptions<Configuration> configurationOptions)
        {
            _configuration = configurationOptions.Value.TelegramBot;
        }

        public bool HasAccess(long chatId)
            => _configuration.AllowedChatId == chatId;
    }
}
