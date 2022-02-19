using System.Collections.Generic;

namespace Shared.Models
{
    public class ProfilePermissions
    {
        public List<string> readUserIds { get; set; }
        public List<string> writeUserIds { get; set; }
        public List<string> deleteUserIds { get; set; }
    }
}