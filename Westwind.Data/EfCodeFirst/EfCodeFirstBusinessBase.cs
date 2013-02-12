using System;
using System.Data.Entity;
using System.Xml.Serialization;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using Westwind.Data.Properties;
using System.Linq;
using System.Data.Objects;
using Westwind.Utilities;
using System.Linq.Expressions;
using System.Transactions;
using System.ComponentModel.DataAnnotations.Schema;
using Westwind.Utilities.Data;

namespace Westwind.Data.EfCodeFirst
{
    
    /// <summary>
    /// Business object base class that acts as a container for a base entity
    /// type and a DbContext instance. Subclasses of this business object
    /// should be used to implement most data related logic that deals with
    /// creating, updating, removing and querying of data use EF Code First.
    /// 
    /// The business object provides base CRUD methods like Load, NewEntity,
    /// Remove. The Save() method uses the EF specific context based SaveChanges
    /// which saves all pending changes (not just those for the current entity 
    /// and its relations). As such these business objects should be used as
    /// atomically as possible and call Save() as often as possible to change
    /// pending data.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public class EfCodeFirstBusinessBase<TEntity, TContext> : IDisposable, IBusinessObject<TContext>
        where TEntity : class, new()
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
                if (_DbSet == null)
                    _DbSet = Context.Set<TEntity>();
                return _DbSet; 
            }            
        }
        private DbSet<TEntity> _DbSet = null;

        /// <summary>
        /// Get an instance of the underlying object context
        /// </summary>
        protected ObjectContext ObjectContext 
        {
            get
            {
                if (_ObjectContext == null)
                    _ObjectContext = ((IObjectContextAdapter)Context).ObjectContext;
            	return _ObjectContext;
            }
        }
        ObjectContext _ObjectContext = null;

        /// <summary>
        /// A collection that can be used to hold errors. This collection
        /// is set by the AddValidationError method.
        /// </summary>
        [XmlIgnore]
        [NotMapped]
        public ValidationErrorCollection ValidationErrors
        {
            get
            {
                if (_ValidationErrors == null)
                    _ValidationErrors = new ValidationErrorCollection();
                return _ValidationErrors;
            }
        }
        ValidationErrorCollection _ValidationErrors;

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
        /// </summary>            img
        [NotMapped]
        [XmlIgnore]
        public Exception ErrorException
        {
            get { return _ErrorException; }
            set { _ErrorException = value; }
        }
        [NonSerialized]
        private Exception _ErrorException = null;


        #region ObjectInitializers

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
            Context = parentBusinessObject.Context as TContext;
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
        protected virtual TContext CreateContext(string connectionString)
        {            
            TContext context = ReflectionUtils.CreateInstanceFromType(typeof(TContext), connectionString) as TContext;

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

        #endregion


        /// <summary>
        /// Creates a new instance of an Entity tracked
        /// by the DbContext.
        /// </summary>
        /// <returns></returns>
        public virtual TEntity NewEntity()
        {            
            Entity = Context.Set<TEntity>().Add(new TEntity()) as TEntity;

            OnNewEntity(Entity);

            if (Entity == null)
                return null;

            return Entity;
        }

        /// <summary>
        /// Adds a new entity as if it was created and fires
        /// the OnNewEntity internally. 
        /// 
        /// This allows for external creation of the entity
        /// and then adding the entity to the context after
        /// the fact.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual TEntity NewEntity(TEntity entity)
        {
            Entity = Context.Set<TEntity>().Add(entity) as TEntity;

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
            object match = DbSet.Find(new object[] { id });
            if (match == null)
            {
                SetError(Westwind.Data.Properties.Resources.UnableToFindMatchingEntityForKey);
                return null;
            }                       

            // Assign to internal member
            Entity = match as TEntity;            

            OnEntityLoaded(Entity);

            return Entity as TEntity;
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
                TContext context = Context;
                
                var dbSet = Context.Set<TEntity>();
                if (dbSet == null)                                   
                    return null;
                
                //var res = dbSet.Where(whereClauseLambda);
                Entity = dbSet.Where(whereClauseLambda).FirstOrDefault();

                if (Entity != null)
                    OnEntityLoaded(Entity);

                return Entity;
            }
            catch (InvalidOperationException)
            {
                // Handles errors where an invalid Id was passed, but SQL is valid
                SetError(Westwind.Data.Properties.Resources.CouldnTLoadEntityInvalidKeyProvided);            
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
                GetEntityEntry(Entity).State = System.Data.EntityState.Modified;
            }

            return Entity;
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
        public object Attach(object entity, bool addNew = false, System.Data.EntityState entityState = System.Data.EntityState.Modified )
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
        /// <param name="noTransaction">Optional - 
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

            if (useTransaction)
            {
                using (var trans = new TransactionScope())
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
        /// Deletes an entity from the main entity set
        /// based on a key value.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  bool Delete(object id) 
        {
            TEntity entity = DbSet.Find(id);
            return Delete(entity);
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
        public bool Save(TEntity entity = null)
        {
            if (entity == null)
                entity = Entity;

            using (var transaction = new TransactionScope() )
            {
                // hook point - allow logic to abort saving
                if (!OnBeforeSave(entity))
                    return false;
            
                // now do validations
                if (AutoValidate)
                {
                    if (!Validate(entity))
                        return false;
                }

                int affected = 0;
                try
                {
                    affected = Context.SaveChanges();                
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
                    return false;
                }
                catch (DbUpdateException ex)
                {
                    SetError(ex,true);
                    return false;
                }            
                catch (Exception ex)
                {
                    SetError(ex,true);
                    return false;
                }

                if (!OnAfterSave(Entity))
                    return false;

                transaction.Complete();
            }            

            return true;
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
        /// Generally this method consists of a bunch of if statements that validate 
        /// the data of the business object and adds any errors to the 
        /// <see>wwBusiness.ValidationErrors</see> collection.
        /// 
        /// If the <see>wwBusiness.AutoValidate</see> flag is set to true causes Save()
        ///  to automatically call this method. Must be overridden to perform any 
        /// validation.
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
        /// Method that should be overridden in a business object to handle actual validation. 
        /// This method is called from the Validate method.
        /// 
        /// This method should add any errors to the <see cref="ValidationErrors"/> collection.
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
        private PropertyBag _Properties = null;

        /// <summary>
        /// Loads the Properties dictionary with values from a Properties property of 
        /// an entity object. Once loaded you can access the dictionary to read and write
        /// values from it arbitrarily and use SetProperties to write the values back
        /// in serialized form to the underlying property for database storage.
        /// </summary>
        /// <param name="stringFieldNameToLoadFrom"></param>
        protected bool GetProperties(string stringFieldNameToLoadFrom = "Properties", object entity = null)
        {
            Properties = null;

            if (entity == null)
                entity = this.Entity;

            string fieldValue = ReflectionUtils.GetProperty(entity, stringFieldNameToLoadFrom) as string;
            if (string.IsNullOrEmpty(fieldValue))
                return false;

            // load up Properties from XML
            Properties = new PropertyBag();
            Properties.FromXml(fieldValue);

            //DataContractSerializationUtils.DeserializeXmlString(fieldValue,typeof(Dictionary<string,object>),true) as Dictionary<string,object>;
            return true;
        }

        /// <summary>
        /// Saves the Properties Dictionary - in serialized string form - to a specified entity field which 
        /// in turn allows writing the data back to the database.
        /// </summary>
        /// <param name="stringFieldToSaveTo"></param>
        protected void SetProperties(string stringFieldToSaveTo = "Properties", object entity = null)
        {
            if (entity == null)
                entity = this.Entity;

            //string xml = DataContractSerializationUtils.SerializeToXmlString(Properties,true);

            // Serialize to Xml
            string xml = Properties.ToXml();
            ReflectionUtils.SetProperty(Entity, stringFieldToSaveTo, xml);
        }
        #endregion

        /// <summary>
        /// Passes the DbContext from the current business object to the 
        /// a child business object so all operations are running
        /// in the same context. This allows sharing of Business
        /// object logic in the same context
        /// </summary>
        /// <param name="childBusObject"></param>
        protected void SetChildBusinessObject(EfCodeFirstBusinessBase<TEntity, TContext> childBusObject)
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
            
            return entry.State == System.Data.EntityState.Added;
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

            return entry.State == System.Data.EntityState.Added;
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
            return Context.ChangeTracker.Entries<TEntity>()
                            .Where(ent => ent.Entity == entity).FirstOrDefault();
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
            return Context.ChangeTracker.Entries()
                            .Where(ent => ent.Entity == entity).FirstOrDefault();
        }


        /// <summary>
        /// Allows execution of an arbitrary non query SQL command against
        /// the database.
        /// 
        /// Format can either be named parameters (@pk, @name)
        /// with DbParameter objects (CreateParameter) or by using @0,@1 for
        /// positional parameters and passing in the actual values.
        /// 
        /// Uses the Entity Sql Connection
        /// </summary>
        /// <param name="sql">Sql statement as a string</param>
        /// <param name="parameters">Named parameter objects</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            int result = -1;
            try
            {
                result = this.Context.Database.ExecuteSqlCommand(sql, parameters);
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
        /// with DbParameter objects (CreateParameter) or by using @p0,p1 for
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
            this.CloseConnection();       
     
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
            var cmd = Context.Database.Connection.CreateCommand();
            
            var parm = cmd.CreateParameter();            
            parm.ParameterName = name;
            parm.Value = value;
            parm.Direction = direction;
            
            cmd.Dispose();

            return parm;
        }


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
            if (!string.IsNullOrEmpty(this.ErrorMessage))
                return Resources.ErrorColon + ErrorMessage;

            return base.ToString();
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
    }
}
