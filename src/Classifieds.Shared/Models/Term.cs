using System.Collections.Generic;

namespace Shared.Models
{
    public class Term
    {
        public string Id { get; set; }
        public string VocabularyId { get; set; }
        public string ParentId { get; set; }
        public string MachineName { get; set; }
        public string HumanName { get; set; }
        public int Weight { get; set; }
        public bool Group { get; set; }
        public bool Selected { get; set; }
        public int Level { get; set; }
        public List<Term> Children { get; set; }
    }
}
