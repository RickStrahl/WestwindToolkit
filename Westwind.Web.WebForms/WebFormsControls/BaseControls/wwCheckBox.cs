using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.ComponentModel;
using System.Reflection;

namespace Westwind.Web.Controls
{
	/// <summary>
	/// Checkbox override to allow for numeric checkbox value with 0 = false and everyting else denoting true
	/// </summary>
	[ToolboxBitmap(typeof(CheckBox)),
	DefaultProperty("Checked"),
	ToolboxData("<{0}:wwCheckBox runat='server' size='30'></{0}:wwCheckBox>")]
	public class wwCheckBox : System.Web.UI.WebControls.CheckBox
	{

		/// <summary>
		/// Property that can be used to bind the checkbox to an
		/// integer value. Some database don't have bit or bool types
		/// and they can use this int field instead to bind to.
		/// </summary>
		[Category("Databinding"),
		DefaultValue(0)]
		public int CheckedInt 
		{
			get 
			{
				if (Checked)
					return 1;
				return 0;
			}
			set 
			{
				if (value == 0) 
					Checked = false;
				else
					Checked = true;
			}
		}

	}
}
