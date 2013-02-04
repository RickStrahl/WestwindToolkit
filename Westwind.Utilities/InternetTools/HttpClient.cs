#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/12/2009
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion


using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO.Compression;

using Westwind.Utilities;

namespace Westwind.Utilities.InternetTools
{
	/// <summary>
	/// An HTTP wrapper class that abstracts away the common needs for adding post keys
	/// and firing update events as data is received. This class is real easy to use
	/// with many common operations requiring single method calls.
    ///
    /// The class also provides automated cookie and state handling, GZip compression
    /// decompression, simplified proxy and authentication mechanisms to provide a 
    /// simple single level class interface. The underlying WebRequest is also 
    /// exposed so you will not loose any functionality from the .NET BCL class.
	/// </summary>
	public class HttpClient
	{
		/// <summary>
		/// Determines how data is POSTed when when using AddPostKey() and other methods
		/// of posting data to the server. Support UrlEncoded, Multi-Part, XML and Raw modes.
		/// </summary>
		public HttpPostMode PostMode 
		{
			get { return _PostMode; }
			set { _PostMode = value; }
		}

		/// <summary>
		///  User name used for Authentication. 
		///  To use the currently logged in user when accessing an NTLM resource you can use "AUTOLOGIN".
		/// </summary>
		public string Username 
		{
			get { return _Username; }
			set { _Username = value; }
		}

		/// <summary>
		/// Password for Authentication.
		/// </summary>
		public string Password 
		{
			get {return _Password;}
			set {_Password = value;}
		}

		/// <summary>
		/// Address of the Proxy Server to be used.
		/// Use optional DEFAULTPROXY value to specify that you want to IE's Proxy Settings
		/// </summary>
		public string ProxyAddress 	
		{
			get {return _ProxyAddress;}
			set {_ProxyAddress = value;}
		}

		/// <summary>
		/// Semicolon separated Address list of the servers the proxy is not used for.
		/// </summary>
		public string ProxyBypass 
		{
			get {return _ProxyBypass;}
			set {_ProxyBypass = value;}
		}

		/// <summary>
		/// Username for a password validating Proxy. Only used if the proxy info is set.
		/// </summary>
		public string ProxyUsername 
		{
			get {return _ProxyUsername;}
			set {_ProxyUsername = value;}
		}
		/// <summary>
		/// Password for a password validating Proxy. Only used if the proxy info is set.
		/// </summary>
		public string ProxyPassword 
		{
			get {return _ProxyPassword;}
			set {_ProxyPassword = value;}
		}        


		/// <summary>
		/// Timeout for the Web request in seconds. Times out on connection, read and send operations.
		/// Default is 30 seconds.
		/// </summary>
		public int Timeout 
		{
			get {return _ConnectTimeout; }
			set {_ConnectTimeout = value; }
		}

		/// <summary>
		/// Returns whether the last request was cancelled through one of the
		/// events.
		/// </summary>
		public bool Cancelled
		{
			get { return _Cancelled; }
			set { _Cancelled = value; }
		}
		bool _Cancelled;

        
        /// <summary>
        /// Use this option to set a custom content type. 
        /// If possible use PostMode to specify a predefined
        /// content type as it will ensure that Post data is
        /// appropriately formatted.
        /// 
        /// If setting the content type manually POST data
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
            set { 
                _ContentType = value;
                PostMode = HttpPostMode.Raw;
            }
        }
        private string _ContentType = string.Empty;


        // this doesn't seem necessary - . NET will automatically decode common encodings like UTF-8
        ///// <summary>
        ///// The Encoding used to decode the response data
        ///// </summary>        
        //public Encoding ResponseEncoding
        //{
        //    get { return _ResponseEncoding; }
        //    set { _ResponseEncoding = value; }
        //}
        //private Encoding _ResponseEncoding = Encoding.Default;

        
        /// <summary>
        /// Determines whether requests attempt to use GZip when retrieving content
        /// from the server.
        /// </summary>
        public bool UseGZip
        {
            get { return _UseGZip; }
            set { _UseGZip = value; }
        }
        private bool _UseGZip = false;


