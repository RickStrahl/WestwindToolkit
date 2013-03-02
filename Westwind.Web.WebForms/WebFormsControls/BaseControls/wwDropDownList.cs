using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Reflection;
using System.Web;

namespace Westwind.Web.Controls
{

	/// <summary>
	/// This control provides two way databinding, binding validation and limited formatting to the DropDownList control,
	/// but does this only for the controls' value member (simple databinding). List based binding still works
	/// as you would expect.
	/// 
	/// Note that this control works without ViewState to assign values back. It does so after the OnLoad()
	/// event call on post backs by retrieving the previously posted value.
	/// </summary>
	/// 	
	[ToolboxBitmap(typeof(DropDownList)),
	DefaultProperty("SelectedValue"),
	Description("Customized drop downlist whose Selected value property can be bound to a data member in addition to binding the list it displays."),
	ToolboxData("<{0}:wwDropDownList runat='server' width='200'></{0}:wwDropDownList>")]
	public class wwDropDownList : System.Web.UI.WebControls.DropDownList
	{

        /// <summary>
        /// Override to register ControlState
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {            
            base.OnInit(e);
            //Page.RegisterRequiresControlState(this);
        }

        /// <summary>
        /// Save SelectedValue into Control State
        /// </summary>
        /// <returns></returns>
        protected override object SaveControlState()
        {
            return SelectedValue;
        }

	
		/// <summary>
		/// Retrieves the real selected value of the control. Used internally to read the 
		/// selected value OnInit as well as being available for reading the value from 
		/// the Forms collection more easily and storing it into the SelectedValue property.
		/// </summary>
		public void GetSelectedValue() 
		{
			string Value = Page.Request.Form[UniqueID];
			if (Value != null)
				SelectedValue = Value;
		}     
	}

}
