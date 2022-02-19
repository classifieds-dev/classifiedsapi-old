using System.Collections.Generic;
using Shared.Enums;


namespace Shared.Models
{
    public class AttributeValue
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public AttributeTypes Type { get; set; }
        public string Value { get; set; }
        public string ComputedValue { get; set; }
        public int Test { get; set; }
        public List<AttributeValue> Attributes { get; set; }

    }
}
