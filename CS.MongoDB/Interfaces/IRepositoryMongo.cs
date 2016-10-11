using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CS.Common.Enums;
using CS.Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace CS.MongoDB.Interfaces
{
    public interface IRepositoryMongo<T> where T : class,IEntity
    {
        MongoCollection TEMPCOL { get; }

        IBuildPropNames<T> GetBuildPropNames();
        dynamic FormatID<X>(string id);
        IEnumerable<WriteConcernResult> Add(IEnumerable<T> entities);
        WriteConcernResult Add(T entity);
        IQueryable<T> All();
        IQueryable<T> All(Expression<Func<T, bool>> criteria);
        long Count();
        void RenameElement(string oldName, string newName, params string[] caseSensitivePKColumns);

        IQueryable<T> FindDeleted();
        IQueryable<T> FindDeleted(Expression<Func<T, bool>> criteria);
        IQueryable<T> FindRevisions(Expression<Func<T, bool>> criteria);
        void DeleteFromAuditing(DocumentAuditEnum auditType, Expression<Func<T, bool>> criteria);
        void DeleteFromAuditing(DocumentAuditEnum auditType, string id);

        IMongoQuery ConvertExpressionListToGGQuery(List<Expression<Func<T, bool>>> criteria,
                                                   CombinationOperatorsEnum combinationOperatorsEnum);

        void Delete(T entity);
        void Delete(T entity, bool savedCopyOfDeletedItem);
        void Delete(Expression<Func<T, bool>> criteria, bool savedCopyOfDeletedItems);
        void Delete(Expression<Func<T, bool>> criteria);
        void Delete(string id);
        void Delete(string id, bool saveCopyOfDeletedItem);
        void DeleteAll();
        bool Equals(object obj);
        bool Exists(Expression<Func<T, bool>> criteria);
        T GetById(string id);
        T GetSingle(Expression<Func<T, bool>> criteria);
        void RequestDone();
        IDisposable RequestStart();
        IDisposable RequestStart(ReadPreference readPreference);
        T Update(T entity);
        void Update(IEnumerable<T> entities);

        /// <summary>
        /// Drops the collection
        /// </summary>
        void Drop();
        bool Exists();
        CollectionStatsResult GetStats();
        ValidateCollectionResult Validate();

        long GetTotalDataSize();
        long GetTotalStorageSize();
        bool IsCapped();

        void Reindex();
        GetIndexesResult GetIndexes();

        void EnsureIndexes(IMongoIndexKeys indexKeys, IMongoIndexOptions indexOptions);
        bool IndexExists(string indexName);
        bool IndexesExist(IEnumerable<string> indexNames);
        void DropIndex(string indexName);
        void DropIndexes(IEnumerable<string> indexNames);
        void DropAllIndexes();
        void EnsureIndexes(IIndexFactory factory);

        bool Undelete(string id);

        MongoCursor<T> FindAll();
        IQueryable FindAllAsGenericQueryable();
        MongoCursor<T> FindAllWildcardSearch(string elementName, string searchText);
        MongoCursor<T> Find(IMongoQuery query);
        MongoCursor<T> Find(IMongoQuery query, params string[] propertiesToReturn);

        MongoCursor<Y> FindAs<Y>(IMongoQuery query);
        MongoCursor<Y> FindAllAs<Y>();
        MongoCursor FindAllAs(Type type);
        MongoCursor FindAsBsonDocument(IMongoQuery query);

        T MapReduce(string map, string reduce);

        FindAndModifyResult FindAndUpdate(IMongoQuery query, IMongoSortBy sort, IMongoUpdate update);
        FindAndModifyResult FindAndUpdate(IMongoQuery query, IMongoSortBy sort, IMongoUpdate update, bool returnNew);

        FindAndModifyResult FindAndUpdate(IMongoQuery query, IMongoSortBy sort, IMongoUpdate update, bool returnNew,
                                          bool upsert);

        FindAndModifyResult FindAndUpdate(IMongoQuery query, IMongoSortBy sort, IMongoUpdate update, IMongoFields fields,
                                          bool returnNew, bool upsert);

        FindAndModifyResult FindAndRemove(IMongoQuery query, IMongoSortBy sort);

        IEnumerable<string> Distinct(string key);

        IEnumerable<string> Distinct(string key, IMongoQuery query);

        IEnumerable<string> Distinct(Expression<Func<T, string>> expr);

        T FindOne(IMongoQuery query);
        T FindOneById(string id);
        T FindOneById(string id, params string[] propertyNamesToReturn);

        IEnumerable<BsonDocument> Group(IMongoQuery query, BsonJavaScript bsonJavaScript, BsonDocument initial,
                                        BsonJavaScript reduce, BsonJavaScript finalize);

        IEnumerable<BsonDocument> Group(IMongoQuery query, IMongoGroupBy keys, BsonDocument initial,
                                        BsonJavaScript reduce, BsonJavaScript finalize);

        IEnumerable<BsonDocument> Group(IMongoQuery query, string key, BsonDocument initial, BsonJavaScript reduce,
                                        BsonJavaScript finalize);

        MapReduceResult MapReduce(BsonJavaScript map, BsonJavaScript reduce);

        MapReduceResult MapReduce(IMongoQuery query, BsonJavaScript map, BsonJavaScript reduce);

        MapReduceResult MapReduce(BsonJavaScript map, BsonJavaScript reduce, IMongoMapReduceOptions options);

        MapReduceResult MapReduce(IMongoQuery query, BsonJavaScript map, BsonJavaScript reduce,
                                  IMongoMapReduceOptions options);

        CommandResult ReIndex();

        WriteConcernResult Remove(IMongoQuery query);
        WriteConcernResult Remove(IMongoQuery query, RemoveFlags flags);
        WriteConcernResult Remove(IMongoQuery query, RemoveFlags flags, WriteConcern writeConcern);
        WriteConcernResult Remove(IMongoQuery query, WriteConcern writeConcern);
        WriteConcernResult RemoveAll();
        WriteConcernResult RemoveAll(WriteConcern writeConcern);

        WriteConcernResult Save(T item);
        WriteConcernResult Save(T item, MongoInsertOptions options);
        WriteConcernResult Save(T item, WriteConcern writeConcern);


        CommandResult RunCommand(IMongoCommand command);

        MongoCollectionSettings Settings { get; }

        void OnBeforeSave(T record);
        void OnAfterSave(T record);
        void OnBeforeDelete(T record);
        void OnAfterDelete(T record);

        WriteConcernResult Update(IMongoQuery query, UpdateBuilder update);
        WriteConcernResult Update(IMongoQuery query, UpdateBuilder update, MongoUpdateOptions updateOptions);
        WriteConcernResult Update(IMongoQuery query, UpdateBuilder update, WriteConcern writeConcern);
        WriteConcernResult Update(IMongoQuery query, UpdateBuilder update, UpdateFlags updateFlags);
        WriteConcernResult Update(IMongoQuery query, UpdateBuilder update, UpdateFlags updateFlags, WriteConcern writeConcern);
    }
}