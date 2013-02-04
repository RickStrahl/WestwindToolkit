//#define ERRORLOGGING   - old error logging code removed

using System;
using System.Web;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using Westwind.Utilities.Logging;


namespace Westwind.Web
{
	/// <summary>
	/// Class handles generating Error strings for admin email and optional program display
	/// provides the same information that the ASP.Net error page displays, with the ability
	/// to retrieve into string and emailing.
	/// </summary>
	public class WebErrorHandler
	{
		/// <summary>
		/// Name of the logfile. Filename is relative to the virtual Web root.
		/// </summary>
		public string LogFileName = "Errorlog.xml";

		public Exception LastError = null;
		protected object WriteLock = new object();
		protected bool IsParsed = false;
		
		/// <summary>
		/// If true returns only the error message and URL.
		/// </summary>
		public bool CompactFormat = false;

		/// <summary>
		/// Determines whether the routines attempt to retrieve Source Code lines.
		/// </summary>
		public bool RetrieveSourceLines = true;


		/// <summary>
		/// The runtime error message thrown by the application.
		/// </summary>
		public string ErrorMessage = "";

		/// <summary>
		/// The raw Web relative URL that caused this exception to occur. 
		/// Example: /WebStore/Item.aspx?sku=WWHELP
		/// </summary>
		public string RawUrl = "";

		/// <summary>
		/// The completely qualified Url to this request.
		/// Example: http://www.west-wind.com/webstore/item.aspx?sku=WWSTOREDOTNET
		/// </summary>
		public string FullUrl = "";

		/// <summary>
		/// Stack trace listing as available from the runtime compiler
		/// </summary>
		public string StackTrace = "";

		/// <summary>
		/// The source code if available for the error that occurred. The code will include the 5 surrounding lines before and after.
		/// 
		/// Source code is available only in Debug mode and if the source files are available on the server. Some errors that occur
		/// inside of the .Net runtime itself or in ASP.Net pages also do not show the error.
		/// </summary>
		public string SourceCode = "";

		/// <summary>
		/// The client's IP address
		/// </summary>
		public string IPAddress = "";

		/// <summary>
		/// The username of the user attached if the user is authenticated. 
		/// </summary>
		public string Login = "";

		/// <summary>
		/// The client's browser string.
		/// </summary>
		public string Browser = "";

		/// <summary>
		/// The referring Url that was used to access the current page that caused the error. 
		/// </summary>
		public string Referer = "";

		/// <summary>
		/// Content of the POST buffer if the page was posted. The size is limited to 2k. Larger buffers are stripped.
		/// </summary>
		public string PostBuffer = "";

		/// <summary>
		/// The size of the POST buffer if data was posted.
		/// </summary>
		public int ContentSize = 0;

		/// <summary>
		/// The complete querystring.
		/// </summary>
		public string QueryString = "";

		/// <summary>
		/// The Locale string returned by the browser
		/// </summary>
		public string Locale = "";

		/// <summary>
		/// The time the error was logged.
		/// </summary>
		public DateTime Time = DateTime.Now;

		
		/// <summary>
		/// Public constructor requires that an exception is passed in. Generally you'll want to do this is in Application_Error
		/// and pass in in the InnerException of the error:
		/// 
		/// WebErrorHandler Handler = new WebErrorHandler(Server.GetLastError().InnerException);
		/// </summary>
		/// <param name="lastError">The Exception to log</param>
		public WebErrorHandler(Exception lastError) 
		{
			this.LastError = lastError;
		}
		
		
		/// <summary>
		/// Parameterless constructor. Use only if you want to use the 
		/// maintenance methods (Show, ClearLog etc)  of this class. All
		/// processinging functions require that the Exception is passed.
		/// </summary>
		public WebErrorHandler() 
		{
		}



