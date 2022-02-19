using Shared.Enums;

namespace Shared.Models
{
    public class AttributeIndex
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public AttributeTypes Type { get; set; }
        public int intValue { get; set; }
        public string stringValue { get; set; }
    }
}