		/// <summary>
		/// Error Message if the Error Flag is set or an error value is returned from a method.
		/// </summary>
		public string ErrorMessage 
		{
			get { return _ErrorMessage; } 
			set { _ErrorMessage = value; }
		}
		
		/// <summary>
		/// Error flag if an error occurred.
		/// </summary>
		public bool Error
		{
			get { return _Error; } 
			set { _Error = value; }
		}

		/// <summary>
		/// Determines whether errors cause exceptions to be thrown. By default errors 
		/// are handled in the class and the Error property is set for error conditions.
		/// (not implemented at this time).
		/// </summary>
		public bool ThrowExceptions 
		{
			get { return _ThrowExceptions; }
			set { _ThrowExceptions = value;}
		} 

		/// <summary>
		/// If set to true will automatically track cookies
        /// between multiple successive requests on this 
        /// instance. Uses the CookieCollection property
        /// to persist cookie status.
        /// 
        /// When set posts values in the CookieCollection,
        /// and on return fills the CookieCollection with
        /// cookies from the Response.
		/// </summary>
		public bool HandleCookies
		{
			get { return _HandleCookies; }
			set { _HandleCookies = value; }
		}
                

		/// <summary>
		/// Holds the internal Cookie collection before or after a request. This 
		/// collection is used only if HandleCookies is set to .t. which also causes it
		///  to capture cookies and repost them on the next request.
		/// </summary>
		public CookieCollection Cookies 
		{
			get 
			{
				if (_Cookies == null)
					Cookies = new CookieCollection();
					  
				return _Cookies; 
			}
			set { _Cookies = value; }
		}

		/// <summary>
		/// WebResponse object that is accessible after the request is complete and 
		/// allows you to retrieve additional information about the completed request.
		/// 
		/// The Response Stream is already closed after the GetUrl methods complete 
		/// (except GetUrlResponse()) but you can access the Response object members 
		/// and collections to retrieve more detailed information about the current 
		/// request that completed.
		/// </summary>
		public HttpWebResponse WebResponse  
		{
			get { return _WebResponse;}
			set { _WebResponse = value; }
		}

		/// <summary>
		/// WebRequest object that can be manipulated and set up for the request if you
		///  called .
		/// 
		/// Note: This object must be recreated and reset for each request, since a 
		/// request's life time is tied to a single request. This object is not used if
		///  you specify a URL on any of the GetUrl methods since this causes a default
		///  WebRequest to be created.
		/// </summary>
		public HttpWebRequest WebRequest  
		{
			get { return _WebRequest; }
			set { _WebRequest = value; }
		}

		/// <summary>
		/// The buffersize used for the Send and Receive operations
		/// </summary>
		public int BufferSize 
		{
			get { return _BufferSize; }
			set { _BufferSize = value; }
		}
		int _BufferSize = 100;

		/// <summary>
		/// Lets you specify the User Agent  browser string that is sent to the server.
		///  This allows you to simulate a specific browser if necessary.
		/// </summary>
		public string UserAgent 
		{
			get { return _UserAgent; }
			set { _UserAgent = value; }
		}
		string _UserAgent = "West Wind HTTP .NET Client";

		
		// member properties
		//string cPostBuffer = string.Empty;
		MemoryStream _PostStream;
		BinaryWriter _PostData;

		HttpPostMode _PostMode = HttpPostMode.UrlEncoded;

		int _ConnectTimeout = 30;

		string _Username = string.Empty;
		string _Password = string.Empty;

		string _ProxyAddress = string.Empty;
		string _ProxyBypass = string.Empty;
		string _ProxyUsername = string.Empty;
		string _ProxyPassword = string.Empty;

		bool _ThrowExceptions = false;
		bool _HandleCookies = false;
	
		string _ErrorMessage = string.Empty;
		bool _Error = false;
		
		HttpWebResponse _WebResponse;
		HttpWebRequest _WebRequest;
		CookieCollection _Cookies;

		string _MultiPartBoundary = "-----------------------------" + DateTime.Now.Ticks.ToString("x");

		/// <summary>
		/// The HttpClient Default Constructor
		/// </summary>
		public HttpClient()
		{
		}

