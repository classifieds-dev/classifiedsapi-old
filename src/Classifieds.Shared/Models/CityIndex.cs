using System.Collections.Generic;

namespace Shared.Models
{
    public class CityIndex
    {
        public string SourceId { get; set; }
        public string City { get; set; }
        public string StateId { get; set; }
        public string StateName { get; set; }
        public string CountyName { get; set; }
        public int Population { get; set; }
        public string Timezone { get; set; }
        public List<double> Location { get; set; }
    }
}