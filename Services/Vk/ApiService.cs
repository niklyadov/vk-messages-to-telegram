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
        private VkAccountConfiguration _configuration;
        public ApiService(IOptions<Configuration> configurationOptions, ILoggerFactory loggerFactory)
        {
            _configuration = configurationOptions.Value.VkAccount;

            VkApi = new VkApi(loggerFactory.CreateLogger<VkApi>());

            VkApi.Authorize(new ApiAuthParams
            {
                Login = _configuration.Login,
                Password = _configuration.Password,
                AccessToken = _configuration.AccessToken
            });

            VkApi.RequestsPerSecond = _configuration.RequestsPerSecond;
        }
    }
}
