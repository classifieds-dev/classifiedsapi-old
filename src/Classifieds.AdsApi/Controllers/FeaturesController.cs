using System.Collections.Generic;
using System.Linq;
using AdsApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Enums;

namespace AdsApi.Controllers
{
    [ApiController]
    public class FeaturesController : ControllerBase
    {

        private readonly FeaturesRepository _featuresRepository;
        private readonly AdTypesRepository _adTypesRepository;

        public FeaturesController(FeaturesRepository featuresRepository, AdTypesRepository adTypesRepository)
        {
            _featuresRepository = featuresRepository;
            _adTypesRepository = adTypesRepository;
        }

        [HttpGet("/featurelistitems")]
        public List<AdFeature> Get(AdTypes adType, string searchString = null, string adSearchString = null, string location = null, [FromQuery(Name = "features")] List<string> features = null, [FromQuery] IDictionary<string, IDictionary<int, string>> attributes = null)
        {
            var loc = location != null && location.Trim() != "" ? location.Trim().Split(',').Select(double.Parse).ToList() : null;
            var search = searchString == null || searchString == "" ? null : searchString;
            var adSearch = adSearchString == null || adSearchString == "" ? null : adSearchString;
            var adTypeEntity = _adTypesRepository.Get(adType);
            return _featuresRepository.Get(adTypeEntity, search, adSearch, loc, features, attributes);
        }
    }
}
