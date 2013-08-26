//#define ERRORLOGGING   - old error logging code removed

using System;
using System.Web;
using System.Text;
using System.IO;
using System.Diagnostics;
using Westwind.Utilities.Logging;
using Westwind.Web.Properties;


namespace Westwind.Web
{
	/// <summary>
	/// Class that can be used to parse ASP.NET and exception error information
	/// into an object. This class is ideal for capturing information for logging,
	/// notification and display of detailed error information.
	/// 
	/// Also supports complete Application_Error handling with the HandleError
	/// method which works off an exception and provides a complete handler for
	/// processing error information with hooks for logging and displaying
	/// of an error page. 
	/// </summary>
	public class WebErrorHandler
	{		
        /// <summary>
        /// The last exception that the error parsing uses
        /// for creating the error properties when calling
        /// Parse().
        /// </summary>
        public Exception LastException { get; set;  }

		protected object WriteLock = new object();
		protected bool IsParsed = false;
		
		/// <summary>
		/// If true returns only the error message and URL.
		/// </summary>
        public bool CompactFormat { get; set; }

		/// <summary>
		/// Determines whether the routines attempt to retrieve Source Code lines.
		/// </summary>
        public bool RetrieveSourceLines { get; set;  }


		/// <summary>
		/// The runtime error message thrown by the application.
		/// </summary>
        public string ErrorMessage { get; set; }

		/// <summary>
		/// The raw Web relative URL that caused this exception to occur. 
		/// Example: /WebStore/Item.aspx?sku=WWHELP
		/// </summary>
		public string RawUrl  {get; set;}

		/// <summary>
		/// The completely qualified Url to this request.
		/// Example: http://www.west-wind.com/webstore/item.aspx?sku=WWSTOREDOTNET
		/// </summary>
		public string FullUrl {get; set; }

		/// <summary>
		/// Stack trace listing as available from the runtime compiler
		/// </summary>
        public string StackTrace { get; set; }

		/// <summary>
		/// The source code if available for the error that occurred. The code will include the 5 surrounding lines before and after.
		/// 
		/// Source code is available only in Debug mode and if the source files are available on the server. Some errors that occur
		/// inside of the .Net runtime itself or in ASP.Net pages also do not show the error.
		/// </summary>
        public string SourceCode { get; set;  }

        /// <summary>
        /// Retrieves the source file if available
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// Method where the error occurred
        /// </summary>
        public string SourceMethod { get; set;  }

        /// <summary>
        /// Line Number if available
        /// </summary>
        public int SourceLineNumber { get; set;  }

		/// <summary>
		/// The client's IP address
		/// </summary>
		public string IPAddress  {get; set;}

		/// <summary>
		/// The username of the user attached if the user is authenticated. 
		/// </summary>
		public string Login {get; set; }

		/// <summary>
		/// The client's browser string.
		/// </summary>
		public string Browser {get; set;}

		/// <summary>
		/// The referring Url that was used to access the current page that caused the error. 
		/// </summary>
		public string Referer {get; set; }

		/// <summary>
		/// Content of the POST buffer if the page was posted. The size is limited to 2k. Larger buffers are stripped.
		/// </summary>
		public string PostBuffer {get; set; }

		/// <summary>
		/// The size of the POST buffer if data was posted.
		/// </summary>
		public int ContentSize {get; set; }

		/// <summary>
		/// The complete querystring.
		/// </summary>
		public string QueryString { get; set; }

		/// <summary>
		/// The Locale string returned by the browser
		/// </summary>
		public string Locale {get; set; }

		/// <summary>
		/// The time the error was logged.
		/// </summary>
		public DateTime Time  {get; set; }

        /// <summary>
        /// List of all server variables
        /// </summary>
        public string ServerVariables {get; set;}

        /// <summary>
        /// Gets the HTTP Status Code
        /// </summary>
        public int OriginalHttpStatusCode { get; set;  }
		
		/// <summary>
		/// Public constructor requires that an exception is passed in. Generally you'll want to do this is in Application_Error
		/// and pass in in the InnerException of the error:
		/// 
		/// WebErrorHandler Handler = new WebErrorHandler(Server.GetLastError().InnerException);
		/// </summary>
		/// <param name="lastException">The Exception to log</param>
		public WebErrorHandler(Exception lastException = null) 
		{
		    if (lastException == null && HttpContext.Current != null)
		    {
		        lastException = HttpContext.Current.Server.GetLastError();
		        if (lastException != null)
		            lastException = lastException.GetBaseException();
		    }
            
            LastException = lastException;
            RetrieveSourceLines = true;
		}
		


