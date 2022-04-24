using System;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Newtonsoft.Json;

namespace VkToTg.Services.Telegram
{
    public class ErrorsReceiver
    {
        private ILogger<ErrorsReceiver> _logger;
        public ErrorsReceiver(ILogger<ErrorsReceiver> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError($"{JsonConvert.SerializeObject(exception)}");
        }
    }
}
