using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace UsersApi.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAmazonCognitoIdentityProvider cognitoClient, ILogger<UsersController> logger)
        {
            _cognitoClient = cognitoClient;
            _logger = logger;
        }

        [HttpGet("publicuserprofile/{userId:required}", Name = "GetPublicUserProfile")]
        public async Task<ActionResult<PublicUserProfile>> Get(string userId)
        {
            var response = await _cognitoClient.ListUsersAsync(new ListUsersRequest {
                Filter = $"sub=\"{userId}\"",
                Limit = 1,
                UserPoolId = "us-east-1_z8PhK3D8V"
                // AttributesToGet = new[] { "username", "sub" }
            });
            if(response.HttpStatusCode.ToString() != "200")
            {
                _logger.LogDebug("GetPublicUserProfile Error");
            }
            var user = response.Users.FirstOrDefault();
            if(user == null)
            {
                return NotFound();
            }
            return new PublicUserProfile() { Id = userId, UserName = user.Username };
        }

        /*[Route("authorizeuser")]
        public async Task<ActionResult<string>> AuthorizeUser()
        {
            var policy = "{\"principalId\":\"{principalId}\",\"policyDocument\":{\"Version\":\"2012-10-17\",\"Statement\":[{\"Action\":\"execute-api:Invoke\",\"Effect\":\"Allow\",\"Resource\":\"*\"}]}}";
            try
            {
                var userId = await _authApi.GetUserId();
                if (userId != null)
                {
                    policy = policy.Replace("{principalId}", $"{userId}");
                    return policy;
                }
                else
                {
                    return Unauthorized();
                }
            } catch(Exception)
            {
                return Unauthorized();
            }
        }*/

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            //_logger.LogDebug("Log this");
            return "This is a test";
        }
    }
}
