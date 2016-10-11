using System;
using System.Collections.Generic;
using CS.BO.Interfaces;
using CS.BO.Model;
using CS.MongoDB;

namespace CS.BO.DataObjects.Collections
{
    /// <summary>
    /// A UserObject is business object (e.g.  Customer) that defines a related set of 'fields'
    /// </summary>

    [Serializable]
    public class UserObject : IEntity, IObjectId, IIndex<UserObject>
    {
        private IDictionary<string, object> _NonMappedFields = new Dictionary<string, object>();
        public IDictionary<string, object> NonMappedFields
        {
            get { return _NonMappedFields; }
            set { _NonMappedFields = value; }
        }

        public UserObject()
        {
            Elements = new SortedDictionary<int, UserElement>();
        }

        public string _id { get; set; }

        public int CompareTo(IEntity other)
        {
            return other._id.CompareTo(_id);
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Is this UserObject stored in its own collection
        /// </summary>
        public bool IsCollection { get; set; }

        public SortedDictionary<int,UserElement> Elements { get; set; }

    }
}
