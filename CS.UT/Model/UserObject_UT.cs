using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CS.BO;
using CS.BO.DataObjects;
using CS.BO.DataObjects.Collections;
using CS.BO.Model;
using CS.Common.Enums;
using CS.MongoDB.Configuration;
using CS.MongoDB.CSDataManager;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace CS.UT.Model
{
    [TestFixture]
    public class UserObject_UT
    {
        private MongoConfig tmpConfig = null;

        [SetUp]
        public void startup()
        {
            tmpConfig = new MongoConfig("MongoServerTest");
            Discriminator.Instance.Register();
        }

        [Test]
        public void AddUserObject()
        {
            UserElement firstName = new UserElement();
            firstName.DataType = CSDataTypeEnum.String;
            firstName.Name = "FirstName";
            firstName.ElementConstraint = new UserElementConstraint(){MaxLength = 100,MinLength = 0,Required = true};
            UserElement lastName = new UserElement();
            lastName.DataType = CSDataTypeEnum.String;
            lastName.Name = "LastName";
            lastName.ElementConstraint = new UserElementConstraint() { MaxLength = 100, MinLength = 0, Required = true };
            UserObject person = new UserObject();
            person.Description = "Person";
            person.IsActive = true;
            person.Name = "Person";
            person.Elements.Add(0,firstName);
            person.Elements.Add(1,lastName);

            DataManager.Instance.Add(person);

//            MongoCollection col = tmpConfig.Database.GetCollection("UserObject");
//            col.Insert(person.ToBsonDocument());

        }

    }
}
