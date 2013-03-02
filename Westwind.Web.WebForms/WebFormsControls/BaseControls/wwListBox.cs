using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace Westwind.Web.Controls
{
	/// <summary>
	/// This control provides two way databinding, binding validation and limited formatting to the Listbox control,
	/// but does this only for the controls' value member (simple databinding). List based binding still works
	/// as you would expect.
	/// </summary>
	[ToolboxBitmap(typeof(ListBox)),
	DefaultProperty("SelectedValue"),
	ToolboxData("<{0}:wwListBox runat='server' width='200'></{0}:wwListBox>")]
	public class wwListBox : System.Web.UI.WebControls.ListBox
	{
        
        /// <summary>
        /// Override to register ControlState
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.RegisterRequiresControlState(this);
        }

        /// <summary>
        /// Retrieve SelectedValue from ControlState
        /// </summary>
        /// <param name="savedState"></param>
        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
                SelectedValue = savedState as string; 
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
			string lcValue = Page.Request.Form[ID];
			if (lcValue != null)
				SelectedValue = lcValue;
		}
		
	
	
	
	}
}
