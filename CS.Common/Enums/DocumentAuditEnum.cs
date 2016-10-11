using System;

namespace CS.Common.Enums
{
    [Flags]
    [Serializable]
    public enum DocumentAuditEnum
    {
        None,
        Edit,
        Delete
    }
}
