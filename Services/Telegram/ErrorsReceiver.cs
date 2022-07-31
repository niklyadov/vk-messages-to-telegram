using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace VkToTg.Services.Telegram
{
    public class ErrorsReceiver
    {
        private readonly ILogger<ErrorsReceiver> _logger;
        public ErrorsReceiver(ILogger<ErrorsReceiver> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _logger.LogError($"{JsonConvert.SerializeObject(exception)}");
            });
        }
    }
}
