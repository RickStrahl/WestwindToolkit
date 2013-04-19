using System;
using System.Data.Entity;
using Westwind.Utilities;

namespace Westwind.Data.EfCodeFirst
{
   /// <summary>
   /// Class that creates dbContext instances and scopes them
   /// either to a thread or a Web context for efficient reuse.
   /// </summary>
    public class DbContextFactory
    {
        static object WebRequestLock = new object();


        /// <summary>
        /// Creates a new DbContext for a specific DbContext type
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <returns></returns>
        public static TDbContext GetDbContext<TDbContext>()
                where TDbContext : DbContext, new()
        {
            return new TDbContext(); 
        }

        /// <summary>
        /// Creates a new DbContext for a specific DbContext type with an explicit
        /// connection string id
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <returns></returns>
        public static TDbContext GetDbContext<TDbContext>(string connectionStringId)
                where TDbContext : DbContext, new()
        {       
            return   Activator.CreateInstance(typeof(TDbContext), connectionStringId) as TDbContext;
        }


        /// <summary>
        /// Retrieves a Web Request DbContext if available. If not available will use a thread scoped DbContext instead
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="connectionStringId">Optional connection string ID from .config file</param>
        /// <param name="key">optional key name of the cached item</param>
        /// <returns></returns>
        public static TDbContext GetWebRequestOrThreadScopedDataContext<TDbContext>(string connectionStringId = null)
                where TDbContext : DbContext, new()
        {
            return ObjectFactory<TDbContext>.CreateWebRequestOrThreadScopedObject(connectionStringId);
        }


        /// <summary>
        /// Creates a ASP.NET Context scoped instance of a DbContext. This static
        /// method creates a single instance and reuses it whenever this method is
        /// called.
        /// 
        /// This version creates an internal request specific key shared key that is
        /// shared by each caller of this method from the current Web request.
        /// </summary>
        /// <param name="connectionStringId">optional connectionstring id to load context with</param>
        /// <param name="key">Optional name of the key to store</param>
        public static TDbContext GetWebRequestScopedDbContext<TDbContext>(string connectionStringId = null)
                where TDbContext : DbContext, new()
        {
            return ObjectFactory<TDbContext>.CreateWebRequestScopedObject(connectionStringId);         
        }


        /// <summary>
        /// Creates a Thread Scoped DataContext object that can be reused.
        /// The DataContext is stored in Thread local storage.
        /// </summary>
        /// <typeparam name="TDataContext"></typeparam>
        /// <param name="connectionStringId">optional connection string ID from config file</param>
        /// <param name="key">optional key name of the context to cache</param>     
        /// <typeparam name="TDbContext">DbContext Parameter</typeparam>
        /// <returns></returns>
        public static TDbContext GetThreadScopedDbContext<TDbContext>(string connectionStringId = null)
                                   where TDbContext : DbContext, new()
        {
            return ObjectFactory<TDbContext>.CreateThreadScopedObject(connectionStringId);            
        }

    }

}
