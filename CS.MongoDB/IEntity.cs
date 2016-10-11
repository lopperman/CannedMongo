using System;
using System.Collections.Generic;
using CS.Common.Interfaces;

namespace CS.MongoDB
{
    /// <summary>
    /// Entity interface.
    /// </summary>
    public interface IEntity : IComparable<IEntity>, IRequireClassMap
    {
        /// <summary>
        /// Gets or sets the Id of the Entity.
        /// </summary>
        /// <value>Id of the Entity.</value>
        string _id { get; set; }
        IDictionary<string, object> NonMappedFields { get; set; }
    }
}
