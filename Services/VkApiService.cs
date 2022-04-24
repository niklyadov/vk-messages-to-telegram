using Microsoft.Extensions.Options;
using VkNet;
using VkNet.Model;
using VkToTg.Models;

namespace VkToTg.Services
{
    public class VkApiService
    {
        public VkApi VkApi { get; set; }
        private VkAccountConfiguration _configuration;
        public VkApiService(IOptions<Configuration> configurationOptions)
        {
            _configuration = configurationOptions.Value.VkAccount;

            VkApi = new VkApi();
            VkApi.Authorize(new ApiAuthParams
            {
                Login = _configuration.Login,
                Password = _configuration.Password,
                AccessToken = _configuration.AccessToken
            });

            VkApi.RequestsPerSecond = 1;
        }
    }
}
