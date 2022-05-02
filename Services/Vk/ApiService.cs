using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VkNet;
using VkNet.Model;
using VkToTg.Models;

namespace VkToTg.Services.Vk
{
    public class ApiService
    {
        public VkApi VkApi { get; set; }
        private VkConfiguration _configuration;
        public ApiService(IOptions<Configuration> configurationOptions, ILoggerFactory loggerFactory)
        {
            _configuration = configurationOptions.Value.Vk;

            VkApi = new VkApi(loggerFactory.CreateLogger<VkApi>());

            VkApi.Authorize(new ApiAuthParams
            {
                Login = _configuration.Account.Login,
                Password = _configuration.Account.Password,
                AccessToken = _configuration.Account.AccessToken
            });

            VkApi.RequestsPerSecond = _configuration.RequestsPerSecond;
        }
    }
}