		/// <summary>
		/// Creates a new WebRequest instance that can be set prior to calling the 
		/// various Get methods. You can then manipulate the WebRequest property, to 
		/// custom configure the request.
		/// 
		/// Instead of passing a URL you  can then pass null.
		/// 
		/// Note - You need a new Web Request for each and every request so you need to
		///  set this object for every call if you manually customize it.
		/// </summary>
		/// <param name="String Url">
		/// The Url to access with this WebRequest
		/// </param>
		/// <returns>Boolean</returns>
		public bool CreateWebRequestObject(string Url) 
		{
			try 
			{
				WebRequest =  (HttpWebRequest) System.Net.WebRequest.Create(Url);                
                
 			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
				return false;
			}

			return true;
		}


        /// <summary>
        /// Resets the Post buffer by clearing out all existing content
        /// </summary>
        public void ResetPostData()
        {
            _PostStream = new MemoryStream();
            _PostData = new BinaryWriter(_PostStream);
        }

		/// <summary>
		/// Adds POST form variables to the request buffer.
		/// PostMode determines how parms are handled.
		/// </summary>
		/// <param name="Key">Key value or raw buffer depending on post type</param>
		/// <param name="Value">Value to store. Used only in key/value pair modes</param>
		public void AddPostKey(string Key, byte[] Value)
		{
            if (Key == "RESET")
            {
                ResetPostData();
                return;
            }
			
			if (_PostData == null) 
			{
				_PostStream = new MemoryStream();
				_PostData = new BinaryWriter(_PostStream);
			}

			switch(_PostMode)
			{
				case HttpPostMode.UrlEncoded:
					_PostData.Write( Encoding.Default.GetBytes(Key + "=" +
						StringUtils.UrlEncode(Encoding.Default.GetString(Value)) +
						"&") );
					break;
				case HttpPostMode.MultiPart:
					_PostData.Write( Encoding.Default.GetBytes(
						"--" + _MultiPartBoundary + "\r\n" + 
						"Content-Disposition: form-data; name=\"" +Key+"\"\r\n\r\n") );
					
					_PostData.Write( Value );

					_PostData.Write( Encoding.Default.GetBytes("\r\n") );
					break;
				default:  // Raw or Xml, JSON modes
					_PostData.Write( Value );
					break;
			}
		}

        public void SetPostStream(Stream postStream)
        {
            MemoryStream ms = new MemoryStream(1024);
            FileUtils.CopyStream(postStream, ms, 1024);
            ms.Flush();
            ms.Position = 0;
            _PostStream = ms;
            _PostData = new BinaryWriter(ms);            
        }

		/// <summary>
		/// Adds POST form variables to the request buffer.
		/// PostMode determines how parms are handled.
		/// </summary>
		/// <param name="Key">Key value or raw buffer depending on post type</param>
		/// <param name="Value">Value to store. Used only in key/value pair modes</param>
		public void AddPostKey(string Key, string Value)
		{
			AddPostKey(Key,Encoding.GetEncoding(1252).GetBytes(Value));
		}

		/// <summary>
		/// Adds a fully self contained POST buffer to the request.
		/// Works for XML or previously encoded content.
		/// </summary>
		/// <param name="FullPostBuffer">String based full POST buffer</param>
		public void AddPostKey(string FullPostBuffer) 
		{
			AddPostKey(null,FullPostBuffer );
		}

		/// <summary>
		/// Adds a fully self contained POST buffer to the request.
		/// Works for XML or previously encoded content.
		/// </summary>
		/// <param name="PostBuffer">Byte array of a full POST buffer</param>
		public void AddPostKey(byte[] FullPostBuffer) 
		{
			AddPostKey(null,FullPostBuffer);
		}