		/// <summary>
		/// Parses the Exception into properties of this object. Called internally 
		/// by LogError, but can also be called externally to get easier information
		/// about the error through the property interface.
		/// </summary>
		public bool Parse() 
		{
			if (LastError == null) 
				return false;

			IsParsed = true;

			// Use the Inner Exception since that has the actual error info
			HttpRequest Request = HttpContext.Current.Request;

			RawUrl = Request.RawUrl;

			if ( LastError is System.IO.FileNotFoundException)
				ErrorMessage = "File not found: " + LastError.Message;
			else
				ErrorMessage = LastError.Message;

			Time = DateTime.Now;

			if (CompactFormat)
				return true;

			this .StackTrace = LastError.StackTrace;
			if (RetrieveSourceLines) 
			{
				StringBuilder sb = new StringBuilder(1024);

				// Try to retrieve Source Code information
				StackTrace st = new StackTrace(LastError,true);
				StackFrame sf = st.GetFrame(0);
				if (sf != null) 
				{
					string Filename = sf.GetFileName();
	            
					if (RetrieveSourceLines && Filename != null && File.Exists(Filename)) 
					{
						int LineNumber = sf.GetFileLineNumber();
						if (LineNumber > 0) 
						{
							StreamReader sr = new StreamReader(Filename);

							// Read over unwanted lines
							int x = 0;
							for (x = 0; x < LineNumber - 4; x++ ) 
								sr.ReadLine();

							sb.AppendLine("--- Code ---");
							sb.AppendFormat("File: {0}r\n",Filename);
							sb.AppendFormat("Method: {0}\r\n\r\n",LastError.TargetSite);
							sb.AppendFormat("Line {0}: {1}\r\n",x + 1,sr.ReadLine());
							sb.AppendFormat("Line {0}: {1}\r\n",x + 2,sr.ReadLine());
							sb.AppendFormat("Line {0}: {1}\r\n",x + 3,sr.ReadLine());
							sb.AppendFormat("<b>Line {0}: {1}</b>\r\n", x+ 4,sr.ReadLine() );
							sb.AppendFormat("Line {0}: {1}\r\n",x +5,sr.ReadLine());
							sb.AppendFormat("Line {0}: {1}\r\n",x +6,sr.ReadLine());
							sb.AppendFormat("Line {0}: {1}\r\n",x +7,sr.ReadLine());

							sr.Close();
						}
					}
				}

				SourceCode = sb.ToString();
			}

			FullUrl = string.Format("http://{0}{1}",Request.ServerVariables["SERVER_NAME"],Request.RawUrl);
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
				string Lang =  Request.UserLanguages[0];
				if (Lang != null)  
					Locale = Lang;
			}

			if (Request.TotalBytes > 0 && Request.TotalBytes < 2048)  
			{
				PostBuffer = Encoding.Default.GetString(Request.BinaryRead(Request.TotalBytes));
				ContentSize = Request.TotalBytes;
			}
			else if (Request.TotalBytes > 2048)  // strip the result
			{
				PostBuffer = Encoding.Default.GetString(Request.BinaryRead(2048)) + "...";
				ContentSize = Request.TotalBytes;
			}

			return true;
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

            sb.AppendFormat("{0}\r\n\r\n",RawUrl);
            			
            sb.AppendLine(" Exception: " + LastError.ToString());
			sb.AppendFormat( " on {0}\r\n",DateTime.Now.ToString().ToLower());            


			if (CompactFormat)
				return sb.ToString();

			if (StackTrace != "") 
				sb.AppendFormat( "\r\n--- Stack Trace ---\r\n{0}\r\n\r\n",StackTrace); 	

			if (SourceCode != "")
				sb.Append(SourceCode);

			sb.Append("--- Request Information ---\r\n");
			sb.AppendFormat("  Full Url: {0}\r\n", FullUrl);
			sb.AppendFormat("        IP: {0}\r\n",IPAddress );
			
			if (Referer != "")
				sb.AppendFormat("   Referer: {0}\r\n",Referer );

			sb.AppendFormat("   Browser: {0}\r\n",Browser);

			
			sb.AppendFormat("     Login: {0}\r\n",Login);

			sb.AppendFormat("    Locale: {0}\r\n",Locale);

			if (PostBuffer != "")
				sb.AppendFormat("\r\n\r\n--- Raw Post Buffer ---\r\n\r\n{0}",PostBuffer);

			return sb.ToString();
		}

#if ERRORLOGGING

