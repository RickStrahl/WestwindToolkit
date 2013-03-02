// Flag determines whether the West Wind Business objects are supported here
// when enabled Business object errors can be added to the ValidationErrors
// collection of the form.
// If this #define is removed the dependence on wwbusiness.dll goes away.
//#define IncludeWestWindBusinessObjects

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using System.IO;
using System.Reflection;



namespace Westwind.Web.Controls
{
	/// <summary>
	/// This class provides a thin wrapper around the .Net Web Page class by providing
	/// simple interfaces for the databinding features. This includes BindData and UnbindData
	/// methods (as well as auto-hookup for DataBind), support for Validation Error Messages
	/// and methods to provide combination of Business object and Binding errors into a single
	/// collection which can be used for display.
	/// </summary>
	public class wwPage : System.Web.UI.Page
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
                ClientScript.RegisterStartupScript(GetType(), "FocusedControl", "document.getElementById('" + FocusedControl.ClientID + "').focus();\r\n",true);

			base.OnPreRender (e);            
        }


    }

}
