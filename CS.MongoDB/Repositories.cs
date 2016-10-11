using System;
using System.Collections.Generic;
using System.Linq;
using CG.MongoDB.Base;
using CS.MongoDB.Configuration;
using CS.MongoDB.Interfaces;

namespace CS.MongoDB
{
    public class Repositories : IRepositories
    {
        private List<Tuple<IMongoConfig, SortedList<string, object>>> _repos = new List<Tuple<IMongoConfig, SortedList<string, object>>>();

        public static Repositories Instance = new Repositories();


        public IRepositoryMongo<T> Repository<T>(IMongoConfig mongoConfig) where T : class,IEntity
        {
            if (_repos.Count() == 0)
            {
                //build default tuple
                _repos.Add(new Tuple<IMongoConfig, SortedList<string, object>>(mongoConfig, new SortedList<string, object>()));
            }

            var repo = _repos.Where(x => x.Item1.ToString() == mongoConfig.ToString()).SingleOrDefault();

            if (repo == null)
            {
                _repos.Add(new Tuple<IMongoConfig, SortedList<string, object>>(mongoConfig, new SortedList<string, object>()));
                repo = _repos.Where(x => x.Item1.ToString() == mongoConfig.ToString()).SingleOrDefault();
            }


            if (!repo.Item2.ContainsKey(MongoUtil.GetCollectioNameFromInterface<T>()))
            {
                var config = repo.Item1;
                repo.Item2.Add(MongoUtil.GetCollectioNameFromInterface<T>(), new RepositoryMongo<T>(config));
            }

            return (IRepositoryMongo<T>)repo.Item2[MongoUtil.GetCollectioNameFromInterface<T>()];
        }


    }
}