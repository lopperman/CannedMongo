using System;
using CS.BO.Interfaces;

namespace CS.BO.DataObjects
{
    [Serializable]
    public class UserElementConstraint: IUserElementConstraint
    {
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public bool Required { get; set; }
    }
}
