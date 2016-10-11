using CS.MongoDB.Configuration;

namespace CS.MongoDB.Interfaces
{
    public interface IRepositories
    {
        IRepositoryMongo<T> Repository<T>(IMongoConfig mongoConfig) where T : class,IEntity;
    }
}