		/// <summary>
		/// Allows posting a file to the Web Server. Make sure that you 
		/// set PostMode
		/// </summary>
		/// <param name="Key"></param>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public bool AddPostFile(string Key,string FileName) 
		{
			byte[] lcFile;	

			if (_PostMode != HttpPostMode.MultiPart) 
			{
				_ErrorMessage = "File upload allowed only with Multi-part forms";
				_Error = true;
				return false;
			}

			try 
			{			
				FileStream loFile = new FileStream(FileName,System.IO.FileMode.Open,System.IO.FileAccess.Read);

				lcFile = new byte[loFile.Length];
				loFile.Read(lcFile,0,(int) loFile.Length);
				loFile.Close();
			}
			catch(Exception e) 
			{
				_ErrorMessage = e.Message;
				_Error = true;
				return false;
			}

			if (_PostData == null) 
			{
				_PostStream = new MemoryStream();
				_PostData = new BinaryWriter(_PostStream);
			}

			_PostData.Write( Encoding.Default.GetBytes(
				"--" + _MultiPartBoundary + "\r\n"  + 
				"Content-Disposition: form-data; name=\"" + Key + "\"; filename=\"" + 
				new FileInfo(FileName).Name + "\"\r\n\r\n") );

			_PostData.Write( lcFile );

			_PostData.Write( Encoding.Default.GetBytes("\r\n")) ;

			return true;
		}

        /// <summary>
        /// Return a the result from an HTTP Url into a StreamReader.
        /// Client code should call Close() on the returned object when done reading.
        /// </summary>
        /// <param name="url">Url to retrieve.</param>		
        /// <returns></returns>
        [Obsolete("Use DownloadStream() instead.")]
        public StreamReader GetUrlStream(string Url)
        {
            return DownloadStream(Url);
        }

		/// <summary>
		/// Return a the result from an HTTP Url into a StreamReader.
		/// Client code should call Close() on the returned object when done reading.
		/// </summary>
		/// <param name="url">Url to retrieve.</param>		
		/// <returns></returns>
		public StreamReader DownloadStream(string url) 
		{
			Encoding enc;

			HttpWebResponse Response = DownloadResponse(url);
			if (Response == null)
				return null;
			            
			try 
			{
				if (!string.IsNullOrEmpty(Response.CharacterSet) )
					enc = Encoding.GetEncoding(Response.CharacterSet);
				else
					enc = Encoding.Default;
			}
			catch
			{
				// Invalid encoding passed
				enc = Encoding.Default; 
			}
            
            Stream responseStream = null;
            if (Response.ContentEncoding.ToLower().Contains("gzip"))
                responseStream = new GZipStream(Response.GetResponseStream(), CompressionMode.Decompress);
            else if (Response.ContentEncoding.ToLower().Contains("deflate"))
                responseStream = new DeflateStream(Response.GetResponseStream(), CompressionMode.Decompress);
            else
                responseStream = Response.GetResponseStream();
            			
			// drag to a stream
			StreamReader strResponse = new StreamReader(responseStream,enc); 
			return strResponse;
		}


        /// <summary>
        /// Return an HttpWebResponse object for a request. You can use the Response to
        /// read the result as needed. This is a low level method. Most of the other 'Get'
        /// methods call this method and process the results further.
        /// </summary>
        /// <remarks>Important: The Response object's Close() method must be called when you are done with the object.</remarks>
        /// <param name="url">Url to retrieve.</param>
        /// <returns>An HttpWebResponse Object</returns>
        [Obsolete("Use DownloadResponse instead.")]
        public HttpWebResponse GetUrlResponse(string url)
        {
            return DownloadResponse(url);
        }

