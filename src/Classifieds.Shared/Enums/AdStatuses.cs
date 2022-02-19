using System;

namespace Shared.Enums
{
    [Flags]
    public enum AdStatuses
    {
        Submitted,
        Approved,
        Rejected,
        Expired,
        Deleted
    }
}
