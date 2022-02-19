using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Shared.Models;
using IdentityModel.Client;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Shared.Services
{
    public class AuthApi
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthApi> _logger;

        public AuthApi(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<AuthApi> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PublicUserProfile> GetPublicUserProfile(string userId)
        {
            // var apiGateway = "https://classifieds.apigateway";
            var apiGateway = "https://gzm61h96d7.execute-api.us-east-1.amazonaws.com/dev/users";
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            var client = new HttpClient(httpClientHandler);
            var response = await client.GetAsync($"{apiGateway}/publicuserprofile/{userId}");
            var profile = await response.Content.ReadAsAsync<PublicUserProfile>();
            return profile;
        }

        public async Task<string> GetUserId()
        {
            // var authority = _configuration.GetSection("IdentityServer")["Authority"];
            // var authority = "https://dev-585865.okta.com/oauth2/default/v1";
            var authority = "https://classifieds-ui-dev.auth.us-east-1.amazoncognito.com/oauth2";
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            var client = new HttpClient(httpClientHandler);
            //_logger.LogDebug("GetUserId before headers");
            //_logger.LogDebug("Headers: " + JsonConvert.SerializeObject(_httpContextAccessor));
            var response = await client.GetUserInfoAsync(new UserInfoRequest
            {
                // Address = $"{authority}/connect/userinfo",
                Address = $"{authority}/userInfo",
                Token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Substring(7)
            });
            /*if(response.IsError)
            {
                _logger.LogError("GetUserId Fail");
                _logger.LogError("{response}", response);
            }*/
            // Set the user who created the ad.
            var sub = response.Claims.Where(c => c.Type == "sub").First()?.Value;
            return sub;
        }
    }
}
