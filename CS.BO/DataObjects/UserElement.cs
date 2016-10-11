using System;
using CS.Common.Enums;

namespace CS.BO.DataObjects
{
    [Serializable]
    public class UserElement
    {
        public CSDataTypeEnum DataType { get; set; }
        public UserElementConstraint ElementConstraint { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

    }
}
