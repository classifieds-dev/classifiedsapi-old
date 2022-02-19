using System;

namespace Shared.Enums
{
    [Flags]
    public enum ProfileStatuses
    {
        Submitted,
        Approved,
        Rejected,
        Deleted
    }
}