using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Data.Objects;
using System.Data.EntityClient;
using System.Data.Entity;
using Westwind.Utilities.Data;

namespace Westwind.Data.EfCodeFirst
{
    /// <summary>
    /// Customization of the LINQ to SQL DataContext class that provides
    /// core ADO.NET Data Access methods to the data context via a Db 
    /// property.
    /// </summary>
    public class EfCodeFirstContext : DbContext
    {
        public DataAccessBase Db
        {
            get
            {
                if (_DbNative == null)
                    _DbNative = new SqlDataAccess(Database.Connection.ConnectionString);
                return _DbNative;
            }
            set
            {
                _DbNative = value;
            }
        }
        private DataAccessBase _DbNative;

        /// <summary>
        /// Custom constructor that allows passing in of a custom IDbNative context
        /// to provide SQL interactivity.
        /// </summary>
        /// <param name="dbNative"></param>
        public EfCodeFirstContext(DataAccessBase dbNative)
            : base()
        {
            this.Db = dbNative;
        }

        public EfCodeFirstContext()
        {
        }
    }    
}