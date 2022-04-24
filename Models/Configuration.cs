namespace VkToTg.Models
{
    public class Configuration
    {
        public TelegramBotConfiguration TelegramBot { get; set; }
            = new TelegramBotConfiguration();
        public VkAccountConfiguration VkAccount { get; set; }
            = new VkAccountConfiguration();
    }

    public class TelegramBotConfiguration
    {
        public string BotToken { get; set; } = "";
        public long AllowedChatId { get; set; } = 0;
    }

    public class VkAccountConfiguration
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public int RequestsPerSecond { get; set; } = 1;
    }
}