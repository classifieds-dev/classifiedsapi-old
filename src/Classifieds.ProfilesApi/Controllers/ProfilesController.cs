using System.Threading.Tasks;
using ProfilesApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace ProfilesApi.Controllers
{
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly ProfilesRepository _profilesRepository;

        public ProfilesController(ProfilesRepository profilesRepository)
        {
            _profilesRepository = profilesRepository;
        }

        [HttpGet("profilelistitems")]
        public async Task<IEnumerable<Profile>> Get(string parentId = null)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;
            return await _profilesRepository.Get(token.Subject, parentId);
        }

        [HttpGet("profilenavitems")]
        public async Task<IEnumerable<ProfileNavItem>> GetNavItems()
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;
            return await _profilesRepository.GetNavItems(token.Subject);
        }

        [HttpPost("profile")]
        public async Task<ActionResult<Profile>> Create(Profile profile)
        {

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;

            profile.Id = Guid.NewGuid().ToString();
            profile.UserId = token.Subject;
            profile.Status = Shared.Enums.ProfileStatuses.Submitted;

            profile.EntityPermissions = new ProfilePermissions() {
                readUserIds = new List<string> { token.Subject },
                writeUserIds = new List<string> { token.Subject },
                deleteUserIds = new List<string> { token.Subject }
            };

            await _profilesRepository.Create(profile);
            return profile;
        }

    }

}
