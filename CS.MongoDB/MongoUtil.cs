using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using CS.MongoDB;
using CS.MongoDB.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.IO;
using System.Runtime.Serialization;

namespace CG.MongoDB.Base
{
    public static class MongoUtil
    {
        public const string DefaultConnectionstringName = "MongoServerSettings";

        public static void DropIndexes(IMongoConfig config, Assembly assemblyWithIEntityInterfaces)
        {
            Type[] interfaceTypes = assemblyWithIEntityInterfaces.GetTypes().Where(x => x.IsClass == true).ToArray();
            foreach (var t in interfaceTypes)
            {
                if (typeof(IEntity).IsAssignableFrom(t))
                {
                    if (t.GetInterfaces().Where(x => x.Name.StartsWith("IIndex")).Any())
                    {
                        MongoDatabase db = new MongoClient(config.MongoUrl).GetServer().GetDatabase(config.Database.Name);
                        db.GetCollection(t.Name).DropAllIndexes();
                    }
                }
            }

        }

        public static MongoServer Server(MongoCollection coll)
        {
            return coll.Database.Server;
        }

        /// <summary>
        /// Get the BsonDateTime representation (UTC) of a Date/Time value.
        /// </summary>
        /// <remarks>
        ///This method will return the "correct" DateTime value taking into account the offset
        ///of the DateTime value passed in, including all offsets.  It might run a bit slower 
        ///than the one that works with System.DateTime, so only use when needed for now.  
        ///NOTE: The values returned by this method are valid to the milisecond level.
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BsonDateTime GetISODateTime(DateTimeOffset value)
        {
            return new BsonDateTime(value.UtcDateTime);
        }

        /// <summary>
        /// Get the BsonDateTime representation (UTC) of a Date/Time value.
        /// </summary>
        /// <remarks>
        ///This method will return the "correct" DateTime value taking into account the offset
        ///of the DateTime value passed in.  An issue is that it seems to only handle Local and UTC.
        ///NOTE: The values returned by this method are valid to the milisecond level.
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BsonDateTime GetISODateTime(DateTime value)
        {
            return new BsonDateTime(value);
        }

        /// <summary>
        /// Get the BsonDateTime representation (UTC) of the date of a Date/Time value.
        /// </summary>
        /// <remarks>
        /// The TimeZone and Offset values of the BsonDateTime returned from this method shoul be
        /// ignored completely.  They will always be in UTC, but are not intended to vary based 
        /// on the TimeZone of the parameter value.
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BsonDateTime GetISODate(DateTime value)
        {
            return GetISODateTime(new DateTime(value.Date.Ticks, DateTimeKind.Utc));
        }

#warning Why use GetIsoDateNoTime at all?  See TODO for details.
        //Why is this being used at all?  If a field is setup in the discriminator to be DateOnly, 
        //and you pass in a date with a time component, it will throw an exception.  Just use DateTime.Date.
        //Further testing has shown that the same fields always come back the same regardless of how Kind
        //is set in the discriminator.  Just the Kind on the result is different.


#warning When querying Mongo with a DateTime and DateOnly = true, time zone does not matter.

#warning When querying Mongo with a DateTime and DateOnly = false, time zone matters.
        [Obsolete("Use GetISODate instead")]
        public static BsonDateTime GetISODateNoTime(DateTime dt)
        {
            return GetISODate(dt.Date);
        }

        public static DateTime GetUtcDate(BsonDateTime value)
        {
            return value.ToUniversalTime();
        }

        public static DateTime GetDate(BsonDateTime value)
        {
            DateTime Converted = GetUtcDate(value);

            return new DateTime(Converted.Year, Converted.Month, Converted.Day);
        }

        public static DateTimeOffset GetDate(BsonDateTime value, TimeSpan offset)
        {
            DateTime Converted = GetUtcDate(value);

            return new DateTimeOffset(Converted.Year, Converted.Month, Converted.Day, 0, 0, 0, 0, offset);
        }

        /// Retrieves the default connectionstring from the App.config or Web.config file.
        /// </summary>
        /// <returns>Returns the default connectionstring from the App.config or Web.config file.</returns>
        public static string GetDefaultConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[DefaultConnectionstringName].ConnectionString;
        }

        public static IMongoQuery FormatIdElementForMongoQuery(IMongoQuery query)
        {
            return new QueryDocument(FormatIdElementForBsonDocument(query.ToBsonDocument()));
        }

        public static BsonDocument FormatIdElementForBsonDocument(BsonDocument doc)
        {
            foreach (BsonElement element in doc.Elements)
            {
                if (element.Name == "_id")
                {
                    if (element.Value.BsonType == BsonType.ObjectId)
                    {
                        continue;
                    }
                    else
                    {
                        var objId = ObjectId.Empty;
                        ObjectId.TryParse(element.Value.ToString(), out objId);
                        if (objId != ObjectId.Empty)
                        {
                            element.Value = objId;
                        }
                    }
                }
                else if (element.Value.BsonType == BsonType.Document)
                {
                    element.Value = FormatIdElementForBsonDocument(element.Value.ToBsonDocument());
                }
                else if (element.Value.BsonType == BsonType.Array)
                {
                    for (int i = element.Value.AsBsonArray.ToArray().GetLowerBound(0);
                         i <= element.Value.AsBsonArray.ToArray().GetUpperBound(0);
                         i++)
                    {
                        var ele = element.Value.AsBsonArray[i];
                        if (ele.BsonType == BsonType.Document)
                        {
                            element.Value.AsBsonArray[i] = FormatIdElementForBsonDocument((BsonDocument)ele);
                        }
                    }
                }
            }


            return doc;
        }