		/// <summary>
		/// Return an HttpWebResponse object for a request. You can use the Response to
		/// read the result as needed. This is a low level method. Most of the other 'Get'
		/// methods call this method and process the results further.
		/// </summary>
		/// <remarks>Important: The Response object's Close() method must be called when you are done with the object.</remarks>
		/// <param name="url">Url to retrieve.</param>
		/// <returns>An HttpWebResponse Object</returns>
		public HttpWebResponse DownloadResponse(string url)
		{
			Cancelled = false;
		
            //try 
            //{
				_Error = false;
				_ErrorMessage = string.Empty;
				_Cancelled = false;

				if (WebRequest == null) 
				{
					WebRequest =  (HttpWebRequest) System.Net.WebRequest.Create(url);
					WebRequest.Headers.Add("Cache","no-cache");
				}

				
				WebRequest.UserAgent = _UserAgent;
				WebRequest.Timeout = _ConnectTimeout * 1000;

				// Handle Security for the request
				if (_Username.Length > 0) 
				{
					if (_Username  == "AUTOLOGIN")
						WebRequest.Credentials = CredentialCache.DefaultCredentials;
					else
						WebRequest.Credentials = new NetworkCredential(_Username,_Password);
				}

				// Handle Proxy Server configuration
				if (_ProxyAddress.Length > 0) 
				{
					if (_ProxyAddress == "DEFAULTPROXY") 
					{
                        WebRequest.Proxy = HttpWebRequest.DefaultWebProxy;
					}
					else 
					{
                        WebProxy Proxy = new WebProxy(_ProxyAddress,true);
            
						if (_ProxyBypass.Length > 0) 
						{
							Proxy.BypassList = _ProxyBypass.Split(';');
						}

						if (_ProxyUsername.Length > 0)
							Proxy.Credentials = new NetworkCredential(_ProxyUsername,_ProxyPassword);

						WebRequest.Proxy = Proxy;
					}
				}

                if (UseGZip)
                    WebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
				
				// Handle cookies - automatically re-assign 
				if (_HandleCookies || (_Cookies != null && _Cookies.Count > 0)  )
				{
					WebRequest.CookieContainer = new CookieContainer();
					if (_Cookies != null && _Cookies.Count > 0) 
					{
						WebRequest.CookieContainer.Add(_Cookies);
					}
				}

				// Deal with the POST buffer if any
				if (_PostData != null) 
				{
					WebRequest.Method = "POST";

                        switch (_PostMode)
                        {
                            case HttpPostMode.UrlEncoded:
                                WebRequest.ContentType = "application/x-www-form-urlencoded";
                                break;
                            case HttpPostMode.MultiPart:
                                WebRequest.ContentType = "multipart/form-data; boundary=" + _MultiPartBoundary;
                                _PostData.Write(Encoding.GetEncoding(1252).GetBytes("--" + _MultiPartBoundary + "--\r\n"));
                                break;
                            case HttpPostMode.Xml:
                                WebRequest.ContentType = "text/xml";
                                break;
                            case HttpPostMode.Json:
                                WebRequest.ContentType = "application/json";
                                break;
                            case HttpPostMode.Raw:
                                //WebRequest.ContentType = "application/octet-stream";
                                break;
                            default:
                                goto case HttpPostMode.UrlEncoded;
                    }

                    if (!string.IsNullOrEmpty(ContentType))
                        WebRequest.ContentType = ContentType;

					Stream requestStream = WebRequest.GetRequestStream();
					
					if (SendData == null)
						_PostStream.WriteTo(requestStream);  // Simplest version - no events
					else 
						StreamPostBuffer(requestStream);     // Send in chunks and fire events

					//*** Close the memory stream
					_PostStream.Close();
					_PostStream = null;

					//*** Close the Binary Writer
                    if (_PostData != null)
                    {
                        _PostData.Close();
                        _PostData = null;
                    }
					//*** Close Request Stream
					requestStream.Close();

                    // clear out the Post buffer
                    ResetPostData();

					// If user cancelled the 'upload' exit
					if (Cancelled) 
					{
						ErrorMessage = "HTTP Request was cancelled.";
						Error = true;
						return null;
					}
				}
		
				// Retrieve the response headers 
				HttpWebResponse Response = null;
				try
				{
					Response = (HttpWebResponse) WebRequest.GetResponse();                    
				}
				catch(WebException ex)
				{
					// Check for 500 error return - if so we still want to return a response
					// Client can check oHttp.WebResponse.StatusCode
					if (ex.Status == WebExceptionStatus.ProtocolError) 
					{
						Response = (HttpWebResponse) ex.Response;
					}
					else
						throw;
				}

				_WebResponse = Response;
                				
				// Close out the request - it cannot be reused
				WebRequest = null;

				// ** Save cookies the server sends
				if (_HandleCookies)  
				{
					if (Response.Cookies.Count > 0)  
					{
						if (_Cookies == null)  
						{
							_Cookies = Response.Cookies;
						}
						else 
						{
							// ** If we already have cookies update the list
							foreach (Cookie oRespCookie in Response.Cookies)  
							{
								bool bMatch = false;
								foreach(Cookie oReqCookie in _Cookies)  
								{
									if (oReqCookie.Name == oRespCookie.Name)  
									{
										oReqCookie.Value = oRespCookie.Value;
										bMatch = true;
										break; // 
									}
								} // for each ReqCookies
								if (!bMatch)
									_Cookies.Add(oRespCookie);
							} // for each Response.Cookies
						}  // Cookies == null
					} // if Response.Cookie.Count > 0
				}  // if bHandleCookies = 0

				
				return Response;
            //}
            //catch (Exception e)
            //{
            //    if (_ThrowExceptions)
            //        throw e;

            //    _ErrorMessage = e.Message;
            //    _Error = true;
            //    return null;
            //}
		}

