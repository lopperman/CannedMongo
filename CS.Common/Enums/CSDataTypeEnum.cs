using System;

namespace CS.Common.Enums
{
    [Serializable]
    public enum CSDataTypeEnum
    {
        String,
        Integer,
        Numeric,
        Boolean,
        Date,
        DateTime,
        Binary, 
        UserObject, 
        ListOfUserObjects
    }
}
