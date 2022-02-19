using System;
using System.Collections.Generic;
using Shared.Enums;

namespace Shared.Models
{
    public class AdTypeAttribute
    {
        public string Name { get; set; }
        public AttributeTypes Type { get; set; }
        public string Label { get; set; }
        public Boolean Required { get; set; }
        public string Widget { get; set; }
        public List<AdTypeAttribute> Attributes { get; set; }
    }
}
