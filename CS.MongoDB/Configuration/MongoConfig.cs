using System;
using System.Configuration;
using MongoDB.Driver;

namespace CS.MongoDB.Configuration
{
    public interface IMongoConfig
    {
        MongoServerSettings SettingsServer { get; }
        MongoUrl MongoUrl { get; }
        MongoServer Server { get; }
        MongoDatabase Database { get; }
        MongoCredential Credentials { get; }
    }

    public class MongoConfig : IComparable<MongoConfig>, IMongoConfig
    {

        private MongoUrl _mongoUrl;
        private MongoServerSettings _settingsServer = null;
        private MongoServer _server = null;
        private MongoCredential _credentials;

        public MongoUrl MongoUrl
        {
            get { return _mongoUrl; }
        }

        public MongoCredential Credentials
        {
            get { return _credentials; }
        }

        public MongoServer Server
        {
            get { return _server; }
        }

        public MongoDatabase Database
        {
            get { return _server.GetDatabase(_mongoUrl.DatabaseName); }
        }

        public MongoServerSettings SettingsServer
        {
            get { return _settingsServer; }
        }

        public MongoConfig(string appConfigConnectionStringName)
            : this(MongoUrl.Create(ConfigurationManager.ConnectionStrings[appConfigConnectionStringName].ConnectionString))
        {
        }

        public MongoConfig(MongoUrl mongoUrl)
        {
            _mongoUrl = mongoUrl;
            _settingsServer = MongoServerSettings.FromUrl(mongoUrl);
            _server = new MongoClient(mongoUrl).GetServer();

            _credentials = MongoCredential.CreateMongoCRCredential(mongoUrl.DatabaseName, mongoUrl.Username,
                                                                   mongoUrl.Password);
        }

        public int CompareTo(MongoConfig other)
        {
            return other.ToString().CompareTo(this.ToString());
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", SettingsServer.Server, _mongoUrl.DatabaseName);
        }

    }
}
