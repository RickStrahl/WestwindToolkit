using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Transactions;
using Westwind.Utilities;
using IsolationLevel = System.Data.IsolationLevel;

namespace Westwind.Data.EfCodeFirst
{
    /// <summary>
    /// Marker interface for business objects and so we have access to
    /// DbContext instance.
    /// </summary>
    public interface IBusinessObject<TContext>
    {
        TContext Context { get; set; }
    }

 

    /// <summary>
    /// Plain marker interface
    /// </summary>
    public interface IBusinessObject
    { }

    /// <summary>
    /// Marker interface for business objects that provides access
    /// to the DbContext and Entity instances
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public interface IBusinessObject<TEntity, TContext>
        where TEntity : class, new() where TContext : DbContext, new()
    {
        /// <summary>
        /// Instance of the Context that is either auto-created
        /// or shared.
        /// </summary>
        TContext Context { get; set; }

        /// <summary>
        /// A collection that can be used to hold errors or
        /// validation errors. This 
        /// </summary>
        ValidationErrorCollection ValidationErrors { get; }

        /// <summary>
        /// Determines whether or not the Save operation causes automatic
        /// validation
        /// </summary>                        
        bool AutoValidate { get; set; }

        /// <summary>
        /// Internally loaded instance from load and newentity calls
        /// </summary>
        TEntity Entity { get; set; }

        /// <summary>
        /// Error Message of the last exception
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// Instance of an exception object that caused the last error
        /// </summary>
        Exception ErrorException { get; set; }

        bool ThrowExceptions { get; set; }

        /// <summary>
        /// Dictionary of arbitrary property values that can be attached
        /// to the current object. You can use GetProperties, SetProperties
        /// to load the properties to and from a text field.
        /// </summary>
        PropertyBag Properties { get; }

        void Dispose();

        /// <summary>
        /// Creates a new instance of an Entity tracked
        /// by the DbContext.
        /// </summary>
        /// <returns></returns>
        TEntity NewEntity();

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
        TEntity NewEntity(TEntity entity);

        /// <summary>
        /// Loads in instance based on its integer id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity Load(int id);

        /// <summary>
        /// Loads in instance based on its Guid id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity Load(Guid id);

        /// <summary>
        /// Loads in instance based on its string id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity Load(string id);

        /// <summary>
        /// Attaches an untracked entity to an entity set and marks it as modified.
        /// Note: child elements need to be manually added.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entitySet"></param>
        /// <param name="markAsModified"></param>
        /// <param name="addNew"></param>
        /// <returns></returns>
        object Attach(object entity, bool addNew = false, EntityState entityState = EntityState.Modified);

        /// <summary>
        /// Attaches an untracked to the internal context and 
        /// marks it as modified optionally
        /// Note: child elements need to be manually added.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        TEntity Attach(TEntity entity, bool addNew = false);

        /// <summary>
        /// Deletes an entity from the main entity set
        /// based on a key value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="saveChanges">if true changes are saved to disk. Otherwise entity is removed from context only</param>
        /// <returns></returns>
        bool Delete(object id, bool saveChanges = false, bool useTransaction = false);

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
        bool Delete(TEntity entity, DbSet dbSet = null, bool saveChanges = true, bool useTransaction = false);

        /// <summary>
        /// Cancel Changes on the current connected context
        /// </summary>
        void AbortChanges();

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
        bool Validate(TEntity entity = null, bool clearValidationErrors = false);

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
        bool Save(TEntity entity = null, bool useTransactionScope = false);

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
        int ExecuteNonQuery(string sql, params object[] parameters);

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
        IEnumerable<TResult> Execute<TResult>(string sql, params object[] parameters);

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
        IList<TResult> ExecuteList<TResult>(string sql, params object[] parameters);

        /// <summary>
        /// Creates a new SQL Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        DbParameter CreateParameter(string name, object value,
            ParameterDirection direction,
            DbType type = DbType.Object);

        /// <summary>
        /// Opens the connection on this business object's Context.
        /// Use this before manually creating Transactions to ensure
        /// transactions execute on a single connection.
        /// </summary>
        void OpenConnection();

        /// <summary>
        /// explicitly closes a connection
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// Createst a new Sql Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        DbParameter CreateParameter(string name, object value,
            ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Allows you to configure how the transaction scope is created internally.
        /// Sets the default isolation level to repetable read
        /// </summary>
        /// <returns>A transaction scope</returns>
        TransactionScope CreateTransactionScope(System.Transactions.IsolationLevel isolationLevel = System.Transactions.IsolationLevel.Unspecified);

        /// <summary>
        /// Retrieves a value from the Properties collection safely.
        /// If the value doesn't exist null is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetProperty(string key);

        /// <summary>
        /// Sets an internal error message.
        /// </summary>
        /// <param name="message"></param>
        void SetError(string message);

        /// <summary>
        /// Sets an internal error exception object. If ThrowExceptions is 
        /// set this method causes an Exception to be fired.
        /// </summary>
        /// <param name="ex"></param>
        void SetError(Exception ex, bool checkInnerException = false);

        /// <summary>
        /// Clear out errors
        /// </summary>
        void SetError();
    }
}
