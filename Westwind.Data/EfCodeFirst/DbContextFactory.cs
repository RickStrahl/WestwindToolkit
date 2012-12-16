using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Web;
using System.Threading;

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
        public static TDbContext GetWebRequestOrThreadScopedDataContext<TDbContext>(string connectionStringId = null, string key = null)
                where TDbContext : DbContext, new()
        {
            if (HttpContext.Current != null)
                // Create a request specific unique key 
                return (TDbContext)GetWebRequestScopedDbContextInternal(typeof(TDbContext), key, connectionStringId);
            else
                return (TDbContext)GetThreadScopedDbContextInternal(typeof(TDbContext), key, connectionStringId);
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
        public static TDbContext GetWebRequestScopedDbContext<TDbContext>(string connectionStringId = null, string key = null)
                where TDbContext : DbContext, new()
        {
            // Create a request specific unique key 
            return (TDbContext)GetWebRequestScopedDbContextInternal(typeof(TDbContext), key, connectionStringId);
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
        public static TDbContext GetThreadScopedDbContext<TDbContext>(string connectionStringId = null, string key = null)
                                   where TDbContext : DbContext, new()
        {
            return (TDbContext)GetThreadScopedDbContextInternal(typeof(TDbContext), key, connectionStringId);
        }


        /// <summary>
        /// Internal method that handles creating a context that is scoped to the HttpContext Items collection
        /// by creating and holding the DataContext there.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        static object GetWebRequestScopedDbContextInternal(Type type, string key, string connectionStringId)
        {
            object context;            

            // if HttpContext is not available - load new instance
            if (HttpContext.Current == null)
            {
                if (connectionStringId == null)
                    context = Activator.CreateInstance(type);
                else
                    context = Activator.CreateInstance(type, connectionStringId);

                return context;
            }
                
            // Create a unique Key for the Web Request/Context 
            if (key == null)
                key = "__WRSCDC_" + HttpContext.Current.GetHashCode().ToString("x") + Thread.CurrentContext.ContextID.ToString();

            context = HttpContext.Current.Items[key];

            if (context == null)
            {                
                if (connectionStringId == null)
                    context = Activator.CreateInstance(type);
                else
                    context = Activator.CreateInstance(type, connectionStringId);

                if (context != null)
                    HttpContext.Current.Items[key] = context;
            }

            return context;
        }

        /// <summary>
        /// Creates a Thread Scoped DataContext object that can be reused.
        /// The DataContext is stored in Thread local storage.
        /// </summary>
        /// <typeparam name="TDataContext"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        static object GetThreadScopedDbContextInternal(Type type, string key, string connectionStringId)
        {
            if (key == null)
                key = "__WRSCDC_" + Thread.CurrentContext.ContextID.ToString();

            LocalDataStoreSlot threadData = Thread.GetNamedDataSlot(key);
            object context = null;
            if (threadData != null)
                context = Thread.GetData(threadData);

            if (context == null)
            {
                if (connectionStringId == null)
                    context = Activator.CreateInstance(type);
                else
                    context = Activator.CreateInstance(type, connectionStringId);

                if (context != null)
                {
                    if (threadData == null)
                        threadData = Thread.AllocateNamedDataSlot(key);

                    Thread.SetData(threadData, context);
                }
            }

            return context;
        }
    }

}
