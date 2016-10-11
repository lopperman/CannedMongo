using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace CS.MongoDB.Interfaces
{
    public interface IIndexFactory
    {
        IList<Tuple<IMongoIndexKeys, IMongoIndexOptions>> GetIndexes(Type type);
    }

}
