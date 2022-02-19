using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Shared.Enums;
using Shared.Models;

namespace Shared.Helpers
{
    public class AdIndexerHelper
    {
        private readonly ILogger<AdIndexerHelper> _log;
        private readonly IElasticClient _esClient;
        public AdIndexerHelper(ILogger<AdIndexerHelper> log, IElasticClient esClient)
        {
            this._log = log;
            this._esClient = esClient;
        }
        public async Task IndexAd(Ad ad)
        {
            _log.LogDebug("Received: {@message}", ad);
            var indexAd = new AdIndex
            {
                Id = ad.Id.ToString(),
                AdType = ad.AdType,
                Status = ad.Status,
                Title = ad.Title,
                Description = ad.Description,
                Location = ad.Location,
                Images = new List<AdImageIndex>(),
                Attributes = new List<AttributeIndex>(),
                Features = new List<AdFeatureIndex>()
            };
            if (ad.Attributes != null)
            {
                foreach (var attribute in ad.Attributes)
                {
                    indexAd.Attributes.AddRange(getAttributes(attribute));
                }
            }
            if (ad.FeatureSets != null)
            {
                foreach (var vocab in ad.FeatureSets)
                {
                    foreach (var term in vocab.Terms)
                    {
                        indexAd.Features.AddRange(getFeatures(term));
                    }
                }
            }
            if (ad.Images != null)
            {
                foreach (var image in ad.Images)
                {
                    indexAd.Images.Add(new AdImageIndex
                    {
                        Id = image.Id,
                        Path = image.Path,
                        Weight = image.Weight
                    });
                }
            }
            await _esClient.IndexDocumentAsync(indexAd);
        }

        public List<AdFeatureIndex> getFeatures(Term term)
        {
            List<AdFeatureIndex> leafNodes = new List<AdFeatureIndex>();
            if (term.Children.Count == 0)
            {
                if (term.Selected)
                {
                    leafNodes.Add(new AdFeatureIndex { Id = term.Id.ToString(), HumanName = term.HumanName });
                }
            }
            else
            {
                foreach (var t in term.Children)
                {
                    leafNodes.AddRange(getFeatures(t));
                }
            }
            return leafNodes;
        }

        public List<AttributeIndex> getAttributes(AttributeValue attributeValue)
        {
            List<AttributeIndex> leafNodes = new List<AttributeIndex>();
            if (attributeValue.Attributes == null || attributeValue.Attributes.Count == 0)
            {
                if (attributeValue.Type == AttributeTypes.Number)
                {
                    var computedValue = Int32.Parse(attributeValue.ComputedValue);
                    leafNodes.Add(new AttributeIndex { Name = attributeValue.Name, DisplayName = attributeValue.DisplayName, Type = attributeValue.Type, intValue = computedValue });
                }
                else
                {
                    leafNodes.Add(new AttributeIndex { Name = attributeValue.Name, DisplayName = attributeValue.DisplayName, Type = attributeValue.Type, stringValue = attributeValue.ComputedValue });
                }
            }
            else
            {
                foreach (var a in attributeValue.Attributes)
                {
                    leafNodes.AddRange(getAttributes(a));
                }
            }
            return leafNodes;
        }
    }
}
