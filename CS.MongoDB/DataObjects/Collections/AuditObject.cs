using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace CS.MongoDB.DataObjects.Collections
{
    [Serializable]
    public class AuditObject: IEntity
    {
        private IDictionary<string, object> _NonMappedFields = new Dictionary<string, object>();
        public IDictionary<string, object> NonMappedFields
        {
            get { return _NonMappedFields; }
            set { _NonMappedFields = value; }
        }

        public int CompareTo(IEntity other)
        {
            return other._id.CompareTo(_id);
        }

        public string _id { get; set; }

        public string Source { get; set; }
        public string OriginalId { get; set; }
        public string Action { get; set; }
        public BsonDocument Item { get; set; }

        public DateTime AddDt { get; set; }
        public string AddBy { get; set; }
        public DateTime ModDt { get; set; }
        public string ModBy { get; set; }

    }
}
