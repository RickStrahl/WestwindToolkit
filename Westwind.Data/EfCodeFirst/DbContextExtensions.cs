using System;
using System.Data.Entity;
using System.Linq;

namespace Westwind.Data.EfCodeFirst
{
    /// <summary>
    /// Extensions to the DbContext class
    /// </summary>
    public static class DbContextUtils
    {
        static object _InitializeLock = new object();
        static bool _InitializeLoaded = false;

        /// <summary>
        /// Method to allow running a DatabaseInitializer exactly once
        /// </summary>   
        /// <param name="initializer">A Database Initializer to run</param>
        public static void SetInitializer<TContext>(IDatabaseInitializer<TContext> initializer = null)
            where TContext : DbContext
        {            
            if (_InitializeLoaded)
                return;

            // watch race condition
            lock (_InitializeLock)
            {
                // are we sure?
                if (_InitializeLoaded)                
                    return;

                _InitializeLoaded = true;

                // force Initializer to load only once
                System.Data.Entity.Database.SetInitializer<TContext>(initializer);
            }
        }
    }
}