		/// <summary>
		/// Sends the Postbuffer to the server
		/// </summary>
		/// <param name="PostData"></param>
		protected void StreamPostBuffer(Stream PostData) 
		{

				if ( _PostStream.Length < BufferSize) 
				{
					_PostStream.WriteTo(PostData);

					// Handle Send Data Even
					// Here just let it know we're done
					if (SendData != null) 
					{
						ReceiveDataEventArgs Args = new ReceiveDataEventArgs();
						Args.CurrentByteCount = _PostStream.Length;
						Args.Done = true;
						SendData(this,Args);
					}
				}
				else 
				{
					// Send data up in 8k blocks
					byte[] Buffer = _PostStream.GetBuffer();
					int lnSent = 0;
					int lnToSend = (int)  _PostStream.Length;
					int lnCurrent = 1;
					while (true) 
					{
						if (lnToSend < 1 || lnCurrent < 1) 
						{
							if (SendData != null) 
							{
								ReceiveDataEventArgs Args = new ReceiveDataEventArgs();
								Args.CurrentByteCount = lnSent;
								Args.TotalBytes = Buffer.Length;
								Args.Done = true;
								SendData(this,Args);
							}
							break;
						}

						lnCurrent = lnToSend;

						if (lnCurrent > BufferSize) 
						{
							lnCurrent = BufferSize;
							lnToSend = lnToSend - lnCurrent;
						}
						else 
						{
							lnToSend = lnToSend - lnCurrent;
						}

						PostData.Write(Buffer,lnSent,lnCurrent);

						lnSent = lnSent + lnCurrent;

						if (SendData != null) 
						{
							ReceiveDataEventArgs Args = new ReceiveDataEventArgs();
							Args.CurrentByteCount = lnSent;
							Args.TotalBytes = Buffer.Length;
							if (Buffer.Length == lnSent) 
							{
								Args.Done = true;
								SendData(this,Args);
								break;
							}
							SendData(this,Args);

							if (Args.Cancel) 
							{
								Cancelled = true;
								break;
							}
						}
					}
				}
		}


        /// <summary>
        /// Returns the content of a URL as a string
        /// </summary>
        /// <param name="url"></param>
        /// <param name="bufferSize">The intermediate download buffer used</param>
        /// <param name="encoding">A .NET Encoding scheme or null to attempt sniffing from Charset.</param>
        /// <returns></returns>
        [Obsolete("Use the DownloadString() method instead.")]
        public string GetUrl(string url, long bufferSize = 8192, Encoding encoding = null)        
        {            
            return DownloadString(url, bufferSize, encoding);
        }

