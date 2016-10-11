using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CS.Common.Interfaces
{
    public interface IBuildPropNames<T>
    {
        IBuildPropNames<T> Add<PN>(Expression<Func<T, PN>> expression);
        string AddSingle<PN>(Expression<Func<T, PN>> expression);
        IEnumerable<string> PropertyNames { get; }
        string[] PropertyNamesArray { get; }
        void Clear();
    }
}
