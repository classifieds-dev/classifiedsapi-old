using System.Collections.Generic;
using Shared.Enums;

namespace Shared.Models
{
    public class Profile
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public ProfileStatuses Status { get; set; }
        public ProfileTypes Type { get; set; }
        public ProfileSubtypes Subtype { get; set; }
        public AdTypes Adspace { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PreferredName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Introduction { get; set; }
        public ProfileImage Logo { get; set; }
        public ProfileImage Headshot { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<Location> Locations { get; set; }
        public ProfilePermissions EntityPermissions { get; set; }
    }
}
