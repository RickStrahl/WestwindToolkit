using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;

namespace Westwind.Utilities
{
    /// <summary>
    /// An object factory that can create instances of types
    /// for Http Web Request and Thread Scoped object objects
    /// and value types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectFactory<T>
        where T: class, new()
    {
        /// <summary>
        /// Internal locking for collection storage
        /// </summary>
        static object _syncLock = new object();



        /// <summary>
        /// Returns a standard instance of an object.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T CreateObject(params object[] args)
        {
            return (T) Activator.CreateInstance(typeof(T), args);            
        }

        

        /// <summary>
        /// Creates an instance of an object that is scoped to the
        /// current Http Web Request. The instance
        /// is cached in HttpContext.Current.Items collection and effectively
        /// acts as a request Singleton.
        /// </summary>
        /// <param name="args"></param>        
        /// <returns></returns>
        public static T CreateWebRequestScopedObject(params object[] args)
        {            
            // if HttpContext is not available - load new 
            // non request instance and return
            if (HttpContext.Current == null)                             
                return CreateObject(args);
                        
            // Create a unique Key for the Web Request/Context
            // based on: Type name, Current Context and Thread
            string key = GetUniqueObjectKey(args) +
                        HttpContext.Current.GetHashCode().ToString("x");
            
            object item = HttpContext.Current.Items[key];

            if (item == null)
            {
                // avoid multiple adds in high load
                lock (_syncLock)
                {
                    // recheck the value
                    item = HttpContext.Current.Items[key];
                    if (item == null)
                    {
                        item = CreateObject(args);
                        HttpContext.Current.Items[key] = item;
                    }
                }
            }
            
            return (T) item;
        }
        
        /// <summary>
        /// Create an instance scoped to a current thread.
        /// </summary>
        /// <param name="key">Optional reusable key of the TLS item created</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T CreateThreadScopedObject(params object[] args)
        {
            string key = GetUniqueObjectKey(args) + "_" +
                                            Thread.CurrentContext.ContextID.ToString();
              

            LocalDataStoreSlot threadData = Thread.GetNamedDataSlot(key);

            T item;

            if (threadData != null)
                item = (T)Thread.GetData(threadData);
            else
                item = null;

            // no item - create and store
            if ( item == null )
            {
                lock (_syncLock)
                {
                    // check again inside of lock
                    threadData = Thread.GetNamedDataSlot(key);
                    if (threadData == null)
                        threadData = Thread.GetNamedDataSlot(key);

                    if (item == null)
                        item = CreateObject(args);

                    Thread.SetData(threadData, item);                        
                }                
            }

            return item;
        }

        /// <summary>
        /// Creates either Web Request scoped object instance or value
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T CreateWebRequestOrThreadScopedObject(params object[] args)               
        {
            if (HttpContext.Current != null)
                // Create a request specific unique key 
                return CreateWebRequestScopedObject(args);
            else
                return CreateThreadScopedObject(args);
        }

        ///// <summary>
        ///// Creates either Web Request scoped object instance or value
        ///// </summary>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //public static T CreateWebRequestOrThreadScopedObject(string key, params object[] args)
        //{
        //    if (HttpContext.Current != null)
        //        // Create a request specific unique key 
        //        return CreateWebRequestScopedObject(key, args);
        //    else
        //        return CreateThreadScopedObject(key, args);
        //}


        /// <summary>
        /// Returns a unique ID for a given type and parameter signature
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetUniqueObjectKey(params object[] args)
        {
            Type type = typeof(T);

            StringBuilder sb = new StringBuilder("_" + type.GetHashCode().ToString("x"));
            sb.Append( "_" + type.Name);            

            foreach (var arg in args)
            {
                if (arg == null)
                    sb.Append("_null");
                else
                    sb.Append("_" + arg.GetHashCode().ToString("x"));
            }

            return sb.ToString();
        }
  
        
    }
}
