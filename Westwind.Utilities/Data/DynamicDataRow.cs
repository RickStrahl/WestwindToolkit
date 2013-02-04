using System;
using System.Dynamic;
using System.Data;

namespace Westwind.Utilities.Data
{
    /// <summary>
    /// This class provides an easy way to turn a DataRow 
    /// into a Dynamic object that supports direct property
    /// access to the DataRow fields.
    /// 
    /// The class also automatically fixes up DbNull values
    /// (null into .NET and DbNUll to DataRow)
    /// </summary>
    public class DynamicDataRow : DynamicObject
    {
        /// <summary>
        /// Instance of object passed in
        /// </summary>
        DataRow DataRow;
        
        /// <summary>
        /// Pass in a DataRow to work off
        /// </summary>
        /// <param name="instance"></param>
        public DynamicDataRow(DataRow dataRow)
        {
            DataRow = dataRow;
        }

       /// <summary>
       /// Returns a value from a DataRow items array.
       /// If the field doesn't exist null is returned.
       /// DbNull values are turned into .NET nulls.
       /// 
       /// </summary>
       /// <param name="binder"></param>
       /// <param name="result"></param>
       /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            try
            {
                result = DataRow[binder.Name];

                if (result == DBNull.Value)
                    result = null;
                
                return true;
            }
            catch { }

            result = null;
            return false;
        }


        /// <summary>
        /// Property setter implementation tries to retrieve value from instance 
        /// first then into this object
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                if (value == null)
                    value = DBNull.Value;

                DataRow[binder.Name] = value;
                return true;
            }
            catch {}

            return false;
        }
    }
}
