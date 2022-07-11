using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace VkToTg.Services.Vk
{
    public class VkCacheService
    {
        public Dictionary<long, string> CacheUserNames { get; }
            = new Dictionary<long, string>();

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public VkCacheService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<VkCacheService>>();
        }
    }
}