        /// <summary>
        /// Creates and returns a MongoDatabase from the specified connectionstring.
        /// </summary>
        /// <param name="connectionstring">The connectionstring to use to get the database from.</param>
        /// <returns>Returns a MongoDatabase from the specified connectionstring.</returns>
        public static MongoDatabase GetDatabaseFromConnectionString(string connectionstring)
        {
            var cnn = new MongoUrl(connectionstring);
            MongoServer server = new MongoClient(cnn).GetServer();
            return server.GetDatabase(cnn.DatabaseName);
        }

        /// <summary>
        /// Determines the collectionname from the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get the collectionname from.</typeparam>
        /// <returns>Returns the collectionname from the specified type.</returns>
        public static string GetCollectioNameFromInterface<T>() where T : IEntity
        {
            return typeof(T).Name;
        }

        /// <summary>
        /// Determines the collectionname from the specified type.
        /// </summary>
        /// <param name="entitytype">The type of the entity to get the collectionname from.</param>
        /// <returns>Returns the collectionname from the specified type.</returns>
        //private static string GetCollectionNameFromType(Type entitytype)
        //{
        //    string collectionname;

        //    // Check to see if the object (inherited from Entity) has a CollectionName attribute
        //    var att = Attribute.GetCustomAttribute(entitytype, typeof(CollectionName));
        //    if (att != null)
        //    {
        //        // It does! Return the value specified by the CollectionName attribute
        //        collectionname = ((CollectionName)att).Name;
        //    }
        //    else
        //    {
        //        // No attribute found, get the basetype
        //        while (!entitytype.BaseType.Equals(typeof(Entity)))
        //        {
        //            entitytype = entitytype.BaseType;
        //        }

        //        collectionname = entitytype.Name;
        //    }

        //    return collectionname;
        //}

        public static IEnumerable<Type> GetAllMongoTypes(Assembly assemblyWithMongoClasses)
        {
            return assemblyWithMongoClasses.GetTypes().Where(x => x.IsClass && (typeof(IEntity).IsAssignableFrom(x) || x.IsAssignableFrom(typeof(IEntity))));
        }

        //public static Type GetMongoEntityTypeByName(string name, Assembly assemblyWithMongoClasses)
        //{
        //    Type t = null;

        //    foreach (var type in GetAllMongoTypes(assemblyWithMongoClasses))
        //    {
        //        Attribute[] attrs = Attribute.GetCustomAttributes((MemberInfo)type);
        //        foreach (Attribute att in attrs)
        //        {
        //            if (att is CollectionName)
        //            {
        //                CollectionName colName = att as CollectionName;
        //                if (colName.Name == name)
        //                {
        //                    t = type;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    return t;
        //}

        public static void SerializeCursor<T>(IQueryable<T> cursor, IFormatter formatter, Stream stream)
        {
            foreach (T t in cursor)
            {
                //Cannot serialize "null"  
                //It would be nice to do this some way so that end of list would be known in advance.
                //This will be useful when serializing an IQueriable across the wire.
                if (t != null)
                {
                    formatter.Serialize(stream, t);
                }
            }
        }

        public static IQueryable<T> ParseCursor<T>(IFormatter formatter, Stream stream)
        {
            return ParseCursorAsEnumerable<T>(formatter, stream, -1).AsQueryable();
        }

        public static IQueryable<T> ParseCursor<T>(IFormatter formatter, Stream stream, int count)
        {
            return ParseCursorAsEnumerable<T>(formatter, stream, count).AsQueryable();
        }

        public static IEnumerable<T> ParseCursorAsEnumerable<T>(IFormatter formatter, Stream stream, int count)
        {
            int RetCount = 0;

            while ((count < 0 && stream.Position < stream.Length) || RetCount < count)
            {
                T t = (T)formatter.Deserialize(stream);

                //At this point, t is guarenteed to never be null.  See comment above.
                //if (t == null)
                //{
                //    break;
                //}

                RetCount++;
                yield return t;
            }

            stream.Close();
        }

        public static dynamic ConvertToObjectId(string value)
        {
            ObjectId ret = ObjectId.Empty;

            if (ObjectId.TryParse(value, out ret))
            {
                return ret;
            }

            throw new InvalidOperationException(string.Format("Cannot convert '{0}' to an ObjectId", value));
        }
    }

    public class SimpleDiscrimnator<T> : IDiscriminatorConvention
    {
        public string ElementName
        {
            get
            {
                return null;
            }
        }

        public Type GetActualType(BsonReader bsonReader, Type nominalType)
        {
            return typeof(T);
        }

        public BsonValue GetDiscriminator(Type nominalType, Type actualType)
        {
            return null;
        }
    }
}
