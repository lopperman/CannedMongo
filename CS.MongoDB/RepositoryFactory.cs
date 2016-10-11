using CS.Common.Extensions;
using CS.Common.Interfaces;
using CS.MongoDB.Configuration;
using CS.MongoDB.Interfaces;

namespace CS.MongoDB
{

    public interface IRepositoryFactory
    {
        IRepositoryMongo<T> GetRepository<T>() where T : class, IEntity;
        IBuildPropNames<T> GetBuildPropNames<T>();
    }

    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IMongoConfig mongoConfig;
        private readonly IRepositories repositories;

        public RepositoryFactory(IMongoConfig config)
        {
            this.mongoConfig = config;
            this.repositories = Repositories.Instance;
        }

        public IBuildPropNames<T> GetBuildPropNames<T>()
        {
            return new BuildPropNames<T>();
        }

        public IRepositoryMongo<T> GetRepository<T>() where T : class, IEntity
        {
            return repositories.Repository<T>(mongoConfig);
        }

    }
}
