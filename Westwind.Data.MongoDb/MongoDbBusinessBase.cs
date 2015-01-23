using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Westwind.Data.MongoDb.Properties;
using Westwind.Utilities;

namespace Westwind.Data.MongoDb
{



    /// <summary>
    /// Light weight MongoDb Business object base class
    /// that acts as a logical business container for MongoDb
    /// databases. 
    /// 
    /// Subclasses of this business object should be used to implement most data
    /// related logic that deals with creating, updating, removing and querying 
    /// of data use the MongoDb query or LINQ operators.
    /// 
    /// The business object provides base CRUD methods like Load, NewEntity,
    /// Remove that act on the specified entity type. The Save() method uses
    /// the EF specific context based SaveChanges
    /// which saves all pending changes (not just those for the current entity 
    /// and its relations). 
    /// 
    /// These business objects should be used as atomically as possible and 
    /// call Save() as often as possible to update pending data.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of the entity that this business object is tied to. 
    /// Note that you can access any of the context's entities - this entity
    /// is meant as a 'top level' entity that controls the operations of
    /// the CRUD methods. Maps to the Entity property on this class
    /// </typeparam>
    /// <typeparam name="TMongoContext">
    /// A MongoDbContext type that configures MongoDb driver behavior and startup operation.
    /// </typeparam>
    public class MongoDbBusinessBase<TEntity, TMongoContext> : IDisposable, IBusinessObject
        where TEntity : class, new()
        where TMongoContext : MongoDbContext, new()
    {

        /// <summary>
        /// Instance of the MongoDb core database instance.
        /// Set internally when the driver is initialized.
        /// </summary>
        public MongoDatabase Database { get; set; }

        protected string CollectionName { get; set; }
        protected Type EntityType = typeof(TEntity);
        protected TMongoContext Context = new TMongoContext();

        /// <summary>
        /// Re-usable MongoDb Collection instance.
        /// Set internally when the driver is initialized
        /// and accessible after that.
        /// </summary>
        public MongoCollection<TEntity> Collection
        {
            get
            {
                if (_collection == null)
                    _collection = Database.GetCollection<TEntity>(CollectionName);
                return _collection;
            }
        }
        private MongoCollection<TEntity> _collection;


        /// <summary>
        /// A collection that holds validation errors after Validate()
        /// or Save with AutoValidate on is called
        /// </summary>
        public ValidationErrorCollection ValidationErrors
        {
            get
            {
                if (_validationErrors == null)
                    _validationErrors = new ValidationErrorCollection();
                return _validationErrors;
            }
        }
        private ValidationErrorCollection _validationErrors;

        /// <summary>
        /// Determines whether or not the Save operation causes automatic
        /// validation. Default is false.
        /// </summary>                        
        public bool AutoValidate { get; set; }

        /// <summary>
        /// Internally loaded instance from load and newentity calls
        /// </summary>
        public TEntity Entity { get; set; }


        /// <summary>
        /// Error Message set by the last operation. Check if 
        /// results of a method call return an error status.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                if (ErrorException == null)
                    return "";
                return ErrorException.Message;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    ErrorException = null;
                else
                    // Assign a new exception
                    ErrorException = new ApplicationException(value);
            }
        }

        /// <summary>
        /// Instance of an exception object that caused the last error
        /// </summary>
        public Exception ErrorException
        {
            get { return _errorException; }
            set { _errorException = value; }
        }

        [NonSerialized]
        private Exception _errorException;


        #region ObjectInitializers



        /// <summary>
        /// Base constructor using default behavior loading context by 
        /// connectionstring name.
        /// </summary>
        /// <param name="connectionString">Connection string name</param>
        public MongoDbBusinessBase(string collection = null, string database = null, string connectionString = null)
        {
            InitializeInternal();

            Context = new TMongoContext();
            Database = GetDatabase(collection, database, connectionString);

            if (!Database.CollectionExists(CollectionName))
            {
                if (string.IsNullOrEmpty(CollectionName))
                    CollectionName = Pluralizer.Pluralize(EntityType.Name);

                Database.CreateCollection(CollectionName);
            }

            Initialize();
        }



        /// <summary>
        /// Specialized CreateContext that accepts a connection string and provider.
        /// Creates a new context based on a Connection String name or
        /// connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <remarks>Important: 
        /// This only works if you implement a DbContext contstructor on your custom context
        /// that accepts a connectionString parameter.
        /// </remarks>
        protected virtual MongoDatabase GetDatabase(string collection = null,
            string database = null,
            string serverString = null)
        {

            var db = Context.GetDatabase(serverString, database);

            if (string.IsNullOrEmpty(collection))
                collection = Pluralizer.Pluralize(typeof(TEntity).Name);

            CollectionName = collection;

            return db;
        }


        /// <summary>
        /// Override to hook post Context intialization
        /// Fired by all constructors.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Internal common pre-Context creation initialization code
        /// fired by all constructors
        /// </summary>
        private void InitializeInternal()
        {
            // nothing to do yet, but we'll use this for sub objects
            // and potential meta data pre-parsing           
        }



        #endregion

        /// <summary>
        /// Finds an individual entity based on the entity tyep
        /// of this application.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public TEntity FindOne(IMongoQuery query, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            return Database.GetCollection<TEntity>(collectionName).FindOne(query);
        }


        /// <summary>
        /// Finds an individual entity based on the entity type passed in
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public T FindOne<T>(IMongoQuery query, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            return Database.GetCollection<T>(collectionName).FindOne(query);
        }

        public IEnumerable<T> Find<T>(IMongoQuery query, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            return Database.GetCollection<T>(collectionName).Find(query);
        }

        /// <summary>
        /// Allows you to query for a single entity  using a Mongo Shell query 
        /// string. Uses the default entity defined.
        /// </summary>
        /// <param name="jsonQuery">Json object query string</param>
        /// <param name="collectionName">Optional - name of the collection to search</param>
        /// <returns>Collection of items or null if none</returns>
        public TEntity FindOneFromString(string jsonQuery, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            var query = GetQueryFromString(jsonQuery);
            return Database.GetCollection<TEntity>(collectionName).FindOne(query);
        }

        /// <summary>
        /// Allows you to query for a single entity  using a Mongo Shell query 
        /// string. Uses the entity type passed.
        /// </summary>
        /// <param name="jsonQuery">Json object query string</param>
        /// <param name="collectionName">Optional - name of the collection to search</param>
        /// <returns>Collection of items or null if none</returns>
        public T FindOneFromString<T>(string jsonQuery, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            var query = GetQueryFromString(jsonQuery);
            return Database.GetCollection<T>(collectionName).FindOne(query);
        }

        /// <summary>
        /// Allows you to query for a single entity  using a Mongo Shell query 
        /// string. Uses the entity type passed.
        /// </summary>
        /// <param name="jsonQuery">Json object query string</param>
        /// <param name="collectionName">Optional - name of the collection to search</param>
        /// <returns>Collection of items or null if none</returns>
        public string FindOneFromStringJson(string jsonQuery, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            var query = GetQueryFromString(jsonQuery);
            var cursor = Database.GetCollection(collectionName).FindOne(query);
            if (cursor == null)
                return null;

            return cursor.ToJson();
        }


        public IEnumerable<TEntity> FindAll(string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            return Database.GetCollection<TEntity>(collectionName).FindAll();
        }

        public IEnumerable<T> FindAll<T>(string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            return Database.GetCollection<T>(collectionName).FindAll();
        }



        public IEnumerable<TEntity> FindFromString(string jsonQuery, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            var query = GetQueryFromString(jsonQuery);
            var items = Database.GetCollection<TEntity>(collectionName).Find(query);

            return items;
        }


        /// <summary>
        /// Allows you to query Mongo using a Mongo Shell query 
        /// string and explicitly specify the result type.
        /// </summary>
        /// <param name="jsonQuery">Json object query string</param>
        /// <param name="collectionName">Optional - name of the collection to search</param>
        /// <returns>Collection of items or null if none</returns>    
        public IEnumerable<T> FindFromString<T>(string jsonQuery, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            var query = GetQueryFromString(jsonQuery);
            var items = Database.GetCollection<T>(collectionName).Find(query);

            return items;
        }

        /// <summary>
        /// Allows you to query Mongo using a Mongo Shell query 
        /// string.
        /// </summary>
        /// <param name="jsonQuery">Json object query string</param>
        /// <param name="collectionName">Optional - name of the collection to search</param>
        /// <returns>Collection of items or null if none</returns>
        public string FindFromStringJson(string jsonQuery, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            var query = GetQueryFromString(jsonQuery);
            var cursor = Database.GetCollection(collectionName).Find(query);

            return cursor.ToJson();
        }

        /// <summary>
        /// Allows you to query Mongo using a Mongo Shell query 
        /// by providing a .NET object that is translated into
        /// the appropriate JSON/BSON structure. 
        /// 
        /// This might be easier to write by hand than JSON strings
        /// in C# code.
        /// </summary>
        /// <param name="queryObject">Any .NET object that conforms to Mongo query object structure</param>
        /// <param name="collectionName">Optional - name of the collection to search</param>
        /// <returns>Collection of items or null if none</returns>
        public IEnumerable<TEntity> FindFromObject(object queryObject, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            var query = new QueryDocument(queryObject.ToBsonDocument());
            var items = Database.GetCollection<TEntity>(collectionName).Find(query);

            return items;
        }

        /// <summary>
        /// Allows you to query Mongo using a Mongo Shell query 
        /// by providing a .NET object that is translated into
        /// the appropriate JSON/BSON structure. This version
        /// allows you to specify the result type explicitly.
        /// 
        /// This might be easier to write by hand than JSON strings
        /// in C# code.
        /// </summary>
        /// <param name="queryObject">Any .NET object that conforms to Mongo query object structure</param>
        /// <param name="collectionName">Optional - name of the collection to search</param>
        /// <returns>Collection of items or null if none</returns>
        public IEnumerable<T> FindFromObject<T>(object queryObject, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            var query = new QueryDocument(queryObject.ToBsonDocument());
            var items = Database.GetCollection<T>(collectionName).Find(query);

            return items;
        }

        /// <summary>
        /// Creates a Bson Query document from a Json String.
        /// 
        /// You can pass this as a Query operation to any of the
        /// Collection methods that expect a query.
        /// </summary>
        /// <param name="jsonQuery"></param>
        /// <returns></returns>
        public QueryDocument GetQueryFromString(string jsonQuery)
        {
            return new QueryDocument(BsonSerializer.Deserialize<BsonDocument>(jsonQuery));
        }

        /// <summary>
        /// Creates a new instance of an Entity tracked
        /// by the DbContext.
        /// </summary>
        /// <returns></returns>
        public virtual TEntity NewEntity()
        {
            Entity = new TEntity();

            OnNewEntity(Entity);

            if (Entity == null)
                return null;

            return Entity;
        }

        /// <summary>
        /// Adds a new entity as if it was created and fires
        /// the OnNewEntity internally. If NULL is passed in
        /// a brand new entity is created and passed back.
        /// 
        /// This allows for external creation of the entity
        /// and then adding the entity to the context after
        /// the fact.
        /// </summary>
        /// <param name="entity">An entity instance</param>
        /// <returns></returns>
        public virtual TEntity NewEntity(TEntity entity)
        {
            if (entity == null)
                return NewEntity();

            Entity = entity;
            OnNewEntity(Entity);

            if (Entity == null)
                return null;

            return Entity;
        }


        /// <summary>
        /// Overridable method that allows adding post NewEntity functionaly
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void OnNewEntity(TEntity entity)
        {
        }


        /// <summary>
        /// Loads in instance based on its string id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Load(string id)
        {
            return LoadBase(id);
        }

        /// <summary>
        /// Loads in instance based on its string id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Load(int id)
        {
            return LoadBase(id);
        }

        /// <summary>
        /// Loads an instance based on its key field id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual TEntity LoadBase(string id)
        {
            Entity = Collection.FindOneByIdAs(typeof(TEntity), new BsonString(id)) as TEntity;

            if (Entity == null)
            {
                SetError("No match found.");
                return null;
            }

            OnEntityLoaded(Entity);

            return Entity;
        }


        /// <summary>
        /// Loads an instance based on its key field id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual TEntity LoadBase(int id)
        {
            Entity = Collection.FindOneByIdAs(typeof(TEntity), id) as TEntity;

            if (Entity == null)
            {
                SetError("No match found.");
                return null;
            }
            OnEntityLoaded(Entity);
            return Entity;
        }

        /// <summary>
        /// Loads an entity based on a Lambda expression
        /// </summary>
        /// <param name="whereClauseLambda"></param>
        /// <returns></returns>
        protected virtual TEntity LoadBase(Expression<Func<TEntity, bool>> whereClauseLambda)
        {
            SetError();
            Entity = null;

            try
            {
                var query = Query<TEntity>.Where(whereClauseLambda);
                Entity = Database.GetCollection<TEntity>(CollectionName).FindOne(query);

                if (Entity != null)
                    OnEntityLoaded(Entity);

                return Entity;
            }
            catch (InvalidOperationException)
            {
                // Handles errors where an invalid Id was passed, but SQL is valid
                SetError(Resources.CouldntLoadEntityInvalidKeyProvided);
                return null;
            }
            catch (Exception ex)
            {
                // handles Sql errors                                
                SetError(ex);
            }

            return null;
        }


        /// <summary>
        /// Fired after an entity has been loaded with the .Load() method
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void OnEntityLoaded(TEntity entity)
        {

        }


        /// <summary>
        /// removes an individual entity instance.
        /// 
        /// This method allows specifying an entity in a dbSet other
        /// then the main one as long as it's specified by the dbSet
        /// parameter.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dbSet">Optional - 
        /// Allows specifying the Collection to which the entity passed belongs.
        /// If not specified the current Collection for the current entity is used </param>
        /// <param name="saveChanges">Optional - 
        /// If true does a Context.SaveChanges. Set to false
        /// when other changes in the Context are pending and you don't want them to commit
        /// immediately
        /// </param>
        /// <param name="noTransaction">Optional - 
        /// If true the Delete operation is wrapped into a TransactionScope transaction that
        /// ensures that OnBeforeDelete and OnAfterDelete all fire within the same Transaction scope.
        /// Defaults to false as to improve performance.
        /// </param>
        public virtual bool Delete(TEntity entity)
        {
            if (entity == null)
                entity = Entity;

            if (entity == null)
                return true;

            if (!DeleteInternal(entity))
                return false;

            return true;
        }


        /// <summary>
        /// Deletes an entity from the main entity set
        /// based on a key value.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(string id)
        {
            var query = Query.EQ("_id", new BsonString(id));
            var result = Collection.Remove(query);
            if (result.HasLastErrorMessage)
            {
                SetError(result.ErrorMessage);
                return false;
            }
            return true;
        }

        public virtual bool Delete(int id)
        {
            var query = Query.EQ("_id", id);
            var result = Collection.Remove(query);
            if (result.HasLastErrorMessage)
            {
                SetError(result.ErrorMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Actual delete operation that removes an entity
        /// </summary>
        private bool DeleteInternal(TEntity entity)
        {
            if (!OnBeforeDelete(entity))
                return false;

            try
            {
                var query = Query.EQ("_id", new BsonString(((dynamic)entity).Id.ToString()));
                var result = Collection.Remove(query);

                if (result.HasLastErrorMessage)
                {
                    SetError(result.ErrorMessage);
                    return false;
                }

                if (!OnAfterDelete(entity))
                    return false;
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Called before a delete operation occurs
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual bool OnBeforeDelete(TEntity entity)
        {
            return true;
        }

        /// <summary>
        /// Called after a resource is deleted. Runs within the same
        /// transaction scope
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual bool OnAfterDelete(TEntity entity)
        {
            return true;
        }


        //protected DbTransaction BeginTransaction(IsolationLevel level = IsolationLevel.Unspecified)
        //{
        //    if (Context.DatabaseName.Connection.State != ConnectionState.Open)
        //        Context.DatabaseName.Connection.Open();

        //    return Context.DatabaseName.Connection.BeginTransaction(level);
        //}

        //public void CommitTransaction()
        //{
        //    Context.DatabaseName.Connection.
        //}

        //public void RollbackTransaction()
        //{
        //}


        /// <summary>
        /// Saves all changes. 
        /// </summary>
        /// <remarks>
        /// This method calls Context.SaveChanges() so it saves
        /// all changes made in the context not just changes made
        /// to the current entity. It's crucial to Save() as
        /// atomically as possible or else use separate Business
        /// object instances with separate contexts.
        /// </remarks>
        /// <returns></returns>
        public bool Save(TEntity entity = null, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            if (entity == null)
                entity = Entity;

            // hook point - allow logic to abort saving
            if (!OnBeforeSave(entity))
                return false;

            // now do validations
            if (AutoValidate)
            {
                if (!Validate(entity))
                    return false;
            }

            try
            {
                var result = Collection.Save(entity);
                if (result.HasLastErrorMessage)
                {
                    SetError(result.LastErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return false;
            }

            if (!OnAfterSave(Entity))
                return false;


            return true;
        }

        /// <summary>
        /// Saves an entity based on a provided type.
        /// </summary>
        /// <remarks>
        /// This version of Save() does not run Validation, or
        /// before and after save events since it's not tied to
        /// the current entity type. If you want the full featured
        /// save use the non-generic Save() operation.
        /// </remarks>
        /// <returns></returns>
        public bool Save<T>(T entity, string collectionName = null)
            where T : class, new()
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = Pluralizer.Pluralize(typeof(T).Name);

            if (entity == null)
            {
                SetError("No entity to save passed.");
                return false;
            }

            try
            {
                var result = Database.GetCollection(collectionName).Save(entity);
                if (result.HasLastErrorMessage)
                {
                    SetError(result.LastErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="collectionName"></param>
        /// <returns>Id of object saved</returns>
        public string SaveFromJson(string entityJson, string collectionName = null)
        {
            if (string.IsNullOrEmpty(collectionName))
                collectionName = CollectionName;

            if (string.IsNullOrEmpty(entityJson))
            {
                SetError("No entity to save passed.");
                return null;
            }

            try
            {
                var doc = BsonDocument.Parse(entityJson);
                if (doc == null)
                {
                    SetError("No entity to save passed.");
                    return null;
                }

                var result = Database.GetCollection(collectionName).Save(doc);

                if (result.HasLastErrorMessage)
                {
                    SetError(result.LastErrorMessage);
                    return null;
                }

                var id = doc["_id"].AsString;
                return id;
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                return null;
            }

        }

        /// <summary>
        /// Hook point fired just before the save method is called.
        /// 
        /// Override this method to fix up entity values before a save
        /// operation occurs.                
        /// </summary>
        /// <returns>return true to save or false to avoid saving</returns>
        /// <param name="entity"></param>
        protected virtual bool OnBeforeSave(TEntity entity)
        {
            return true;
        }

        /// <summary>
        /// Hook point fired after the Save operation has completed
        /// successfully. Note doesn't fire if the Save() operation
        /// fails.
        /// 
        /// Override this method to fix up or fire actions after
        /// the Save operation completes.
        /// </summary>
        /// <param name="entity"></param>
        protected virtual bool OnAfterSave(TEntity entity)
        {
            return true;
        }


        /// <summary>
        /// Validate() is used to validate business rules on the business object. 
        /// Validates both EF entity validation rules on pending changes as well
        /// as any custom validation rules you implement in the OnValidate() method.
        /// 
        /// Do not override this method for custom Validation(). Instead override
        /// OnValidate() or add error entries to the ValidationErrors collectionName.        
        /// <remarks>
        /// If the AutoValidate flag is set to true causes Save()
        /// to automatically call this method. Must be overridden to perform any 
        /// validation.
        /// </remarks>
        /// <seealso>Class wwBusiness Class ValidationErrorCollection</seealso>
        /// </summary>
        /// <param name="entity">Optional entity to validate. Defaults to this.Entity</param>
        /// <param name="clearValidationErrors">If true clears all validation errors before processing rules</param>
        /// <returns>True or False.</returns>
        public bool Validate(TEntity entity = null, bool clearValidationErrors = false)
        {
            if (clearValidationErrors)
                ValidationErrors.Clear();

            if (entity == null)
                entity = Entity;

            // No entity - no validation errors
            if (entity == null)
                return true;

            // call business object level validation errors                        
            OnValidate(entity);

            if (ValidationErrors.Count > 0)
            {
                SetError(ValidationErrors.ToString());
                return false;
            }

            return true;
        }



        /// <summary>
        /// Method that should be overridden in a business object to handle actual validation. 
        /// This method is called from the Validate method.
        /// 
        /// This method should add any errors to the <see cref="ValidationErrors"/> collectionName.
        /// </summary>
        /// <param name="entity">The entity to be validated</param>
        protected virtual void OnValidate(TEntity entity)
        {
        }


        #region GenericPropertyStorage

        /// <summary>
        // Dictionary of arbitrary property values that can be attached
        // to the current object. You can use GetProperties, SetProperties
        // to load the properties to and from a text field.
        /// </summary>
        public PropertyBag Properties
        {
            get
            {
                if (_Properties == null)
                    _Properties = new PropertyBag();
                return _Properties;
            }
            private set { _Properties = value; }
        }

        private PropertyBag _Properties;

        /// <summary>
        /// Retrieves a value from the Properties collectionName safely.
        /// If the value doesn't exist null is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetProperty(string key)
        {
            if (Properties == null)
                return null;

            object value = null;
            Properties.TryGetValue(key, out value);

            return value;
        }

        /// <summary>
        /// Loads the Properties dictionary with values from a Properties property of 
        /// an entity object. Once loaded you can access the dictionary to read and write
        /// values from it arbitrarily and use SetProperties to write the values back
        /// in serialized form to the underlying property for database storage.
        /// </summary>
        /// <param name="stringFieldNameToLoadFrom">The name of the field to load the XML properties from.</param>
        protected void GetProperties(string stringFieldNameToLoadFrom = "Properties", object entity = null)
        {
            Properties = null;

            if (entity == null)
                entity = Entity;

            // Always create a new property bag
            Properties = new PropertyBag();

            string fieldValue = ReflectionUtils.GetProperty(entity, stringFieldNameToLoadFrom) as string;
            if (string.IsNullOrEmpty(fieldValue))
                return;

            // load up Properties from XML                       
            Properties.FromXml(fieldValue);
        }

        /// <summary>
        /// Saves the Properties Dictionary - in serialized string form - to a specified entity field which 
        /// in turn allows writing the data back to the database.
        /// </summary>
        /// <param name="stringFieldToSaveTo"></param>
        protected void SetProperties(string stringFieldToSaveTo = "Properties", object entity = null)
        {
            if (entity == null)
                entity = Entity;


            string xml = null;
            if (Properties.Count > 0)
            {
                // Serialize to Xm
                xml = Properties.ToXml();
            }
            ReflectionUtils.SetProperty(entity, stringFieldToSaveTo, xml);
        }

        #endregion



        ///// <summary>
        ///// Checks to see if the current entity has been added
        ///// to the data context as a new entity
        ///// 
        ///// This entity specific version is more efficient
        ///// than the generic object parameter version.
        ///// </summary>
        ///// <param name="entity"></param>
        ///// <returns></returns>
        //protected bool IsNewEntity(TEntity entity)
        //{
        //    DbEntityEntry entry = GetEntityEntry(entity);
        //    if (entry == null)
        //        throw new ArgumentException(Resources.EntityIsNotPartOfTheContext);

        //    return entry.State == EntityState.Added;
        //}

        ///// <summary>
        ///// Checks to see if the current entity has been added
        ///// to the data context as a new entity
        ///// </summary>
        ///// <param name="entity"></param>
        ///// <returns></returns>
        //protected bool IsNewEntity(object entity)
        //{
        //    DbEntityEntry entry = GetEntityEntry(entity);
        //    if (entry == null)
        //        throw new ArgumentException(Resources.EntityIsNotPartOfTheContext);

        //    return entry.State == EntityState.Added;
        //}


        /// <summary>
        /// Sets an internal error message.
        /// </summary>
        /// <param name="Message"></param>
        public void SetError(string Message)
        {
            if (string.IsNullOrEmpty(Message))
            {
                ErrorException = null;
                return;
            }

            ErrorException = new ApplicationException(Message);

            //if (Options.ThrowExceptions)
            //    throw ErrorException;

        }

        /// <summary>
        /// Sets an internal error exception
        /// </summary>
        /// <param name="ex"></param>
        public void SetError(Exception ex, bool checkInnerException = false)
        {
            ErrorException = ex;

            if (checkInnerException)
            {
                while (ErrorException.InnerException != null)
                {
                    ErrorException = ErrorException.InnerException;
                }
            }

            ErrorMessage = ErrorException.Message;
            //if (ex != null && Options.ThrowExceptions)
            //    throw ex;
        }

        /// <summary>
        /// Clear out errors
        /// </summary>
        public void SetError()
        {
            ErrorException = null;
            ErrorMessage = null;
        }

        /// <summary>
        /// Overridden to display error messages if one exists
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                return "Error: " + ErrorMessage;

            return base.ToString();
        }

        private bool disposed;

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            if (Database != null)
            {
                Database = null;
            }
        }
    }

    /// <summary>
    /// A MongoDb Business object and data acces library that provides a thin 
    /// business wrapper around the MongoDb C# driver to simplify common CRUD
    /// operations and common data queries.
    /// 
    /// Use this non-generic version, if you don't have matching entities to
    /// load data into or your want to run string queries. 
    /// </summary>
    public class MongoDbBusinessBase : MongoDbBusinessBase<object, MongoDbContext>
    {

        /// <summary>
        /// Base constructor using default behavior loading context by 
        /// connectionstring name.
        /// </summary>
        /// <param name="connectionString">Connection string name</param>
        public MongoDbBusinessBase(string collection = null, string database = null, string connectionString = null)
            : base(collection, database, connectionString)
        {
        }
    }
}
