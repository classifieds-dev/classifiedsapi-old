using System.Collections.Generic;
using Shared.Enums;

namespace Shared.Models
{
    public class AdType
    {
        public AdTypes Id { get; set; }
        public string Name { get; set; }
        public List<AdTypeAttribute> Attributes { get; set; }
        public List<AdTypeAttribute> Filters { get; set; }
    }
}
