using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CS.BO;
using CS.MongoDB.Configuration;
using CS.MongoDB.CSDataManager;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace CS.UT.DatabaseServer
{
    [TestFixture]
    public class Connectivity_UT
    {

        [SetUp]
        public void startup()
        {
            Discriminator.Instance.Register();
            CreateCSDatabaseIfMissing();
        }

        [Test]
        public void CreateCSDatabaseIfMissing()
        {
            var databases = DataManager.Instance.Config.Server.GetDatabaseNames().ToList();
            if (!databases.Contains("CS"))
            {
                DataManager.Instance.Config.Server.GetDatabase("CS");
            }

        }

        [TearDown]
        public void teardown()
        {
        }

        [Test]
        public void CanConnectToMongoServerTestLocalDB()
        {
            Assert.IsNotNull(DataManager.Instance.Config);
            Assert.IsNotNull(DataManager.Instance.Config.Server);
            Assert.IsNotNull(DataManager.Instance.Config.Database);
        }

        [Test]
        public void CannAddCollection()
        {
            MongoCollection col = DataManager.Instance.Config.Database.GetCollection("Test1");
            Assert.IsNotNull(col);

            col.Insert(new BsonDocument(new BsonElement("Test", "Test")));

            col.Drop();
        }

        [Test]
        public void invalidCollectionReturnsNull()
        {
            DataManager mgr = DataManager.Instance;

            Assert.IsNull(mgr.GetCollection("abcdefg"));
        }

        [Test]
        public void CanGetValidCollection()
        {
            DataManager mgr = DataManager.Instance;

            MongoCollection col = mgr.GetCollection("UserObject");

            Assert.IsNotNull(col);
        }
    }
}