	    /// <summary>
	    /// Parses the Exception into properties of this object. Called internally 
	    /// by LogError, but can also be called externally to get easier information
	    /// about the error through the property interface.
	    /// </summary>
	    public bool Parse()
	    {
	        if (LastException == null)
	            return false;

	        IsParsed = true;

	        // Use the Inner Exception since that has the actual error info
	        HttpRequest Request = HttpContext.Current.Request;

	        RawUrl = Request.RawUrl;

	        if (LastException is System.IO.FileNotFoundException)
	            ErrorMessage = "File not found: " + LastException.Message;
	        else
	            ErrorMessage = LastException.Message;

	        Time = DateTime.Now;

	        if (CompactFormat)
	            return true;
	        
	        FullUrl = Request.Url.AbsoluteUri;
	        //string.Format("http://{0}{1}", Request.ServerVariables["SERVER_NAME"], Request.RawUrl);
	        IPAddress = Request.UserHostAddress;

	        if (Request.UrlReferrer != null)
	            Referer = Request.UrlReferrer.ToString();

	        Browser = Request.UserAgent;

	        if (Request.IsAuthenticated)
	            Login = HttpContext.Current.User.Identity.Name;
	        else
	            Login = "Anonymous";

	        // Get the Locale String
	        if (Request.UserLanguages != null)
	        {
	            string Lang = Request.UserLanguages[0];
	            if (Lang != null)
	                Locale = Lang;
	        }

	        if (Request.TotalBytes > 0)
	        {
                int bytes = Request.TotalBytes;
	            if (bytes > 2048)
	                bytes = 2048;

                PostBuffer = Encoding.Default.GetString(Request.BinaryRead(bytes));
	            if (bytes == 2048)
	                PostBuffer += "...";

                ContentSize = Request.TotalBytes;
            }

	        ParseServerVariables();

            ParseSourceCode();

			return true;
		}

        /// <summary>
        /// Parses all ServerVariables into a string
        /// </summary>
        private void ParseServerVariables()
        {
            var serverVars = HttpContext.Current.Request.ServerVariables;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < serverVars.Count; i++)
            {
                string value = serverVars[i];
                string key = serverVars.Keys[i];
                sb.AppendFormat("{0:30}: {1}\r\n", key, value);
            }
            ServerVariables = sb.ToString();
        }

        /// <summary>
        /// Parses source code related properties
        /// </summary>
	    private void ParseSourceCode()
	    {
            StackTrace = LastException.StackTrace;
	        if (RetrieveSourceLines)
	        {
	            StringBuilder sb = new StringBuilder(1024);

	            // Try to retrieve Source Code information
	            StackTrace st = new StackTrace(LastException, true);
	            StackFrame sf = st.GetFrame(0);

	            if (sf != null)
	            {
	                string Filename = sf.GetFileName();                    

	                if (Filename != null && File.Exists(Filename))
	                {
                        SourceFile = Filename;
	                    SourceLineNumber = sf.GetFileLineNumber();
	                    SourceMethod = LastException.TargetSite.Name;

	                    if (SourceLineNumber > 0)
	                    {
	                        StreamReader sr = new StreamReader(Filename);

	                        // Read over unwanted lines
	                        int x = 0;
	                        for (x = 0; x < SourceLineNumber - 4; x++)
	                            sr.ReadLine();
	                        
	                        sb.AppendFormat("  File: {0}\r\n", Filename);
                            if (SourceLineNumber > 0)
                                sb.AppendFormat("Line #: {0}\r\n", SourceLineNumber);
                            sb.AppendFormat("Method: {0}\r\n", LastException.TargetSite);
	                        sb.AppendLine();

	                        sb.AppendFormat("Line {0}: {1}\r\n", x + 1, sr.ReadLine());
	                        sb.AppendFormat("Line {0}: {1}\r\n", x + 2, sr.ReadLine());
	                        sb.AppendFormat("Line {0}: {1}\r\n", x + 3, sr.ReadLine());
	                        sb.AppendFormat("<b>Line {0}: {1}</b>\r\n", x + 4, sr.ReadLine());
	                        sb.AppendFormat("Line {0}: {1}\r\n", x + 5, sr.ReadLine());
	                        sb.AppendFormat("Line {0}: {1}\r\n", x + 6, sr.ReadLine());
	                        sb.AppendFormat("Line {0}: {1}\r\n", x + 7, sr.ReadLine());

	                        sr.Close();
	                    }
	                }
	            }
	            SourceCode = sb.ToString();
	        }

	    }

