using System.Collections.Generic;
using AdsApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Enums;

namespace AdsApi.Controllers
{

    [ApiController]
    public class AdTypesController : ControllerBase
    {
        private readonly AdTypesRepository _adTypesRepository;

        public AdTypesController(AdTypesRepository adTypesRepository)
        {
            _adTypesRepository = adTypesRepository;
        }

        [HttpGet("/adtypes")]
        public List<AdType> Get()
        {
            return _adTypesRepository.Get();
        }

        [HttpGet("/adtype/{id:required:length(1)}", Name = "GetAdType")]
        public ActionResult<AdType> Get(AdTypes id)
        {
            var adType = _adTypesRepository.Get(id);

            if (adType == null)
            {
                return NotFound();
            }

            return adType;
        }

    }
}
