using System.Collections.Generic;
using Shared.Enums;

namespace Shared.Models
{
    public class AdIndex
    {
        public string Id { get; set; }
        public AdTypes AdType { get; set; }
        public AdStatuses Status { get; set; }
        public string Title { get; set; }
        public List<decimal> Location { get; set; }
        public string Description { get; set; }
        public List<AdImageIndex> Images { get; set; }
        public List<AttributeIndex> Attributes { get; set; }
        public List<AdFeatureIndex> Features { get; set; }
    }
}
