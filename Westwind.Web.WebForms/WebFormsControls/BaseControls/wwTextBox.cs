using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Reflection;	

namespace Westwind.Web.Controls
{

	/// <summary>
	/// Textbox override that handles posting back of passwords.
	/// </summary>	
	[ToolboxBitmap(typeof(TextBox)),DefaultProperty("Text"),
	ToolboxData("<{0}:wwTextBox runat='server' width='400px' height='22px'></{0}:wwTextBox>")]
	public class wwTextBox : System.Web.UI.WebControls.TextBox
	{
		/// <summary>
		/// Overriden to handle displaying password characters from
		/// preloaded data (ASP.Net doesn't display Text in passwords)
		/// </summary>
		/// <param name="e"></param>
		override protected void OnLoad(EventArgs e) 
		{
			base.OnLoad(e);

			// Post back password values as well - you can always clear it manually
			if (TextMode ==  TextBoxMode.Password)
				Attributes.Add("value", Text);
		}
	}
}
