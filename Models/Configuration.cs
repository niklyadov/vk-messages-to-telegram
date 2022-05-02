namespace VkToTg.Models
{
    public class Configuration
    {
        public TelegramBotConfiguration TelegramBot { get; set; }
            = new TelegramBotConfiguration();
        public VkConfiguration Vk { get; set; }
            = new VkConfiguration();
    }

    public class TelegramBotConfiguration
    {
        public string Token { get; set; } = "";
        public long AllowedChatId { get; set; } = 0;
        public bool AllowSendMessages { get; set; } = true;
    }

    public class VkConfiguration
    {
        public VkAccountConfiguration Account { get; set; }
        public VkMessagesMonitoringConfiguration MessagesMonitoring { get; set; }
            = new VkMessagesMonitoringConfiguration();

        public int RequestsPerSecond { get; set; } = 1;
    }

    public class VkMessagesMonitoringConfiguration
    {
        public bool IsEnabled { get; set; } = true;
        public int IntervalInMinutes { get; set; } = 1;
    }

    public class VkAccountConfiguration
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string AccessToken { get; set; } = "";
    }
}