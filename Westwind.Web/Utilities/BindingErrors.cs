using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Westwind.Web.Controls
{
    /// <summary>
    /// This class provides a holding container for BindingErrors. BindingErrors 
    /// occur during binding and unbinding of controls and any errors are stored in
    ///  this collection. This class is used extensively for checking for 
    /// validation errors and then displaying them with the ToString() or ToHtml() 
    /// methods.
    /// </summary>
	public class BindingErrors : List<BindingError>
	{
        /// <summary>
        /// Formats all the BindingErrors into a rich list of error messages. The error
        ///  messages are marked up with links to the appropriate controls. Format of 
        /// the list is a &lt;ul&gt; style list ready to display in an HTML page.
        /// <seealso>Class BindingErrors</seealso>
        /// </summary>
        /// <returns>an Html string of the errors</returns>
         public string ToHtml()
         {
               if (Count < 1)
		         return "";

	         StringBuilder sb = new StringBuilder("");
	         sb.Append("\r\n<ul>");
             foreach (var error in this) 
	         {	
		         sb.Append("<li style='margin-left:0px;'>");
                 if (error.ClientID != null && error.ClientID != "")
                    sb.Append(LinkedErrorMessage(error.ClientID,error.Message));

                 else
                     sb.Append(error.Message + "\r\n");
	         }
	         sb.Append("</ul>");
	         return sb.ToString();
         }


         /// <summary>
         /// Renders a link with the error message that attempts to find the control on the 
         /// page and highlights it.
         /// </summary>
         /// <param name="clientId"></param>
         /// <param name="?"></param>
         /// <returns></returns>
         private string LinkedErrorMessage(string clientId, string message)
         {
            string  link = "<a href='javascript:{}' onclick=\"var T = document.getElementById('" + clientId + "'); if(T == null) { return }; T.style.borderWidth='2px';T.style.borderColor='Red';try {T.focus();} catch(e) {}; " +
                               @"if (window.onBindingErrorLink) onBindingErrorLink(T); " +
                               @"window.setTimeout( function() { T=document.getElementById('" + clientId + @"'); " +                               
                               @"T.style.borderWidth='';T.style.borderColor=''},4000);" + 
                               "\">" + message + "</a></li>\r\n";
             return link;
          
         }


		/// <summary>
		/// Formats an Binding Errors collection as a string with carriage returns
		/// </summary>
		/// <param name="Errors"></param>
		/// <returns></returns>
		public override string ToString() 
		{
			// Optional Error Parsing
			if (Count > 0) 
			{
				StringBuilder sb = new StringBuilder("");
				foreach (BindingError Error in this) 
				{	
					sb.Append(Error.Message + "\r\n");
				}
				return sb.ToString();
			}

			return "";
		}
	}


	/// <summary>
	/// Error object used to return error information during databinding.
	/// </summary>
	public class BindingError 
	{
        /// <summary>
        /// The ClientID of the control the error occurred on. This value is used to 
        /// provide the hot linking to the control.
        /// <seealso>Class BindingError</seealso>
        /// </summary>
		public string ClientID
		{
            get { return _ClientID; }
			set { _ClientID = value; }
		}
		string _ClientID = "";

        /// <summary>
        /// The error message that is displayed for the Binding error.
        /// <seealso>Class BindingError</seealso>
        /// </summary>
        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
        string _Message = "";

        /// <summary>
        /// The raw Exception error message. Not used at the moment.
        /// <seealso>Class BindingError</seealso>
        /// </summary>
        public string ErrorMessage
        {
            get { return _cErrorMessage; }
            set { _cErrorMessage = value; }
        }
        string _cErrorMessage;

      

		public BindingError()
		{
		}
		public BindingError(string errorMessage) 
		{
			Message = errorMessage;
		}
		public BindingError(string errorMessage, string clientId) 
		{
			Message = errorMessage;
			//Id = "txt" + CliendID;

            if (clientId == null)
                clientId = "";
			
            this.ClientID = clientId;
		}
	}


     
}
