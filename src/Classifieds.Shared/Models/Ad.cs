using System.Collections.Generic;
using Shared.Enums;

namespace Shared.Models
{
    public class Ad
    {
        public string Id { get; set; }
        public AdTypes AdType { get; set; }
        public AdStatuses Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<decimal> Location { get; set; }
        public string UserId { get; set; }
        public string ProfileId { get; set; }
        public string UserName { get; set; }
        public string CityDisplay { get; set; }
        public List<AdImage> Images { get; set; }
        public List<AttributeValue> Attributes { get; set; }
        public List<Vocabulary> FeatureSets { get; set; }
    }
}