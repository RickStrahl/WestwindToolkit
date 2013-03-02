using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;


namespace Westwind.Web.Controls
{
	/// <summary>
	/// Summary description for wwTextBox.
	/// </summary>
	/// 	
	[ToolboxBitmap(typeof(RadioButtonList)),
	DefaultProperty("SelectedValue"),
	ToolboxData("<{0}:wwRadioButtonList runat='server' size='20'></{0}:wwRadioButtonList>")]
	public class wwRadioButtonList : System.Web.UI.WebControls.RadioButtonList
	{
        public wwRadioButtonList()
        {
            RepeatDirection = RepeatDirection.Horizontal;
        }

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
			string lcValue = Page.Request.Form[UniqueID];
			if (lcValue != null)
				SelectedValue = lcValue;
		}

		
		
	}
}
