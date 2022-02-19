using System.Collections.Generic;
using Shared.Enums;

namespace Shared.Models
{
    public class Location
    {
        public string Title { get; set; }
        public LocationTypes Type { get; set; }
        public Address Address { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
    }
}
