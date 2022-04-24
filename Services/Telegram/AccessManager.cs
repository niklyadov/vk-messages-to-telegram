using Microsoft.Extensions.Options;
using VkToTg.Models;

namespace VkToTg.Services.Telegram
{
    public class AccessManager
    {
        public TelegramBotConfiguration _configuration;
        public AccessManager(IOptions<Configuration> configurationOptions)
        {
            _configuration = configurationOptions.Value.TelegramBot;
        }

        public bool HasAccess(long chatId)
            => _configuration.AllowedChatId == chatId;
    }
}