	    /// <summary>
	    /// Takes the parsed error information from the object properties and parses this information into a string,
	    /// that can be displayed as plain text or in a browser. The string is formatted text and displays well
	    /// in a browser using a PRE tag.
	    ///
	    /// Method also displays request information including:
	    /// 
	    /// Full Url
	    /// Refering Url
	    /// IP Address of caller
	    /// Client Browser
	    /// Full POST buffer 
	    /// 
	    /// Full handling also returns:
	    /// 
	    /// Stack Trace
	    /// Source code blocks of the 5 lines before and after failure line if and/or debugging info is available
	    /// <returns>Formatted Error string</returns>
	    public override string ToString()
	    {
	        if (!IsParsed)
	            Parse();

	        StringBuilder sb = new StringBuilder();

	        sb.AppendLine(ErrorMessage + "\r\n\r\n");

	        sb.AppendLine("--- Base Error Info ---");
	        sb.AppendLine("Exception: " + LastException.GetType().Name);
	        sb.AppendFormat("      Url: {0}\r\n", RawUrl);
	        sb.AppendFormat("     Time: {0}\r\n", DateTime.Now.ToString("MMM dd, yyyy  HH:mm"));


	        if (CompactFormat)
	            return sb.ToString();


            if (!string.IsNullOrEmpty(SourceCode))
	        {
	            sb.AppendLine("\r\n--- Code ---");
	            sb.Append(SourceCode);
	        }

	        if (!string.IsNullOrEmpty(StackTrace) )
	            sb.AppendFormat("\r\n--- Stack Trace ---\r\n{0}\r\n\r\n", StackTrace);	    

    	    sb.Append("\r\n--- Request Information ---\r\n");
			sb.AppendFormat("  Full Url: {0}\r\n", FullUrl);
			sb.AppendFormat(" Client IP: {0}\r\n",IPAddress );
	        			
			if (!string.IsNullOrEmpty(Referer))
				sb.AppendFormat("   Referer: {0}\r\n",Referer );

			sb.AppendFormat("   Browser: {0}\r\n",Browser);
			sb.AppendFormat("     Login: {0}\r\n",Login);
			sb.AppendFormat("    Locale: {0}\r\n",Locale);
            sb.AppendFormat("    Status: {0}\r\n", OriginalHttpStatusCode);

			if (!string.IsNullOrEmpty(PostBuffer))
				sb.AppendFormat("\r\n\r\n--- Raw Post Buffer ---\r\n\r\n{0}",PostBuffer);


            if (!string.IsNullOrEmpty(ServerVariables))
                sb.AppendFormat("\r\n\r\n--- All Server Variables ---\r\n\r\n{0}", ServerVariables);

			return sb.ToString();
		}

        /// <summary>
        /// Semi generic Application_Error handler method that can be used
        /// to handle errors without additional code. You should implement
        /// the DisplayError event and LogError Event methods to display
        /// a message when ErrorHandlingModes is not Default
        /// </summary>
        /// <param name="errorHandlingMode"></param>
        public void HandleError(ErrorHandlingModes errorHandlingMode = ErrorHandlingModes.Default)
        {
            var context = HttpContext.Current;
            var Response = context.Response;            
            var Server = context.Server;

            if (Response.Filter != null)
            {
                var f = Response.Filter;
                f = null;  // REQUIRED! - w/o this null setting doesn't work
                Response.Filter = null;
            }

            var ex = Server.GetLastError().GetBaseException();

            OriginalHttpStatusCode = 500;
            if (ex is HttpException)
            {
                var httpException = (HttpException)ex;
                OriginalHttpStatusCode = httpException.GetHttpCode();
            }

            var errorHandler = new WebErrorHandler(ex);
            errorHandler.RetrieveSourceLines = this.RetrieveSourceLines;
            errorHandler.Parse();

            if (LogManagerConfiguration.Current.LogErrors &&
                // don't log URL link errors
                OriginalHttpStatusCode < 400 || OriginalHttpStatusCode > 410)
            {                
                OnLogError(errorHandler, LastException);
            }

            var model = new ErrorViewModel();
            model.Title = Resources.AnErrorOccurredInYourApplication;
            model.Message = errorHandler.ErrorMessage;
            
            model.WebErrorHandler = errorHandler;
            model.ErrorHandlingMode = errorHandlingMode;            

            if (errorHandlingMode == ErrorHandlingModes.Default)
            {
                 // return default ASP.NET error screen behavior
                // Yellow Screen of Death or ASP.NET DisplayErrors form
                Response.TrySkipIisCustomErrors = true;
                return;
            }        

            Response.ClearContent();
            Response.ClearHeaders();
            Server.ClearError();
            Response.TrySkipIisCustomErrors = true;

            Response.ContentType = "text/html";
            Response.StatusCode = 500;

            OnDisplayError(errorHandler, model);
        }

        /// <summary>
        /// Fired when all error information has been collected and 
        /// the information is ready for logging.
        /// </summary>
        public event Action<WebErrorHandler,Exception> LogError;

        /// <summary>
        /// Allows overriding for logging error information in a subclass
        /// </summary>
        /// <param name="error"></param>
        /// <param name="ex"></param>
	    protected virtual void OnLogError(WebErrorHandler error, Exception ex)
	    {
	        if (LogError != null)
	            LogError(error, ex);
	    }

        /// <summary>
        /// Fired when at the end of the error handling process when an error page 
        /// needs to be displayed
        /// </summary>
        public event Action<WebErrorHandler,ErrorViewModel> DisplayError;

        /// <summary>
        /// Allows overriding of DisplayError
        /// </summary>
        /// <param name="error"></param>
        /// <param name="model"></param>
        protected virtual void OnDisplayError(WebErrorHandler error, ErrorViewModel model)
        {
            if (DisplayError != null)
                DisplayError(error, model);
        }
	}

}
