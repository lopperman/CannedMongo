using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CG.MongoDB.Base;
using CS.Common.Attributes;
using CS.Common.Enums;
using CS.Common.Extensions;
using CS.Common.Interfaces;
using CS.MongoDB.Configuration;
using CS.MongoDB.DataObjects.Collections;
using CS.MongoDB.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace CS.MongoDB
{
    public delegate void BeforeSaveHandler<T>(T record) where T : class,IEntity;
    public delegate void AfterSaveHandler<T>(T record) where T : class,IEntity;
    public delegate void BeforeDeleteHandler<T>(T record) where T : class,IEntity;
    public delegate void AfterDeleteHandler<T>(T record) where T : class,IEntity;

    //TODO:  deal with "_id" in MongoQuery objects
    //TODO:  when adding objects to audit table, make sure to save entity._id as an objectid, if it is one.

    public class RepositoryMongo<T> : IRepositoryMongo<T> where T : class,IEntity
    {

        public event BeforeDeleteHandler<T> BeforeDelete;
        public event BeforeSaveHandler<T> BeforeSave;
        public event AfterDeleteHandler<T> AfterDelete;
        public event AfterSaveHandler<T> AfterSave;

        private IMongoConfig _mongoConfig;

        public IBuildPropNames<T> GetBuildPropNames()
        {
            return new BuildPropNames<T>();
        }

        public dynamic FormatID<X>(string id)
        {
            if (typeof(IObjectId).IsAssignableFrom(typeof(X)))
            {
                return ObjectId.Parse(id);
            }
            return id;
        }

        public void OnBeforeSave(T record)
        {
            if (BeforeSave != null) BeforeSave(record);
        }

        public void OnAfterSave(T record)
        {
            if (AfterSave != null) AfterSave(record);
        }

        public void OnBeforeDelete(T record)
        {
            if (BeforeDelete != null) BeforeDelete(record);
        }

        public void OnAfterDelete(T record)
        {
            if (AfterDelete != null) AfterDelete(record);
        }

        private MongoCollection<T> _collection;

        public MongoCollection TEMPCOL
        {
            get { return _collection; }
        }

        public RepositoryMongo(string connnectionString)
            : this(new MongoConfig(new MongoUrl(connnectionString)))
        {
        }

        public RepositoryMongo(MongoUrl mongoUrl)
            : this(new MongoConfig(mongoUrl))
        {
        }

        public CommandResult RunCommand(IMongoCommand command)
        {
            return _collection.Database.RunCommand(command);
        }

        public RepositoryMongo(IMongoConfig mongoConfig)
        {
            //MongoServer server = MongoServer.Create(mongoConfig.SettingsServer);
            var cnn = mongoConfig.MongoUrl;
            MongoServer server = new MongoClient(cnn).GetServer();
            MongoDatabase db = server.GetDatabase(mongoConfig.Database.Name);

            _mongoConfig = mongoConfig;
            _collection = db.GetCollection<T>(MongoUtil.GetCollectioNameFromInterface<T>());

        }

        /// <summary>
        /// ** DEVELOPER USE ONLY **
        /// ** DO NOT ALLOW DIRECT MONGOCOLLECTION CALLS IN PRODUCTION SOFTWARE **
        /// Bypass RepositoryMongo and go straight to MongoCollection
        /// </summary>

        private MongoCollection<T> Collection
        {
            get { return _collection; }
        }

        #region RepositoryWrappers

        /// <summary>
        /// Add items to mongo collects for type T
        /// </summary>
        /// <param name="entities">Items to add to mongo collection</param>
        public IEnumerable<WriteConcernResult> Add(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException("entities","Null argument exception");

//            if (typeof(IAuditFields).IsAssignableFrom(typeof(T)))
//            {
//                Parallel.ForEach(entities, body: AuditAddEdit);
//            }

            foreach (T entity in entities)
            {
                OnBeforeSave(entity);
            }

            var ret = Collection.InsertBatch<T>(entities);

            foreach (T entity in entities)
            {
                OnAfterSave(entity);
            }

            return ret;

        }

        /// <summary>
        /// Add single item to mongo collection
        /// </summary>
        /// <param name="entity">Item to add to mongo collection</param>
        public WriteConcernResult Add(T entity)
        {
//            if (typeof(IAuditFields).IsAssignableFrom(typeof(T)))
//            {
//                AuditAddEdit(entity);
//            }

            WriteConcernResult ret = null;

            OnBeforeSave(entity);

            ret = Collection.Insert(entity);

            if (ret.Ok)
            {
                OnAfterSave(entity);
            }

            return ret;
        }

        public MongoCursor<T> FindAll()
        {
            return Collection.FindAll();
        }

        public IQueryable FindAllAsGenericQueryable()
        {
            return Collection.FindAll().AsQueryable();

        }

        public MongoCursor<T> FindAllWildcardSearch(string elementName, string searchText)
        {
            return Find(Query.Matches(elementName, string.Format("/{0}/i", searchText)));
        }


        //Finds one matching document and applies updates
        /// <summary>
        /// Find and Update FIRST matching document
        /// Note:  Before/After Save does not get called with this method.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public FindAndModifyResult FindAndUpdate(IMongoQuery query, IMongoSortBy sort, IMongoUpdate update)
        {
            //Implement necessary logic here (e.g.  auditing)
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.FindAndModify(fixedQuery, sort, update);

        }

        /// <summary>
        /// Note:  Before/After Save does not get called with this method.
        /// Find and Update FIRST matching document
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="update"></param>
        /// <param name="returnNew"></param>
        /// <returns></returns>
        public FindAndModifyResult FindAndUpdate(IMongoQuery query, IMongoSortBy sort, IMongoUpdate update, bool returnNew)
        {
            //Implement necessary logic here (e.g.  auditing)
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.FindAndModify(fixedQuery, sort, update, returnNew);
        }

        /// <summary>
        /// Note:  Before/After Save does not get called with this method.
        /// Find and Update FIRST matching document
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="update"></param>
        /// <param name="returnNew"></param>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public FindAndModifyResult FindAndUpdate(IMongoQuery query, IMongoSortBy sort, IMongoUpdate update, bool returnNew, bool upsert)
        {
            //Implement necessary logic here (e.g.  auditing)
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.FindAndModify(fixedQuery, sort, update, returnNew, upsert);

        }

        /// <summary>
        /// Note:  Before/After Save does not get called with this method.
        /// Find and Update FIRST matching document
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="update"></param>
        /// <param name="fields"></param>
        /// <param name="returnNew"></param>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public FindAndModifyResult FindAndUpdate(IMongoQuery query, IMongoSortBy sort, IMongoUpdate update, IMongoFields fields, bool returnNew, bool upsert)
        {
            //Implement necessary logic here (e.g.  auditing)

            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);

            return Collection.FindAndModify(fixedQuery, sort, update, fields, returnNew, upsert);

        }

        /// <summary>
        ////Find and remove the FIRST item found.
        /// Note:  Before and After Delete do not get called with this method.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public FindAndModifyResult FindAndRemove(IMongoQuery query, IMongoSortBy sort)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);

            return Collection.FindAndRemove(fixedQuery, sort);
        }

        public IEnumerable<string> Distinct(string key)
        {
            IEnumerable<BsonValue> ret = Collection.Distinct(key);

            return ret.Where(x => x != BsonNull.Value).Select(x => x.ToString());

        }

        public IEnumerable<string> Distinct(string key, IMongoQuery query)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);

            var results = Collection.Distinct(key, fixedQuery);

            return results.Where(x => x != BsonNull.Value)
                            .Select(x => x.ToString());
        }

        public IEnumerable<string> Distinct(Expression<Func<T, string>> expr)
        {
            return Collection.AsQueryable<T>().Select(expr).Distinct();
        }

        public T FindOne(IMongoQuery query)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.FindOne(fixedQuery);
        }

        public T FindOneById(string id)
        {
            return Collection.FindOneById(FormatID<T>(id));
        }

        public T FindOneById(string id, params string[] propertyNamesToReturn)
        {
            IMongoQuery query = Query.EQ(Constants.IDField, id);

            var result = Find(query, propertyNamesToReturn);

            if (result.Count() == 1) return result.First();

            return default(T);
        }



        public IEnumerable<BsonDocument> Group(IMongoQuery query, BsonJavaScript bsonJavaScript, BsonDocument initial, BsonJavaScript reduce, BsonJavaScript finalize)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);

            return Collection.Group(fixedQuery, bsonJavaScript, initial, reduce, finalize);
        }

        public IEnumerable<BsonDocument> Group(IMongoQuery query, IMongoGroupBy keys, BsonDocument initial, BsonJavaScript reduce, BsonJavaScript finalize)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);

            return Collection.Group(fixedQuery, keys, initial, reduce, finalize);
        }

        public IEnumerable<BsonDocument> Group(IMongoQuery query, string key, BsonDocument initial, BsonJavaScript reduce, BsonJavaScript finalize)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Group(fixedQuery, key, initial, reduce, finalize);
        }

        public MapReduceResult MapReduce(BsonJavaScript map, BsonJavaScript reduce)
        {
            return Collection.MapReduce(map, reduce);
        }

        public MapReduceResult MapReduce(IMongoQuery query, BsonJavaScript map, BsonJavaScript reduce)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.MapReduce(fixedQuery, map, reduce);
        }

        public MapReduceResult MapReduce(BsonJavaScript map, BsonJavaScript reduce, IMongoMapReduceOptions options)
        {
            return Collection.MapReduce(map, reduce, options);
        }

        public MapReduceResult MapReduce(IMongoQuery query, BsonJavaScript map, BsonJavaScript reduce, IMongoMapReduceOptions options)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.MapReduce(fixedQuery, map, reduce, options);
        }

        public CommandResult ReIndex()
        {
            return Collection.ReIndex();
        }

        /// <summary>
        /// Note:  Before/After delete do not get called with this method
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public WriteConcernResult Remove(IMongoQuery query)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Remove(fixedQuery);
        }

        /// <summary>
        /// Note:  Before/After delete do not get called with this method
        /// </summary>
        /// <param name="query"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public WriteConcernResult Remove(IMongoQuery query, RemoveFlags flags)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Remove(fixedQuery, flags);
        }

        /// <summary>
        /// Note:  Before/After delete do not get called with this method
        /// </summary>
        /// <param name="query"></param>
        /// <param name="flags"></param>
        /// <param name="writeConcern"></param>
        /// <returns></returns>
        public WriteConcernResult Remove(IMongoQuery query, RemoveFlags flags, WriteConcern writeConcern)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Remove(fixedQuery, flags, writeConcern);
        }

        /// <summary>
        /// Note:  Before/After delete do not get called with this method
        /// </summary>
        /// <param name="query"></param>
        /// <param name="writeConcern"></param>
        /// <returns></returns>
        public WriteConcernResult Remove(IMongoQuery query, WriteConcern writeConcern)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Remove(fixedQuery, writeConcern);
        }

        /// <summary>
        /// Note:  Before/After delete do not get called with this method
        /// </summary>
        /// <returns></returns>
        public WriteConcernResult RemoveAll()
        {
            return Collection.RemoveAll();
        }

        /// <summary>
        /// Note:  Before/After delete do not get called with this method
        /// </summary>
        /// <param name="writeConcern"></param>
        /// <returns></returns>
        public WriteConcernResult RemoveAll(WriteConcern writeConcern)
        {
            return Collection.RemoveAll(writeConcern);
        }

        /// <summary>
        /// If existing, replace item with T, otherwise add T to collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public WriteConcernResult Save(T item)
        {
            AuditAddEdit(item);

            return Collection.Save(item);
        }

        public WriteConcernResult Save(T item, MongoInsertOptions options)
        {
            OnBeforeSave(item);

            AuditAddEdit(item);

            var ret = Collection.Save(item, options);

            OnAfterSave(item);

            return ret;
        }

        public WriteConcernResult Save(T item, WriteConcern writeConcern)
        {
            OnBeforeSave(item);

            var ret = Collection.Save(item, writeConcern);

            OnAfterSave(item);

            return ret;
        }


        public MongoCollectionSettings Settings
        {
            get { return Collection.Settings; }
        }


        public MongoCursor<T> Find(IMongoQuery query)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Find(fixedQuery);
        }

        public MongoCursor<T> Find(IMongoQuery query, params string[] propertiesToReturn)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            var cursor = Collection.Find(fixedQuery);
            cursor.SetFields(propertiesToReturn);

            return cursor;
        }

        public MongoCursor FindAsBsonDocument(IMongoQuery query)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.FindAs<BsonDocument>(fixedQuery);
        }

        public MongoCursor<Y> FindAs<Y>(IMongoQuery query)
        {
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.FindAs<Y>(fixedQuery);
        }

        public MongoCursor<Y> FindAllAs<Y>()
        {
            return Collection.FindAllAs<Y>();
        }

        public MongoCursor FindAllAs(Type type)
        {
            return Collection.FindAllAs(type);
        }


        public IQueryable<T> All()
        {
            return Collection.AsQueryable<T>();
        }

        public IQueryable<T> All(Expression<Func<T, bool>> criteria)
        {
            return Collection.AsQueryable<T>().Where(criteria);
        }

        //TODO:  additional testing needed.
        /// <summary>
        /// WARNING:  this has been tested (pdb) to work when all documents contain element you are renaming.  Further testing is needed before this is production ready.
        /// Rename BsonElements within a collection.  (e.g.  If you change the name of 'LineId' to 'AnimalLineId' in a class)
        /// </summary>
        /// <param name="oldName">Case Sensitive BsonElement name you with to change</param>
        /// <param name="newName">New BsonElement name that will replace [oldName]</param>
        /// <param name="caseSensitivePKColumns">Case Sensitive list of all columns with comprise the Primary Key</param>
        public void RenameElement(string oldName, string newName, params string[] caseSensitivePKColumns)
        {
            //TODO:  figure out how to determine if the 'Id' field is an ObjectId type, and then search by Id instead of PK cols
            foreach (T doc in FindAll())
            {
                List<IMongoQuery> list = new List<IMongoQuery>();
                BsonDocument bsonDoc = doc.ToBsonDocument();
                foreach (string pkCol in caseSensitivePKColumns)
                {
                    list.Add(Query.EQ(pkCol, bsonDoc.GetValue(pkCol)));
                }
                IMongoQuery query = Query.And(list.ToArray());
                IMongoUpdate upd = global::MongoDB.Driver.Builders.Update.Rename(oldName, newName);
                var result = FindAndUpdate(query, SortBy.Null, upd);
                if (result.ErrorMessage != null)
                {
                    throw new Exception(string.Format("Error renaming {0} to {1} for {2}", oldName, newName, query.ToString()));
                }
            }
        }


        public long Count()
        {
            return Collection.Count();
        }

        public void Delete(T entity)
        {
            Delete(entity, false);
        }

        public void Delete(string id)
        {
            Delete(id, false);
        }

        public void Delete(string id, bool saveCopyOfDeletedItem)
        {
            T entityToDelete = FindOneById(id);

            if (entityToDelete != null)
            {
                Delete(entityToDelete, saveCopyOfDeletedItem);
            }
        }

        public void Delete(T entity, bool saveCopyOfDeletedItem)
        {
            OnBeforeDelete(entity);

            //don't archive anything from the DeletedObjects collection.
            if (saveCopyOfDeletedItem && MongoUtil.GetCollectioNameFromInterface<T>() != "AuditObjects")
            {
                //                var audit =
                //                    typeof (T).GetCustomAttributes(typeof (DocumentAuditAttr), false).SingleOrDefault();

                //                if (audit != null && ((audit as DocumentAuditAttr).AuditEnum & DocumentAuditEnum.Delete )== DocumentAuditEnum.Delete)
                {
                    var deletedObjRepo = Repositories.Instance.Repository<AuditObject>(_mongoConfig);
                    var deletedObject = new AuditObject();
                    deletedObject.OriginalId = entity._id;
                    deletedObject.Action = "DEL";
                    deletedObject.Source = MongoUtil.GetCollectioNameFromInterface<T>();
                    deletedObject.Item = entity.ToBsonDocument(typeof(T));
                    deletedObjRepo.Add(deletedObject);
                }
            }

            //WriteConcernResult result;
            RemoveItemById(entity);

            OnAfterDelete(entity);
        }

        private void RemoveItemById(T entity)
        {
            WriteConcernResult result;
            ObjectId ObjId = ObjectId.Empty;
            if (ObjectId.TryParse(entity._id, out ObjId))
            {
                result = Collection.Remove(Query.EQ(Constants.IDField, ObjId));
            }
            else
            {
                result = Collection.Remove(Query.EQ(Constants.IDField, entity._id));
            }
        }


        public void Delete(Expression<Func<T, bool>> criteria, bool savedCopyOfDeletedItems)
        {

            if (savedCopyOfDeletedItems && MongoUtil.GetCollectioNameFromInterface<T>() != "AuditObjects")
            {
                var allToSaveInDeletedItems = All(criteria).ToList();
                foreach (T item in allToSaveInDeletedItems)
                {
                    Delete(item, true);
                }
            }
            else
            {
                //Do mass delete without saving to deletedobjects collection
                var toDelete = All(criteria);

                foreach (T entity in toDelete)
                {
                    Delete(entity);
                }
            }
        }

        public void Delete(Expression<Func<T, bool>> criteria)
        {
            var query = ConvertExpressionListToGGQuery(new List<Expression<Func<T, bool>>>() { criteria }, CombinationOperatorsEnum.And);

            var cursor = Find(query, new string[] { "_id" });

            foreach (T item in cursor)
            {
                RemoveItemById(item);
            }
        }

        /// <summary>
        /// Warning:  This permanently deleted the collection!!!
        /// </summary>
        public void DeleteAll()
        {
            Collection.RemoveAll();
        }

        public new bool Equals(object obj)
        {
            //TODO:  need to test this
            return Collection.Equals(obj);
        }

        public bool Exists(Expression<Func<T, bool>> criteria)
        {
            return Collection.AsQueryable<T>().Any(criteria);
        }

        //Refactor to be able to return a different type, validate if we really need this
        [Obsolete("Paul is trying to fix this", true)]
        public T GetById(string id)
        {
            return Collection.FindOneByIdAs<T>(FormatID<T>(id));
        }

        public T GetSingle(Expression<Func<T, bool>> criteria)
        {
            return Collection.AsQueryable<T>().Where(criteria).FirstOrDefault();
        }

        public void RequestDone()
        {
            Collection.Database.RequestDone();
        }

        public IDisposable RequestStart()
        {
            return Collection.Database.RequestStart();
        }

        public IDisposable RequestStart(ReadPreference readPreference)
        {
            return Collection.Database.RequestStart(readPreference);
        }

        public WriteConcernResult Update(IMongoQuery query, UpdateBuilder update, UpdateFlags updateFlags)
        {
            //TODO:  paul, deal with auditing
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Update(query, update, updateFlags);
        }

        public WriteConcernResult Update(IMongoQuery query, UpdateBuilder update)
        {
            //TODO:  paul, deal with auditing
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Update(query, update);
        }

        public WriteConcernResult Update(IMongoQuery query, UpdateBuilder update, MongoUpdateOptions updateOptions)
        {
            //TODO:  paul, deal with auditing
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Update(query, update, updateOptions);
        }

        public WriteConcernResult Update(IMongoQuery query, UpdateBuilder update, WriteConcern writeConcern)
        {
            //TODO:  paul, deal with auditing
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Update(query, update, writeConcern);
        }

        public WriteConcernResult Update(IMongoQuery query, UpdateBuilder update, UpdateFlags updateFlags, WriteConcern writeConcern)
        {
            //TODO:  paul, deal with auditing
            IMongoQuery fixedQuery = MongoUtil.FormatIdElementForMongoQuery(query);
            return Collection.Update(query, update, updateFlags, writeConcern);
        }

        /// <summary>
        /// If T is new, inserts into collection, otherwise REPLACE item in collection with T
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Update(T entity)
        {
            OnBeforeSave(entity);

            var audit =
typeof(T).GetCustomAttributes(typeof(DocumentAuditAttr), false).SingleOrDefault();

            if (audit != null && entity._id != null && ((audit as DocumentAuditAttr).AuditEnum & DocumentAuditEnum.Edit) == DocumentAuditEnum.Edit)
            {

                var editedObjRep = Repositories.Instance.Repository<AuditObject>(_mongoConfig);
                var auditObject = new AuditObject();
                auditObject.OriginalId = entity._id;
                auditObject.Action = "UPD";
                auditObject.Source = MongoUtil.GetCollectioNameFromInterface<T>();

                T originalItem = FindOneById(entity._id);

                if (originalItem != null)
                {
                    auditObject.Item = originalItem.ToBsonDocument(typeof(T));
                    editedObjRep.Add(auditObject);
                }


            }

//            if (typeof(IAuditFields).IsAssignableFrom(typeof(T)))
//            {
//                AuditAddEdit(entity);
//            }

            Collection.Save<T>(entity);

            OnAfterSave(entity);

            return entity;
        }

        public void Update(IEnumerable<T> entities)
        {
            var audit = typeof(T).GetCustomAttributes(typeof(DocumentAuditAttr), false).SingleOrDefault();

            var mongoDocumentAuditAttr = audit as DocumentAuditAttr;
            if (mongoDocumentAuditAttr != null && ((mongoDocumentAuditAttr.AuditEnum & DocumentAuditEnum.Edit) == DocumentAuditEnum.Edit))
            {
                List<AuditObject> auditObjects = new List<AuditObject>();
                var editedObjRep = Repositories.Instance.Repository<AuditObject>(_mongoConfig);
                foreach (T item in entities)
                {
                    if (item._id == null) continue;
                    var auditObject = new AuditObject();
                    auditObject.OriginalId = item._id;
                    auditObject.Action = "UPD";
                    auditObject.Source = typeof(T).UnderlyingSystemType.Name;
                    auditObject.Item = item.ToBsonDocument(typeof(T));
                    auditObjects.Add(auditObject);
                }
                editedObjRep.Add(auditObjects);
            }

//            if (typeof(IAuditFields).IsAssignableFrom(typeof(T)))
//            {
//                if (entities != null) Parallel.ForEach(entities, AuditAddEdit);
//            }

            foreach (T entity in entities)
            {
                OnBeforeSave(entity);
            }


            foreach (T entity in entities)
            {
                Collection.Save<T>(entity);
            }

            foreach (T entity in entities)
            {
                OnAfterSave(entity);
            }

        }

        #endregion

        #region RepositoryManagerWrappers


        public void Drop()
        {
            Collection.Drop();
        }

        public bool Exists()
        {
            return Collection.Exists();
        }

        public CollectionStatsResult GetStats()
        {
            return Collection.GetStats();
        }

        public ValidateCollectionResult Validate()
        {
            return Collection.Validate();
        }

        public long GetTotalDataSize()
        {
            return Collection.GetTotalDataSize();
        }

        public long GetTotalStorageSize()
        {
            return Collection.GetTotalStorageSize();
        }

        public bool IsCapped()
        {
            return Collection.IsCapped();
        }

        public GetIndexesResult GetIndexes()
        {
            return Collection.GetIndexes();
        }

        public void Reindex()
        {
            Collection.ReIndex();
        }

        public void EnsureIndexes(IMongoIndexKeys indexKeys, IMongoIndexOptions indexOptions)
        {
            Collection.EnsureIndex(indexKeys, indexOptions);
        }

        [Obsolete("obsolete", true)]
        public void EnsureIndexes(IIndexFactory factory)
        {
            //            //only run if T implements IIndexes<T>
            //            if (typeof(IIndex<T>).IsAssignableFrom(typeof(T)))
            //            {
            //                foreach (var indexTuple in factory.GetIndexes<T>())
            //                {
            //                    Collection.EnsureIndex(indexTuple.Item1, indexTuple.Item2);
            //                }
            //            }
        }

        public bool IndexExists(string indexName)
        {
            return IndexesExist(new string[] { indexName });
        }

        public bool IndexesExist(IEnumerable<string> indexNames)
        {
            return Collection.IndexExists(indexNames.ToArray());
        }

        public void DropIndex(string indexName)
        {
            DropIndexes(new string[] { indexName });
        }

        public void DropIndexes(IEnumerable<string> indexNames)
        {
            Collection.DropIndex(indexNames.ToArray());
        }

        public void DropAllIndexes()
        {
            Collection.DropAllIndexes();
        }

        #endregion

        public bool Undelete(string id)
        {
            var ret = false;

            //attempt to find item in AuditObjects that matches type and id.  If found, verify does not exist in current collection.
            var auditRepo = Repositories.Instance.Repository<AuditObject>(_mongoConfig);
            var collNameForT = MongoUtil.GetCollectioNameFromInterface<T>();
            const string action = "DEL"; //TODO  change to constant

            var foundDeleted =
                Queryable.SingleOrDefault<AuditObject>(auditRepo.All(x => x.OriginalId == id && x.Action == action && x.Source == collNameForT));

            if (foundDeleted != null)
            {
                //make sure it's not in the current collection
                //TODO:  for classes that implement IEntity (override type of Id) getting the discrimator error

                T toRestore = (T)BsonSerializer.Deserialize((BsonDocument)foundDeleted.Item, typeof(T));

                bool exists = FindOneById(toRestore._id) != null;

                if (!exists)
                {
                    //insert, then remove from AuditObject
                    //TODO:  add log function to record this event
                    //                    T toRestore = (T) BsonSerializer.Deserialize(foundDeleted.Item, typeof (T));
                    RequestStart();
                    Add(toRestore);
                    auditRepo.Delete(foundDeleted);
                    RequestDone();
                    ret = true;
                }
            }

            return ret;
        }

        public IQueryable<T> FindDeleted(Expression<Func<T, bool>> criteria)
        {

            string colName = MongoUtil.GetCollectioNameFromInterface<T>();

            IQueryable<AuditObject> allAuditObjs = Repositories.Instance.Repository<AuditObject>(_mongoConfig).All(x => x.Action == "DEL" && x.Source == colName);

            var list = allAuditObjs.ToList();

            var asT = list.AsQueryable().Select(item => item.Item).Select(doc => (T)BsonSerializer.Deserialize((BsonDocument)doc, typeof(T))).ToList().AsQueryable();

            return asT.Where(criteria);

        }

        public IQueryable<T> FindDeleted()
        {
            MongoCursor<AuditObject> allAuditObjs = Repositories.Instance.Repository<AuditObject>(_mongoConfig).FindAll();

            string colName = MongoUtil.GetCollectioNameFromInterface<T>();

            var asAuditObjs = allAuditObjs.Where(x => x.Source == colName && x.Action == "DEL").ToList();

            return asAuditObjs.Select(item => item.Item).Select(doc => (T)BsonSerializer.Deserialize((BsonDocument)doc, typeof(T))).ToList().AsQueryable();

        }

        public void DeleteFromAuditing(DocumentAuditEnum auditType, string id)
        {
            if ((auditType & DocumentAuditEnum.Edit) == DocumentAuditEnum.Edit)
            {
                Repositories.Instance.Repository<AuditObject>(_mongoConfig).Delete(x => x.Action == "UPD" && x.Source == MongoUtil.GetCollectioNameFromInterface<T>() && x.OriginalId == id);
            }
            if ((auditType & DocumentAuditEnum.Delete) == DocumentAuditEnum.Delete)
            {
                Repositories.Instance.Repository<AuditObject>(_mongoConfig).Delete(x => x.Action == "DEL" && x.Source == MongoUtil.GetCollectioNameFromInterface<T>() && x.OriginalId == id);
            }
        }

        /// <summary>
        /// Delete any auditing entries from AuditObjects collection.
        /// WARNING:  Source item MUST exist in order to delete from auditing using this method.
        /// To delete auditing history for a non-existant object, us the DeleteFromAuditing(DocumentAuditEnum, ID) overload.
        /// </summary>
        /// <param name="auditType"></param>
        /// <param name="criteria"></param>
        public void DeleteFromAuditing(DocumentAuditEnum auditType, Expression<Func<T, bool>> criteria)
        {
            string colName = MongoUtil.GetCollectioNameFromInterface<T>();

            if (colName == "AuditObjects")
            {
                throw new Exception(
                    "RepositoryMongo.DeleteFromAuditing cannot be used for audit objects themselves.  To delete an AuditOjbect, use the Delete method");
            }

            var audit = typeof(T).GetCustomAttributes(typeof(DocumentAuditAttr), false).SingleOrDefault();

            if (audit != null)
            {
                var sourceObjects = All(criteria);

                if (((audit as DocumentAuditAttr).AuditEnum & DocumentAuditEnum.Edit) == DocumentAuditEnum.Edit)
                {
                    foreach (var soureObject in sourceObjects)
                    {
                        Repositories.Instance.Repository<AuditObject>(_mongoConfig).Delete(x => x.Action == "UPD" && x.Source == colName && x.OriginalId == soureObject._id);
                    }

                }
                if (((audit as DocumentAuditAttr).AuditEnum & DocumentAuditEnum.Delete) == DocumentAuditEnum.Delete)
                {
                    //delete where action = "DEL";
                    foreach (var soureObject in sourceObjects)
                    {
                        Repositories.Instance.Repository<AuditObject>(_mongoConfig).Delete(x => x.Action == "DEL" && x.Source == colName && x.OriginalId == soureObject._id);
                    }

                }

            }



        }

        /// <summary>
        /// Quick way to convert a list of expressions into a IMongoQuery
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <param name="combinationOperatorsEnum"></param>
        /// <returns></returns>
        public IMongoQuery ConvertExpressionListToGGQuery(List<Expression<Func<T, bool>>> criteria, CombinationOperatorsEnum combinationOperatorsEnum)
        {
            //TODO:  Not critical, but figure out how to implement Exclusive Or with Mongo Queries
            if (combinationOperatorsEnum != CombinationOperatorsEnum.And && combinationOperatorsEnum != CombinationOperatorsEnum.Or)
            {
                throw new ArgumentException("Find method can only handle 'And' and 'Or' combination operators");
            }

            var builder = new QueryBuilder<T>();
            var queries = new List<IMongoQuery>();

            foreach (Expression<Func<T, bool>> crit in criteria)
            {
                queries.Add(builder.Where(crit));
            }

            IMongoQuery query = combinationOperatorsEnum == CombinationOperatorsEnum.And
                                    ? Query.And(queries)
                                    : Query.Or(queries);

            return query;

        }


        public T MapReduce(string map, string reduce)
        {
            throw new NotImplementedException();


        }

        public IQueryable<T> FindRevisions(Expression<Func<T, bool>> criteria)
        {
            string colName = MongoUtil.GetCollectioNameFromInterface<T>();

            IQueryable<AuditObject> allAuditObjs = Repositories.Instance.Repository<AuditObject>(_mongoConfig).All(x => x.Action == "UPD" && x.Source == colName);

            var list = allAuditObjs.ToList();

            IList<T> toRet = new List<T>();
            foreach (var auditItem in allAuditObjs)
            {
                toRet.Add((T)BsonSerializer.Deserialize((BsonDocument)auditItem.Item, typeof(T)));
            }

            return toRet.AsQueryable().Where(criteria);
        }

        public static void AuditAddEdit(T entity)
        {
            if (entity == null) return;

//            var audit = (IAuditFields)entity;
//
//            MongoUtil.Audit(audit);
        }
    }

    //    public interface IMongoCursor<T>
    //    {
    //        IMongoCursor<T> SetFields(params string[] fields);
    //    }
    //
    //    public class MongoCursorWrapper<T>: IMongoCursor<T>
    //    {
    //        private readonly MongoCursor<T> _cursor;
    //
    //        public MongoCursorWrapper(MongoCursor<T> cursor )
    //        {
    //            _cursor = cursor;
    //        }
    //
    //        public IMongoCursor<T> SetFields(params string[] fields)
    //        {
    //            return _cursor.SetFields(fields);
    //        }
    //    }
}
