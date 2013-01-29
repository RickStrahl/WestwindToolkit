using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Utilities.Test
{
    public class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            // create database if it doesn't exist
            Database.SetInitializer<WebStoreContext>(new WebStoreContextInitializer());

            // force connection to be fired once
            var customer = new WebStoreContext().Customers.FirstOrDefault();
        }
    }
}
