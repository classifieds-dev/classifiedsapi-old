using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdsApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Enums;
using Microsoft.Extensions.Logging;
using System;
using Shared.Helpers;
using System.IdentityModel.Tokens.Jwt;

namespace AdsApi.Controllers
{
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly AdsRepository _adsRepository;
        private readonly AdTypesRepository _adTypesRepository;
        private readonly ILogger<AdsController> _logger;
        private readonly AdIndexerHelper _adIndexerHelper;

        public AdsController(AdsRepository adsRepository, AdTypesRepository adTypesRepository, ILogger<AdsController> logger, AdIndexerHelper adIndexerHelper)
        {
            _adsRepository = adsRepository;
            _adTypesRepository = adTypesRepository;
            _logger = logger;
            _adIndexerHelper = adIndexerHelper;
        }

        [HttpGet("adlistitems")]
        public List<Ad> Get(AdTypes adType, int page = 1, string searchString = null, string location = null, [FromQuery(Name = "features")] List<string> features = null, [FromQuery] IDictionary<string, IDictionary<int, string>> attributes = null)
        {
            var loc = location != null && location.Trim() !=  "" ? location.Trim().Split(',').Select(double.Parse).ToList() : null;
            var search = searchString == null || searchString == "" ? null : searchString;
            var adTypeEntity = _adTypesRepository.Get(adType);
            return _adsRepository.Get(adTypeEntity, page, search, loc, features, attributes );
        }

        /*[HttpGet("ad/{id:guid:required}", Name = "GetAd")]
        public async Task<ActionResult<Ad>> Get(string id)
        {
            var ad = await _adsRepository.Get(id);

            if (ad == null)
            {
                return NotFound();
            }

            return ad;
        }*/

        [HttpPost("ad")]
        public async Task<ActionResult<Ad>> Create(Ad ad)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;

            ad.Id = Guid.NewGuid().ToString();
            ad.UserId = token.Subject;
            ad.Status = AdStatuses.Submitted;

            await _adsRepository.Create(ad);
            await _adIndexerHelper.IndexAd(ad);

            return CreatedAtRoute("GetAd", new { id = ad.Id.ToString() }, ad);
        }

        /*[HttpPut("ad/{id:guid:required}")]
        public async Task<ActionResult<Ad>> Update(Ad ad)
        {
            var currentAd = await _adsRepository.Get(ad.Id);

            if (currentAd == null)
            {
                return NotFound();
            }
            else if (currentAd.UserId != ad.UserId)
            {
                return Unauthorized();
            }

            ad.UserId = currentAd.UserId;
            ad.AdType = currentAd.AdType;
            ad.Status = currentAd.Status;

            _adsRepository.Update(ad);

            await _adIndexerHelper.IndexAd(ad);

            return CreatedAtRoute("GetAd", new { id = ad.Id }, ad);
        }*/

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            _logger.LogDebug("Log this");
            return "This is a test";
        }

    }
}
