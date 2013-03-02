using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Westwind.Utilities;

namespace Westwind.Web.Controls
{
    
    /// <summary>
    /// Collection of individual DataBindingItems. Implemented explicitly as
    /// a CollectionBase class rather than using List#DataBindingItems#
    /// so that Add can be overridden
    /// </summary>
    public class DataBindingItemCollection : CollectionBase
    {
        /// <summary>
        /// Internal reference to the DataBinder object
        /// that is passed to the individual items if available
        /// </summary>
        DataBinder _ParentDataBinder = null;

        /// <summary>
        /// Preferred Constructor - Add a reference to the DataBinder object here
        /// so a reference can be passed to the children.
        /// </summary>
        /// <param name="Parent"></param>
        public DataBindingItemCollection(DataBinder Parent)
        {
            _ParentDataBinder = Parent;
        }

        /// <summary>
        /// Not the preferred constructor - If possible pass a reference to the
        /// Binder object in the overloaded version.
        /// </summary>
        public DataBindingItemCollection()
        {
        }

        /// <summary>
        /// Public indexer for the Items
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataBindingItem this[int index]
        {
            get
            {
                return InnerList[index] as DataBindingItem;
            }
            set
            {
                InnerList[index] = value;
            }
        }


        /// <summary>
        /// Add a DataBindingItem to the collection
        /// </summary>
        /// <param name="Item"></param>
        public void Add(DataBindingItem Item)
        {
           if (_ParentDataBinder != null)
            {
                Item.Page = _ParentDataBinder.Page;
                Item.Binder = _ParentDataBinder;

                // VS Designer adds new items as soon as their accessed
                // but items may not be valid so we have to clean up
                if (_ParentDataBinder.DesignMode)
                {
                   // Remove any blank items
                   UpdateListInDesignMode();
                }
            }

            InnerList.Add(Item);
        }


        /// <summary>
        /// Add a DataBindingItem to the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Item"></param>
        public void AddAt(int index, DataBindingItem Item)
        {
            if (_ParentDataBinder != null)
            {
                Item.Page = _ParentDataBinder.Page;
                Item.Binder = _ParentDataBinder;

               // VS Designer adds new items as soon as their accessed
                // but items may not be valid so we have to clean up
                if (_ParentDataBinder.DesignMode)
                {
                   UpdateListInDesignMode();
                }
            }

            InnerList.Insert(index, Item);
        }

        /// <summary>
        /// We have to delete 'empty' items because the designer requires items to be 
        /// added to the collection just for editing. This way we may have one 'extra'
        /// item, but not a whole long list of items.
        /// </summary>
        private void UpdateListInDesignMode()
        {
            if (_ParentDataBinder == null)
                return;

            bool Update = false;

            // Remove empty items - so the designer doesn't create excessive empties
            for (int x = 0; x < Count; x++)
            {
                if (string.IsNullOrEmpty(this[x].BindingSource) && string.IsNullOrEmpty(this[x].BindingSourceMember))
                {
                    RemoveAt(x);
                    Update = true;
                }
            }

            if (Update)
                _ParentDataBinder.NotifyDesigner();
        }

    }
}