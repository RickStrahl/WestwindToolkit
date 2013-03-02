using System;
using System.ComponentModel;
using System.Web.UI;

namespace Westwind.Web.Controls
{
	/// <summary>
	/// This class provides a thin wrapper around the .Net Web Page class by providing
	/// simple interfaces for the databinding features. This includes BindData and UnbindData
	/// methods (as well as auto-hookup for DataBind), support for Validation Error Messages
	/// and methods to provide combination of Business object and Binding errors into a single
	/// collection which can be used for display.
	/// </summary>
	public class wwWebForm : System.Web.UI.Page
	{
		/// <summary>
		/// Assigns focus to the specified control. Note the name must match the exact
		/// ID or container Id of the control in question.
		/// Logic for this behavior is provided in OnPreRender()
		/// </summary>
		[Category("Behavior"),
		Description("Set the focus of this form when it starts to the specified control ID")]
		public Control  FocusedControl 
		{
			get { return oFocusedControl; }
			set { oFocusedControl = value; }
		}
		Control oFocusedControl = null;
    

		/// <summary>
		/// Overriden to handle the FocusedControl property.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			if (FocusedControl != null)
                ClientScriptProxy.Current.RegisterStartupScript(this,GetType(), "FocusedControl", "document.getElementById('" + FocusedControl.ClientID + "').focus();\r\n",true);

			base.OnPreRender (e);            
        }
    }

}
