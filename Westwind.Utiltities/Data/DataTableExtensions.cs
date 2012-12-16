using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Dynamic;

namespace Westwind.Utilities.Data
{
    /// <summary>
    /// Extends the DataTable to provide access to DynamicDataRow 
    /// data.
    /// </summary>
    public static class DataTableDynamicExtensions
    {
        /// <summary>
        /// Returns a dynamic DataRow instance that can be accessed
        /// with the field name as a property
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>taTab
        public static dynamic DynamicRow(this DataTable dt, int index)
        {
            var row = dt.Rows[index];            
            return new DynamicDataRow(row);
        }

        /// <summary>
        /// Returns a dynamic list of rows so you can reference them with
        /// row.fieldName
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DynamicDataRows DynamicRows(this DataTable dt)
        {
            DynamicDataRows drows = new DynamicDataRows(dt.Rows);
            return drows;
        }

    }

    /// <summary>
    /// Helper class that extends a DataRow collection to 
    /// be exposed as individual <see cref="Westwind.Utilities.Data.DynamicDataRow"/>  objects
    /// </summary>
    public class DynamicDataRows : IEnumerator<DynamicDataRow>, IEnumerable<DynamicDataRow>
    {
        DataRowCollection Rows;
        IEnumerator RowsEnumerator;

        public DynamicDataRow this[int index]
        {
            get
            {
                return new DynamicDataRow(Rows[index]);
            }
        }

        DynamicDataRow IEnumerator<DynamicDataRow>.Current
        {
            get
            {
                return new DynamicDataRow(RowsEnumerator.Current as DataRow);
            }
        }

        public object Current
        {
            get
            {
                return new DynamicDataRow(RowsEnumerator.Current as DataRow);
            }
        }

        public DynamicDataRows(DataRowCollection rows)
        {
            Rows = rows;
            RowsEnumerator = rows.GetEnumerator();
        }

        IEnumerator<DynamicDataRow> IEnumerable<DynamicDataRow>.GetEnumerator()
        {
           foreach (DataRow row in Rows)
            yield return new DynamicDataRow(row);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (DataRow row in Rows)
                yield return new DynamicDataRow(row);
        }
       
        public void Dispose()
        {
            Rows = null;
            RowsEnumerator = null;
        }



        public bool MoveNext()
        {
            return RowsEnumerator.MoveNext();
        }

        public void Reset()
        {
            RowsEnumerator.Reset();
        }

    }
}
