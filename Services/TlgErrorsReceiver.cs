using System;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Newtonsoft.Json;

namespace VkToTg.Services
{
    public class TlgErrorsReceiver
    {
        private ILogger<TlgErrorsReceiver> _logger;
        public TlgErrorsReceiver(ILogger<TlgErrorsReceiver> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError($"{JsonConvert.SerializeObject(exception)}");
        }
    }
}
