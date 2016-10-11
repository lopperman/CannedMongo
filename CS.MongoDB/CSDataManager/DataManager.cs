using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CS.MongoDB.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CS.MongoDB.CSDataManager
{
    public class DataManager
    {
        private static IRepositoryFactory _repositoryFactory = null;
        private static IMongoConfig _config = null;
        private static MongoServer _server = null;
        private static MongoDatabase _database = null;
        private static SortedList<string, MongoCollection> _collections = null;
        private static DataManager _instance = null;

        public IMongoConfig Config
        {
            get { return _config; }
        }

        public static DataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Initialize();
                    
                }
                return _instance;
            }
        }

        private static void Initialize()
        {
            _config = new MongoConfig("MongoServer");
            _instance = new DataManager();
            
            _repositoryFactory = new RepositoryFactory(_config);
            _server = _config.Server;
            _database = _config.Database;
            _collections = new SortedList<string, MongoCollection>();
        }

        private DataManager()
        {
            
        }

        public string UserName
        {
            get { return _config.Credentials.Username.ToLower(); }
        }

        public void Add<T>(T entity) where T : class, IEntity
        {
            _repositoryFactory.GetRepository<T>().Add(entity);
        }

        public WriteConcernResult Add(string collectionName, BsonDocument entity)
        {
            return GetCollection(collectionName).Insert(entity);
        }

        public MongoCollection GetCollection(string collectionName)
        {
            if (_collections.All(x => x.Key != collectionName))
            {
                //verify this collection actually exists on database, otherwise return null
                var existingCollections = _database.GetCollectionNames().ToList();
                if (existingCollections.Contains(collectionName))
                {
                    _collections.Add(collectionName,_database.GetCollection(collectionName));
                }
            }

            var kvp =  _collections.SingleOrDefault(x => x.Key == collectionName);

            if (kvp.Value != null)
            {
                return kvp.Value;
            }
            return null;
        }
    }
}
