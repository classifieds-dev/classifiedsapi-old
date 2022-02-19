using System.Collections.Generic;
namespace Shared.Models
{
    public class Vocabulary
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string MachineName { get; set; }
        public string HumanName { get; set; }
        public List<Term> Terms { get; set; }
    }
}