        /// <summary>
        /// Returns the content of a URL as a string using a specified Encoding
        /// </summary>
        /// <param name="url"></param>
        /// <param name="bufferSize">Internal download buffer size used to hold data chunks.</param>
        /// <param name="encoding">A .NET Encoding scheme or null to attempt sniffing from Charset.</param>
        /// <returns></returns>
        public string DownloadString(string url, long bufferSize = 8192, Encoding encoding = null)
        {
            byte[] bytes = DownloadBytes(url, bufferSize);
            if (bytes == null)
                return null;

            if (encoding == null)
            {
                encoding = Encoding.Default;

                try
                {
                    if (!string.IsNullOrEmpty(WebResponse.CharacterSet))
                    {
                        string charset = WebResponse.CharacterSet.ToLower();

                        // special case UTF-8 since it's most common
                        if (charset.Contains("utf-8"))
                            encoding = Encoding.UTF8;
                        else if (charset.Contains("utf-16"))
                            encoding = Encoding.Unicode;
                        else if (charset.Contains("utf-7"))
                            encoding = Encoding.UTF7;
                        else if (charset.Contains("utf-32"))
                            encoding = Encoding.UTF32;                        
                        else
                            encoding = Encoding.GetEncoding(WebResponse.CharacterSet);
                    }
                }
                catch { } // ignore encoding assignment failures
            }
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Returns a partial response from the URL by specifying only 
        /// given number of bytes to retrieve. This can reduce network
        /// traffic and keep string formatting down if you are only 
        /// interested a small port at the top of the page. Also 
        /// returns full headers.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Obsolete("Use DownloadStringPartial() instead.")]
        public string GetUrlPartial(string url, int size)
        {
            return GetUrlPartial(url, size);
        }


        /// <summary>
        /// Returns a partial response from the URL by specifying only 
        /// given number of bytes to retrieve. This can reduce network
        /// traffic and keep string formatting down if you are only 
        /// interested a small port at the top of the page. Also 
        /// returns full headers.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public string DownloadStringPartial(string url, int size)
        {
            StreamReader sr = DownloadStream(url);
            if (sr == null)
                return null;

            char[] InBuffer = new char[size];

            sr.Read(InBuffer, 0, size);
            sr.Close();

            return new string(InBuffer);
        }

    
		/// <summary>
		/// Retrieves URL into an Byte Array.
		/// </summary>
		/// <remarks>Fires the ReceiveData Event</remarks>
		/// <param name="Url">Url to read</param>
		/// <param name="bufferSize">Size of the buffer for each read. 0 = 8192</param>
		/// <returns></returns>
        [Obsolete("Use DownloadBytes() method.")]

		public byte[] GetUrlBytes(string url,long bufferSize=8192) 
		{   
           return DownloadBytes(url, bufferSize);
        }       

        /// <summary>
        /// Retrieves URL into an Byte Array.
        /// </summary>
        /// <remarks>Fires the ReceiveData Event</remarks>
        /// <param name="url">Url to read</param>
        /// <param name="bufferSize">Size of the buffer for each read. 0 = 8192</param>
        /// <returns></returns>
        public byte[] DownloadBytes(string url, long bufferSize = 8192)
        {         
			HttpWebResponse Response = DownloadResponse(url);
            if (Response == null)
                return null;            

			BinaryReader responseReader = 
				new BinaryReader(Response.GetResponseStream()); 		

			if (responseReader == null)
				return null;

			if (bufferSize < 1)
				bufferSize = 8192;

			long responseSize = bufferSize;
			if (Response.ContentLength > 0)
				responseSize = _WebResponse.ContentLength;
			else
                // No content size provided
				responseSize = -1;

            // pre-allocate the buffer
            MemoryStream ms = new MemoryStream();
            
            byte[] buffer = new byte[bufferSize];

			ReceiveDataEventArgs args = new ReceiveDataEventArgs();
			args.TotalBytes = responseSize;

            long bytesRead = 1;
			int count = 0;
			long totalBytes = 0;

			while (bytesRead > 0) 
			{
                if (responseSize != -1 && totalBytes + bufferSize >  responseSize)
					bufferSize = responseSize - totalBytes;

                    
				bytesRead = responseReader.Read(buffer,0,(int) bufferSize);
                if (bytesRead > 0)
                {                   
                    // write to stream
                    ms.Write(buffer, 0, (int) bytesRead);
                    
                    count++;
                    totalBytes += bytesRead;
                    
                    // Raise an event if hooked up
                    if (ReceiveData != null)
                    {
                        /// Update the event handler
                        args.CurrentByteCount = totalBytes;
                        args.NumberOfReads = count;
                        args.CurrentChunk = null;  // don't send anything here
                        ReceiveData(this, args);

                        // Check for cancelled flag
                        if (args.Cancel)
                        {
                            _Cancelled = true;
                            goto CloseDown;
                        }
                    }
                }
                else
                    break;
			} // while


			CloseDown:
				responseReader.Close();
                

			// Send Done notification
			if (ReceiveData != null && !args.Cancel) 
			{
				// Update the event handler
				args.Done = true;
				ReceiveData(this,args);
			}
            //ms.Flush();
            ms.Position = 0;
			return ms.ToArray();
		}

		/// <summary>
		/// Writes the output from the URL request to a file firing events.
		/// </summary>
		/// <param name="Url">Url to fire</param>
		/// <param name="BufferSize">Buffersize - how often to fire events</param>
		/// <param name="OutputFile">File to write response to</param>
		/// <returns>true or false</returns>
        [Obsolete("Use DownloadFile() instead.")]
        public bool GetUrlFile(string Url, long BufferSize, string OutputFile)
        {
            return DownloadFile(Url, BufferSize, OutputFile);
        }

        /// <summary>
        /// Writes the output from the URL request to a file firing events.
        /// </summary>
        /// <param name="url">Url to fire</param>
        /// <param name="bufferSize">Buffersize - how often to fire events</param>
        /// <param name="outputFile">File to write response to</param>
        /// <returns>true or false</returns>
        public bool DownloadFile(string url,long bufferSize,string outputFile) 
		{
			byte[] Result = DownloadBytes(url,bufferSize);
			if (Result == null)
				return false;

			FileStream File = new FileStream(outputFile,FileMode.OpenOrCreate,FileAccess.Write);
			File.Write(	Result,0,(int)WebResponse.ContentLength);
			File.Close();

			return true;
		}

        /// <summary>
        /// Sets the certificate policy.
        /// 
        /// Note this is a global setting and affects the entire application.
        /// It's recommended you set this for the application and not on 
        /// a per request basis.
        /// </summary>
        /// <param name="Ignore"></param>
        public static bool IgnoreCertificateErrors
        {
            set
            {
                if (value)
                   ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(CheckCertificateCallback);
                else
                   ServicePointManager.ServerCertificateValidationCallback -= new RemoteCertificateValidationCallback(CheckCertificateCallback);
            }            
        }

        /// <summary>
        /// Handles the Certificate check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cert"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckCertificateCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }


   

		#region Events and Event Delegates and Arguments

		/// <summary>
		/// Fires progress events when receiving data from the server
		/// </summary>
		public event ReceiveDataDelegate ReceiveData;
		public delegate void ReceiveDataDelegate(object sender, ReceiveDataEventArgs e);

		/// <summary>
		/// Fires progress events when using GetUrlEvents() to retrieve a URL.
		/// </summary>
		public event ReceiveDataDelegate SendData;
		
		/// <summary>
		/// Event arguments passed to the ReceiveData event handler on each block of data sent
		/// </summary>
		public class ReceiveDataEventArgs 
		{
			/// <summary>
			/// Size of the cumulative bytes read in this request
			/// </summary>
			public long CurrentByteCount=0;

			/// <summary>
			/// The number of total bytes of this request
			/// </summary>
			public long TotalBytes = 0;

			/// <summary>
			/// The number of reads that have occurred - how often has this event been called.
			/// </summary>
			public int NumberOfReads = 0;
			
			/// <summary>
			/// The current chunk of data being read
			/// </summary>
			public char[] CurrentChunk;
			
			/// <summary>
			/// Flag set if the request is currently done.
			/// </summary>
			public bool Done = false;

			/// <summary>
			/// Flag to specify that you want the current request to cancel. This is a write-only flag
			/// </summary>
			public bool Cancel = false;
		}
		#endregion

	}

	/// <summary>
	/// Enumeration of the various HTTP POST modes supported by HttpClient
	/// </summary>
	
	public enum HttpPostMode 
	{
		UrlEncoded,
		MultiPart,
		Xml,
        Json,
		Raw
	};

    /// <summary>
    /// Internal object used to allow setting WebRequest.CertificatePolicy to 
    /// not fail on Cert errors
    /// </summary>
    internal class AcceptAllCertificatePolicy : ICertificatePolicy
    {
        public AcceptAllCertificatePolicy()
        {
        }

        public bool CheckValidationResult(ServicePoint sPoint,
           X509Certificate cert, WebRequest wRequest, int certProb)
        {
            // Always accept
            return true;
        }
    }



}
