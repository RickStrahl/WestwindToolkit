using System;
using System.Collections.Generic;
using System.Text;

namespace Westwind.Web.Controls
{
    /// <summary>
    /// Extender style interface that allows adding a DataBinder 
    /// object to a control and interact with a DataBinder object
    /// on a Page. 
    /// 
    /// Any control marked with this interface can be automatically
    /// pulled into the a DataBinder instance with 
    /// DataBinder.LoadFromControls().
    /// </summary>
    public interface IDataBinder
    {
        DataBindingItem BindingItem
        {
            get;
        }
    }
}
