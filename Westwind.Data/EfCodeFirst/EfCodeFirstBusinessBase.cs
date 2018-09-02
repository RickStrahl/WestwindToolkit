using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Xml.Serialization;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using Westwind.Data.Properties;
using System.Linq;
using Westwind.Utilities;
using System.Linq.Expressions;
using System.Transactions;
using System.ComponentModel.DataAnnotations.Schema;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Westwind.Data.EfCodeFirst
{

    /// <summary>
    /// Light weight Entity Framework Code First Business object base class
    /// that acts as a logic container for an entity DbContext instance. 
    /// 
    /// Subclasses of this business object should be used to implement most data
    /// related logic that deals with creating, updating, removing and querying 
    /// of data use EF Code First.
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
    /// The type of the 'primary' entity that this business object is tied to. 
    /// Note that you can access any of the context's entities - this entity
    /// is meant as a 'top level' entity that controls the operations of
    /// the CRUD methods.
    /// Maps to the Entity property.
    /// </typeparam>
    /// <typeparam name="TContext">
    /// The type of the context that is attached to the this business object.
    /// Maps to the Context property. 
    /// </typeparam>
    public class EfCodeFirstBusinessBase<TEntity, TContext> : IDisposable, IBusinessObject<TContext>, IBusinessObject<TEntity,TContext> where TEntity : class, new()
        where TContext : DbContext,new()
    {
        [XmlIgnore]
        [NotMapped]        
        /// <summary>
        /// Instance of the Context that is either auto-created
        /// or shared.
        /// </summary>
        public TContext Context { get; set; }

        /// <summary>
        /// Internally re-usable DbSet instance.
        /// </summary>
        protected DbSet<TEntity> DbSet
        {
            get {
                if (_dbSet == null)
                    _dbSet = Context.Set<TEntity>();
                return _dbSet; 
            }            
        }
        private DbSet<TEntity> _dbSet;

        /// <summary>
        /// Get an instance of the underlying object context
        /// </summary>
        protected ObjectContext ObjectContext 
        {
            get
            {
                if (_objectContext == null)
                    _objectContext = ((IObjectContextAdapter)Context).ObjectContext;
            	return _objectContext;
            }
        }
        ObjectContext _objectContext;

        /// <summary>
        /// A collection that can be used to hold errors or
        /// validation errors. This 
        /// </summary>
        [XmlIgnore]
        [NotMapped]
        public ValidationErrorCollection ValidationErrors
        {
            get
            {
                if (_validationErrors == null)
                    _validationErrors = new ValidationErrorCollection();
                return _validationErrors;
            }
        }
        ValidationErrorCollection _validationErrors;

        /// <summary>
        /// Determines whether or not the Save operation causes automatic
        /// validation
        /// </summary>                        
        public bool AutoValidate { get; set; }

        /// <summary>
        /// Internally loaded instance from load and newentity calls
        /// </summary>
        public TEntity Entity { get; set; }


        /// <summary>
        /// Error Message of the last exception
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
        [NotMapped]
        [XmlIgnore]
        public Exception ErrorException { get; set; }        

        [NotMapped]
        [XmlIgnore]
        public bool ThrowExceptions {get; set; }
        

        #region ObjectInitializers and Disposables

        /// <summary>
        /// Base constructor using default behavior loading context by 
        /// connectionstring name that matches the context
        /// </summary>
        public EfCodeFirstBusinessBase()
        {
            InitializeInternal();
            Context = CreateContext();
            Initialize();
        }

        /// <summary>
        /// Base constructor using default behavior loading context by 
        /// connectionstring name.
        /// </summary>
        /// <param name="connectionString">Connection string name</param>
        public EfCodeFirstBusinessBase(string connectionString)
        {
            InitializeInternal();
            Context = CreateContext(connectionString);
            Initialize();
        }               


        /// <summary>
        /// Use this constructor to share a DbContext 
        /// from another business object.
        /// 
        /// Useful for 'child business' objects that
        /// need to operate from within internal business object
        /// operations.
        /// </summary>
        /// <param name="parentBusinessObject">An existing business object to share are context with</param>
        public EfCodeFirstBusinessBase(IBusinessObject<TContext> parentBusinessObject)
        {
            InitializeInternal(); 
            Context = parentBusinessObject.Context;
            Initialize();
        }

        /// <summary>
        /// Creates a new instance of the business Object
        /// and allows passing in an existing instance
        /// of a DbContext.
        /// </summary>
        /// <param name="context">An existing context to share with</param>
        public EfCodeFirstBusinessBase(TContext context)
        {
            InitializeInternal();
            Context = context;
            Initialize();
        }

        /// <summary>
        /// Simple factory method that creates a new Context.
        /// The default behavior simply creates the context by looking
        /// for a matching connectionstring with the same name in the 
        /// .config file.
        /// 
        /// If you need custom logic simply override this method and 
        /// return the context.
        /// 
        /// This method is called from the constructor to ensure that
        /// the Context exists before Initialize() is called.
        /// </summary>
        /// <returns></returns>
        protected virtual TContext CreateContext()
        {            
            return new TContext();                        
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
        public virtual TContext CreateContext(string connectionString)
        {
            TContext context = Activator.CreateInstance(typeof(TContext),connectionString) as TContext;
            
            if (context == null)
                throw new InvalidOperationException(Resources.ThisConstructorOnlyOnCustomContext);

            return context;
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


        private bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }

        #endregion

        #region CRUD Operations
        /// <summary>
        /// Creates a new instance of an Entity tracked
        /// by the DbContext.
        /// </summary>
        /// <returns></returns>
        public virtual TEntity NewEntity()
        {            
            Entity = Context.Set<TEntity>().Add(new TEntity());

            if (Entity == null)
                return null;

            OnNewEntity(Entity);
            
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
            
            // check to see if the entity already exists
            if (GetEntityEntry(entity) == null)
                Entity = Context.Set<TEntity>().Add(entity) as TEntity;
            else
                Entity = entity;
            
            OnNewEntity(Entity);

            return Entity;
        }


        /// <summary>
        /// Loads in instance based on its integer id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Load(int id)
        {
            return LoadBase(id);
        }

        /// <summary>
        /// Loads in instance based on its Guid id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Load(Guid id)
        {
            return LoadBase(id);
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
        /// Loads an instance based on its key field id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual TEntity LoadBase(object id)
        {
            object match = null;
            try
            {
                match = DbSet.Find(id);
                if (match == null)
                {
                    SetError(Resources.UnableToFindMatchingEntityForKey);
                    return null;
                }            
            }
            catch (Exception ex)
            {
                // handles Sql errors                                
                SetError(ex);
            }


            // Assign to internal member
            Entity = match as TEntity;            

            OnEntityLoaded(Entity);

            return Entity;
        }

        /// <summary>
        /// Loads an entity based on a Lambda expression
        /// </summary>
        /// <param name="whereClauseLambda"></param>
        /// <returns></returns>        "
        protected virtual TEntity LoadBase(Expression<Func<TEntity, bool>> whereClauseLambda)
        {
            SetError();
            Entity = null;

            try
            {                                
                Entity = DbSet.FirstOrDefault(whereClauseLambda);

                if (Entity != null)
                    OnEntityLoaded(Entity);
                else
                    SetError(Resources.UnableToFindMatchingEntityForKey);

                return Entity;
            }
            catch (InvalidOperationException)
            {
                // Handles errors where an invalid Id was passed, but SQL is valid                
                SetError( Resources.CouldnTLoadEntityInvalidKeyProvided);                            
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
        /// Attaches an untracked entity to an entity set and marks it as modified.
        /// Note: child elements need to be manually added.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entitySet"></param>
        /// <param name="markAsModified"></param>
        /// <param name="addNew"></param>
        /// <returns></returns>
        public object Attach(object entity, bool addNew = false, EntityState entityState = EntityState.Modified )
        {
            var dbSet = Context.Set(entity.GetType());

            if (addNew)
                dbSet.Add(entity);
            else
            {
                dbSet.Attach(entity);
                GetEntityEntry(entity).State = entityState;
            }

            return entity;
        }

        /// <summary>
        /// Attaches an untracked to the internal context and 
        /// marks it as modified optionally
        /// Note: child elements need to be manually added.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity Attach(TEntity entity, bool addNew = false)
        {
            if (addNew)
                Entity = DbSet.Add(entity);
            else
            {
                Entity = DbSet.Attach(entity);
                GetEntityEntry(Entity).State = EntityState.Modified;
            }

            return Entity;
        }

        /// <summary>
        /// Deletes an entity from the main entity set
        /// based on a key value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="saveChanges">if true changes are saved to disk. Otherwise entity is removed from context only</param>
        /// <returns></returns>
        public virtual bool Delete(object id, bool saveChanges = true, bool useTransaction = false)
        {
            TEntity entity = DbSet.Find(id);
            return Delete(entity, saveChanges: saveChanges, useTransaction: useTransaction);
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
        /// Allows specifying the DbSet to which the entity passed belongs.
        /// If not specified the current DbSet for the current entity is used </param>
        /// <param name="saveChanges">Optional - 
        /// If true does a Context.SaveChanges. Set to false
        /// when other changes in the Context are pending and you don't want them to commit
        /// immediately
        /// </param>
        /// <param name="useTransaction">Optional - 
        /// If true the Delete operation is wrapped into a TransactionScope transaction that
        /// ensures that OnBeforeDelete and OnAfterDelete all fire within the same Transaction scope.
        /// Defaults to false as to improve performance.
        /// </param>
        public virtual bool Delete(TEntity entity, DbSet dbSet = null, bool saveChanges = true, bool useTransaction = false)
        {
            if (entity == null)
                entity = Entity;

            if (entity == null)
                return true;

            if (useTransaction && saveChanges)
            {
                using (var trans = CreateTransactionScope())
                {
                    if (!DeleteInternal(entity, dbSet,saveChanges)) 
                        return false;

                    trans.Complete();
                }
            }
            else
            {
                 if (!DeleteInternal(entity, dbSet,saveChanges))
                    return false;
            }

            return true;
        }
    

        /// <summary>
        /// Actual delete operation that removes an entity
        /// </summary>
        private bool DeleteInternal(TEntity entity, DbSet dbSet, bool saveChanges)
        {
            if (!OnBeforeDelete(entity))
                return false;
            
            try
            {
                if (dbSet == null)
                    dbSet = DbSet;

                dbSet.Remove(entity);

                // one operation that immediately submits
                if (saveChanges)
                    Context.SaveChanges();

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
        /// Cancel Changes on the current connected context
        /// </summary>
        public virtual void AbortChanges()
        {
            // Create a new context instance from scratch
            Context.Dispose();  // close the old context

            // Create a new Context
            CreateContext();
        }

        //protected DbTransaction BeginTransaction(IsolationLevel level = IsolationLevel.Unspecified)
        //{
        //    if (Context.Database.Connection.State != ConnectionState.Open)
        //        Context.Database.Connection.Open();

        //    return Context.Database.Connection.BeginTransaction(level);
        //}

        //public void CommitTransaction()
        //{
        //    Context.Database.Connection.
        //}

        //public void RollbackTransaction()
        //{
        //}

        /// <summary>
        /// Validate() is used to validate business rules on the business object. 
        /// Validates both EF entity validation rules on pending changes as well
        /// as any custom validation rules you implement in the OnValidate() method.
        /// 
        /// Do not override this method for custom Validation(). Instead override
        /// OnValidate() or add error entries to the ValidationErrors collection.        
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

            var validationErrors = Context.GetValidationErrors();

            // First check for model validation errors
            foreach (var entry in validationErrors)
            {
                foreach (var error in entry.ValidationErrors)
                {
                    ValidationErrors.Add(error.ErrorMessage, error.PropertyName);
                }
            }

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
        /// Saves all changes. 
        /// </summary>
        /// <remarks>
        /// This method calls Context.SaveChanges() so it saves
        /// all changes made in the context not just changes made
        /// to the current entity. It's crucial to Save() as
        /// atomically as possible or else use separate Business
        /// object instances with separate contexts.
        /// </remarks>
        /// <param name="entity"></param>
        /// <param name="useTransactionScope">Optional -
        /// if true uses a transaction scope to wrap the save operation
        /// including the OnBeforeSave() and OnAfterSave() operations so
        /// they all run within the context of a single transaction that
        /// can be rolled back.      
        /// Use this if you have code in OnBeforeSave()/OnAfterChange() that
        /// might depend on a transaction or if you require that the Save()
        /// operation occurs under a specific Isolation Level (specified by
        /// overriding the GetTransactionScope() method).
        /// </param>
        /// <returns></returns>
        public virtual bool Save(TEntity entity = null, bool useTransactionScope = false )
        {
            if (entity == null)
                entity = Entity;

            if (useTransactionScope)
            {
                using (var scope = CreateTransactionScope())
                {
                    if (!SaveInternal(entity))
                        return false; // rolls back

                    scope.Complete();
                    return true;
                }                
            }
            
            return SaveInternal(entity);
        }

        /// <summary>
        /// Save currently attached entity.
        /// </summary>
        /// <returns></returns>
        public virtual bool Save()
        {
            return Save(null, false);
        }


        /// <summary>
        /// Handles saving the actual entity by firing OnBeforeSave,SaveChanges and
        /// OnAfterSave().
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual bool SaveInternal(TEntity entity)
        {
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
                Context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entry in ex.EntityValidationErrors)
                {
                    foreach (var error in entry.ValidationErrors)
                    {
                        ValidationErrors.Add(error.ErrorMessage, error.PropertyName);
                    }
                }
                SetError(ValidationErrors.ToString());
                OnAfterSaveError(entity);
                return false;
            }
            catch (DbUpdateException ex)
            {
                SetError(ex, true);
                OnAfterSaveError(entity);

                return false;
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                OnAfterSaveError(entity);
                return false;
            }

            if (!OnAfterSave(Entity))
                return false;

            return true;
        }

        #endregion

        #region Raw SQL Access
        /// <summary>
        /// Allows execution of an arbitrary non query SQL command against
        /// the database.
        /// 
        /// Format can either be named parameters (@pk, @name)
        /// with DbParameter objects (CreateParameter) or by using {0},{1} for
        /// positional parameters and passing in the actual values.
        /// 
        /// Uses the Entity Sql Connection
        /// </summary>
        /// <param name="sql">Sql statement as a string</param>
        /// <param name="parameters">Named parameter objects referenced with {0}-{n} in the Sql command</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            int result = -1;
            try
            {
                result = Context.Database.ExecuteSqlCommand(sql, parameters);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return -1;
            }

            return result;
        }

        /// <summary>
        /// Allows execution of a SQL command as s tring agains the Context's
        /// provider and return the result as an Entity collection
        /// 
        /// Format can either be named parameters (@pk, @name)
        /// with DbParameter objects (CreateParameter) or by using {0}, {1} for
        /// positional parameters and passing in the actual values.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql">Sql String. 
        /// </param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TResult> Execute<TResult>(string sql, params object[] parameters)                        
        {
            try
            {                
                return Context.Database.SqlQuery<TResult>(sql, parameters);                
            }
            catch(Exception ex)
            {
                SetError(ex);
                return null;
            }
        }

        /// <summary>
        /// Allows execution of a SQL command as s tring agains the Context's
        /// provider and return the result as an Entity collection
        /// 
        /// Format can either be named parameters (@pk, @name)
        /// with DbParameter objects (CreateParameter) or by using {0}, {1} for
        /// positional parameters and passing in the actual values.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql">Sql String. 
        /// </param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IList<TResult> ExecuteList<TResult>(string sql, params object[] parameters)
        {
            try
            {
                return Context.Database.SqlQuery<TResult>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
        }

        /// <summary>
        /// Creates a new SQL Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DbParameter CreateParameter(string name, object value, 
            ParameterDirection direction, 
            DbType type = DbType.Object)
        {
            var cmd = Context.Database.Connection.CreateCommand();
            var parm = cmd.CreateParameter();

            parm.ParameterName = name;
            parm.Value = value;
            parm.Direction = direction;
            parm.DbType = type;

            return parm;
        }

        /// <summary>
        /// Opens the connection on this business object's Context.
        /// Use this before manually creating Transactions to ensure
        /// transactions execute on a single connection.
        /// </summary>
        public void OpenConnection()
        {
            CloseConnection();       
     
            if (Context.Database.Connection.State != ConnectionState.Open)            
                Context.Database.Connection.Open();
        }
        /// <summary>
        /// explicitly closes a connection
        /// </summary>
        public void CloseConnection()
        {
            if (Context.Database.Connection.State != ConnectionState.Closed)
                Context.Database.Connection.Close();
        }
        /// <summary>
        /// Createst a new Sql Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public DbParameter CreateParameter(string name, object value,
            ParameterDirection direction = ParameterDirection.Input)
        {
            DbParameter parm;
            using (var cmd = Context.Database.Connection.CreateCommand())
            {
                parm = cmd.CreateParameter();            
                parm.ParameterName = name;
                parm.Value = value;
                parm.Direction = direction;                          
            }

            return parm;
        }
        #endregion


        #region Transactions


        /// <summary>
        /// Default TransactionScope options for CreateTransactionScope
        /// </summary>
        protected virtual TransactionOptions TransactionScopeOptions
        {
            get { return new TransactionOptions {IsolationLevel = IsolationLevel.RepeatableRead}; }
        }

        /// <summary>
        /// Allows you to configure how the transaction scope is created internally.
        /// Sets the default isolation level to repetable read
        /// </summary>
        /// <returns>A transaction scope</returns>
        public virtual TransactionScope CreateTransactionScope(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            TransactionOptions options;
            if (isolationLevel == IsolationLevel.Unspecified)
                options = TransactionScopeOptions;
            else
                options = new TransactionOptions { IsolationLevel = isolationLevel };
      
            return  new TransactionScope(TransactionScopeOption.Required,
                                         options);
        }

        #endregion


        #region Overridable Event Hooks

        /// <summary>
        /// Overridable method that allows adding post NewEntity functionaly
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void OnNewEntity(TEntity entity)
        {
        }

        /// <summary>
        /// Fired after an entity has been loaded with the .Load() method
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void OnEntityLoaded(TEntity entity)
        {

        }


        /// <summary>
        /// Method that should be overridden in a business object to handle actual validation. 
        /// This method is called from the Validate method.
        /// 
        /// This method should add any errors to the <see cref="ValidationErrors"/> collection.
        /// </summary>
        /// <param name="entity">The entity to be validated</param>
        protected virtual void OnValidate(TEntity entity)
        {
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
        /// Allows you to capture Save() operation errors
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void OnAfterSaveError(TEntity entity)
        {            
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

        #endregion


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
        private PropertyBag _Properties = null;

        /// <summary>
        /// Retrieves a value from the Properties collection safely.
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

            //string xml = DataContractSerializationUtils.SerializeToXmlString(Properties,true);

            string xml = null;
            if (Properties.Count > 0)
            {
                // Serialize to Xm
                 xml = Properties.ToXml();
            }
            ReflectionUtils.SetProperty(Entity, stringFieldToSaveTo, xml);
        }
        #endregion


        #region Entity Management

        /// <summary>
        /// Makes a separate business object a child business object,
        /// which inherits the DbContext instance of its parent.
        ///
        /// Use this method to make both parent and child business
        /// object to share a single DbContext.
        /// </summary>
        /// <param name="childBusObject"></param>
        protected void SetChildBusinessObject(IBusinessObject<TContext> childBusObject)
        {
            childBusObject.Context = Context;
        }

        /// <summary>
        /// Checks to see if the current entity has been added
        /// to the data context as a new entity
        /// 
        /// This entity specific version is more efficient
        /// than the generic object parameter version.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool IsNewEntity(TEntity entity)
        {
            DbEntityEntry entry = GetEntityEntry(entity);
            if (entry == null)
                throw new ArgumentException(Resources.EntityIsNotPartOfTheContext);
            
            return entry.State == EntityState.Added;
        }

        /// <summary>
        /// Checks to see if the current entity has been added
        /// to the data context as a new entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool IsNewEntity(object entity)
        {
            DbEntityEntry entry = GetEntityEntry(entity);
            if (entry == null)
                throw new ArgumentException(Resources.EntityIsNotPartOfTheContext);

            return entry.State == EntityState.Added;
        }


        /// <summary>
        /// Returns the Entity Entry meta data object that provides
        /// various pieces of info on the entity.
        /// 
        /// Use this specific version if possible for faster retrieval.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected DbEntityEntry GetEntityEntry(TEntity entity)
        {
            var entries =  Context.ChangeTracker
                                  .Entries<TEntity>();
                            
            var res = entries.FirstOrDefault(ent => ent.Entity == entity);

            // REQUIRED: if res is null and returned method fails
            if (res == null)
                return null;

            return res;
        }

        /// <summary>
        /// Returns the Entity Entry meta data object that provides
        /// various pieces of info on the entity
        /// 
        /// Generic version that works with any entity not just those
        /// of the type defined on this business object.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected DbEntityEntry GetEntityEntry(object entity)
        {
            var res = Context.ChangeTracker.Entries()
                          .FirstOrDefault(ent => ent.Entity == entity);

            if (res == null)
                return null;

            return res;
        }
        #endregion

        #region Error Management
        /// <summary>
        /// Sets an internal error message.
        /// </summary>
        /// <param name="message"></param>
        public void SetError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ErrorException = null;
                return;
            }

            // messages are stored on Exception object so exception always
            // exists
            ErrorException = new ApplicationException(message);
        }

        /// <summary>
        /// Sets an internal error exception object. If ThrowExceptions is 
        /// set this method causes an Exception to be fired.
        /// </summary>
        /// <param name="ex"></param>
        public void SetError(Exception ex, bool checkInnerException = false)
        {
            
            if (checkInnerException)
                ErrorException = ex.GetBaseException();
            else
                ErrorException = ex;
            
            // error message is retrieved from exception object
            if (ex != null && ThrowExceptions)
                throw ex;
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
                return Resources.ErrorColon + ErrorMessage;

            return base.ToString();
        }

        #endregion

    }
}