		/// <summary>
		/// Logs an error to the XML file specified in the 
		/// </summary>
		/// <param name="LogAsString"></param>
		/// <returns></returns>
		public bool LogErrorToXml(bool LogAsString)
		{
			// Parse the exception into local properties
			if (!IsParsed)
				Parse();

			bool WriteEndDoc = true;
	
			// Log as elements
			lock(this) 
			{
				string LogFile = HttpContext.Current.Request.PhysicalApplicationPath + LogFileName;
				
				FileStream loFile = null;
				try 
				{
					loFile = new FileStream(LogFile,FileMode.OpenOrCreate,FileAccess.Write,FileShare.Write);
					loFile.Seek(0,SeekOrigin.End);

					// If the file's not empty start writing over the end doc tag
					// We'll rewrite it at the end
					if (loFile.Position > 0)
						loFile.Seek(-1 * "</WebErrorLog>\r\n".Length,SeekOrigin.End);
				}
				catch 
				{
					return false;
				}

				XmlTextWriter XWriter;
				XWriter = new XmlTextWriter( (Stream) loFile, Encoding.UTF8);
				XWriter.Formatting = Formatting.Indented;
				XWriter.IndentChar = ' ';
				XWriter.Indentation = 4;


				// If the file is empty write the root element
				if (loFile.Position == 0)  
				{
					XWriter.WriteStartElement("WebErrorLog");
					WriteEndDoc = false; // it'll automatically unwind the StartElement
				}
		

				if (LogAsString) 
				{
					XWriter.WriteStartElement("WebError");
					XWriter.WriteAttributeString("time",DateTime.Now.ToString());
		
					XWriter.WriteStartElement("error");
					XWriter.WriteCData(ToString());
					XWriter.WriteEndElement();
					//XWriter.WriteElementString("error",ToString());
					
		
					XWriter.WriteEndElement();
				}
				else 
				{
					XWriter.WriteStartElement("WebError");
					XWriter.WriteAttributeString("time",Time.ToString());
					XWriter.WriteElementString("errormessage",ErrorMessage);
					XWriter.WriteElementString("rawurl",RawUrl);
					XWriter.WriteElementString("fullurl",FullUrl);
					XWriter.WriteElementString("referer",Referer);
					XWriter.WriteElementString("ipaddress",IPAddress);
					XWriter.WriteElementString("login",Login);
					XWriter.WriteElementString("browser",Browser);
					XWriter.WriteElementString("querystring",QueryString);
					XWriter.WriteElementString("contentsize",ContentSize.ToString());
					XWriter.WriteElementString("postbuffer",PostBuffer);

					XWriter.WriteElementString("stacktrace",StackTrace);
					XWriter.WriteElementString("sourcecode",SourceCode);
			
					XWriter.WriteEndElement();
				}
				if (WriteEndDoc)
					XWriter.WriteRaw("\r\n</WebErrorLog>\r\n");
				else 
				{
						XWriter.WriteEndElement();
						XWriter.WriteRaw("\r\n");
				}


				XWriter.Close();
				loFile.Close();

				return true;
			}
		}

		/// <summary>
		/// Logs the current Exception state to the logfile in XML format. The item is logged as individual elements.
		/// </summary>
		public bool LogErrorToXml() 
		{			
			return LogErrorToXml(false);
		}

		/// <summary>
		/// Logs the error to Sql Server through the WebRequestLog
		/// </summary>
		/// <param name="ConnectionString"></param>
		/// <param name="LogAsString"></param>
		/// <returns></returns>
		public bool LogErrorToSql(string ConnectionString) 
		{
			if (!IsParsed)
				Parse();
            
			WebRequestLog.LogCustomMessage(ConnectionString,WebRequestLogMessageTypes.Error,ToString());

			return true;

		}
		
		/// <summary>
		/// Displays the log as an ASP.Net response.
		/// </summary>
		/// <returns>true or false. False likely means file doesn't exist or access is denied.</returns>
		public bool ShowXmlLog() 
		{
			string TFile = HttpContext.Current.Request.PhysicalApplicationPath + LogFileName;
			HttpResponse Response = HttpContext.Current.Response;

			Response.ContentType = "text/xml";

			if ( !File.Exists(TFile) )
				Response.Write("<WebErrorLog>Log File is empty</WebErrorLog>");
			else 
			{
					
				try 
				{
                    Response.Redirect( HttpContext.Current.Request.ApplicationPath + "/" + LogFileName.Replace("\\","/") );

//					Response.WriteFile( TFile );
				}
				catch{ return false; }
			}

			Response.End();
			return true;
		}

		/// <summary>
		/// Tries to delete the log file.
		/// </summary>
		/// <returns>true or false. False likely means file doesn't exist or access is denied.</returns>
		public bool ClearXmlLog() 
		{
			try 
			{
				File.Delete( HttpContext.Current.Request.PhysicalApplicationPath + LogFileName );
			}
			catch { return false; }

			return true;
		}

#endif

	}

}
