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
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Westwind.Utilities;

namespace Westwind.Utilities.InternetTools
{
	/// <summary>
	/// A free standing SMTP implementation that doesn't rely on 
	/// CDONTS or COM Interop. Minimal implementation supports
	/// only basic functionality at this time.
	/// </summary>
	public class SmtpClientCustom
	{
		/// <summary>
		/// Mail Server to send message through. Should be a domain name 
		/// (mail.yourserver.net) or IP Address (211.123.123.123).
		/// 
		/// You can also provide a port number as part of the string which will 
		/// override the ServerPort.
		/// <seealso>Class wwSmtp</seealso>
		/// </summary>
		public string MailServer = string.Empty;

		/// <summary>
		/// Port on the mail server to send through. Defaults to port 25.
		/// </summary>
		public int ServerPort = 25;

		/// <summary>
		/// Email address or addresses of the Recipient. Comma delimit multiple addresses. To have formatted names use
		/// "Rick Strahl" &lt;rstrahl@west-wind.com&gt;
		/// </summary>
		public string Recipient = string.Empty;

		/// <summary>
		/// Carbon Copy Recipients
		/// </summary>
		public string CC = string.Empty;

		/// <summary>
		/// Blind Copy Recipients
		/// </summary>
		public string BCC = string.Empty;

		/// <summary>
		/// Email address of the sender
		/// </summary>
		public string SenderEmail = string.Empty;

		/// <summary>
		/// Display name of the sender (optional)
		/// </summary>
		public string SenderName = string.Empty;

		/// <summary>
		/// Message Subject.
		/// </summary>
		public string Subject = string.Empty;

		/// <summary>
		/// The body of the message.
		/// </summary>
		public string Message = string.Empty;

		/// <summary>
		/// Username to connect to the mail server.
		/// </summary>
		public string Username = string.Empty;
		/// <summary>
		/// Password to connect to the mail server.
		/// </summary>
		public string Password = string.Empty;

		/// <summary>
		/// The content type of the message. text/plain default or you can set to any other type like text/html
		/// </summary>
		public string ContentType = "text/plain";

		/// <summary>
		/// Character Encoding for the message.
		/// </summary>
		public string CharacterEncoding = "8bit";

		/// <summary>
		/// The character Encoding used to write the stream out to disk
		/// Defaults to the default Locale used on the server.
		/// </summary>
		public System.Text.Encoding Encoding = null;

        /// <summary>
        /// An optional file name that appends logging information for the TCP/IP messaging
        /// to the specified file.
        /// </summary>
        public string LogFile = string.Empty;

		/// <summary>
		/// Determines whether wwSMTP passes back errors as exceptions or
		/// whether it sets error properties. Right now only error properties
		/// work reliably.
		/// </summary>
		public bool HandleExceptions = true;

		/// <summary>
		/// An Error Message if the result is negative or Error is set to true;
		/// </summary>
		public string ErrorMessage = string.Empty;

		/// <summary>
		/// Error Flag set when an error occurs.
		/// </summary>
		public bool Error = false;

		/// <summary>
		/// Connection timeouts for the mail server in seconds. If this timeout is exceeded waiting for a connection
		/// or for receiving or sending data the request is aborted and fails.
		/// </summary>
		public int Timeout = 15;

		private NetworkStream NetStream = null;
		private TcpClient Tcp = null;
		private bool isESMTP = false;
        
        /// <summary>
        /// Event fired when sending of a message or multiple messages
        /// is complete and the connection is to be closed. This event
        /// occurs only once per connection as opposed to the MessageSendComplete
        /// event which fires after each message is sent regardless of the
        /// number of SendMessage operations.
        /// </summary>
        public event delSmtpEvent SendComplete;

        /// <summary>
        /// Event that's fired after each message is sent. This
        /// event differs from SendComplete that it fires
        /// after each send operation of each message rather
        /// than before closing the connection.
        /// </summary>
        //public event delSmtpEvent MessageSendComplete;

        //public virtual OnMessageSendComplete()

        /// <summary>
        /// Event fired when an error occurs during processing and before
        /// the connection is closed down.
        /// </summary>
        public event delSmtpEvent SendError;

		public SmtpClientCustom()
		{			
		}

		/// <summary>
		/// Connects to the mail server.
		/// </summary>
		/// <returns>True or False</returns>
		public bool Connect() 
		{
			Tcp = new TcpClient();

			isESMTP = true;

			Tcp.SendTimeout = Timeout * 1000;
			Tcp.ReceiveTimeout = Timeout * 1000;

            if (!string.IsNullOrEmpty(LogFile))
                LogString("\r\n*** Starting SMTP connection - " + DateTime.Now.ToString());

			int serverPort = ServerPort;
			string Server = MailServer;
			string[] Parts = Server.Split(':');
			if (Parts.Length > 1) 
			{
				Server = Parts[0];
				serverPort = int.Parse(Parts[1]);
			}

			if (Server == null || Server == string.Empty) 
			{
				SetError("No Mail Server specified.");
				return false;
			}
			
			try 
			{
				Tcp.Connect(Server,serverPort);
			}
			catch(Exception ex) 
			{ 
				SetError(ex.Message);
				return false;
			}

			NetStream = Tcp.GetStream();

			string response = SendReceive("EHLO " + Environment.MachineName + "\r\n");
			if (!CheckResponseCode(response,"220"))
			{
				response = SendReceive("HELO " + MailServer + "\r\n");
				if (!CheckResponseCode(response,"220") )
				{
					CloseConnection();
					return false;
				}
				isESMTP = false;
			}
			else 
			{
				// ESMTP will send command list - which might get sent on first
				// buffer or with a separate one
				if (response.IndexOf("250 ") < 0) 
					response = Read();
			}

			// Handle Login if provided
			if (isESMTP && Username != null && Username.Length > 0 &&
				response.ToLower().IndexOf("auth ") > -1) 
			{

				response = SendReceive("auth login\r\n");
				if (!CheckResponseCode(response,"334"))
				{
					CloseConnection();
					ErrorMessage = response;
					return false;
				}

				string lcB64 = Convert.ToBase64String( Encoding.Default.GetBytes(Username) );
				response = SendReceive(lcB64 + "\r\n");
				if (!CheckResponseCode(response,"334") )
				{
					CloseConnection();
					return false;
				}

				lcB64 = Convert.ToBase64String( Encoding.Default.GetBytes(Password) );
				response = SendReceive(lcB64 + "\r\n");
				if (!CheckResponseCode(response,"235"))
				{
					CloseConnection();
					return false;
				}
			}

			return true;
		}
		
		/// <summary>
		/// Fully self contained mail sending method. Sends an email message by connecting 
		/// and disconnecting from the email server.
		/// </summary>
		/// <returns>true or false</returns>
		public bool SendMail() 
		{
            if (!Connect())
            {
                if (SendError != null)
                    SendError(this);

                return false;
            }

            if (!SendMessage())
            {
                if (SendError != null)
                    SendError(this);

                return false;
            }

            if (SendComplete != null)
                SendComplete(this);

			Close();
			return true;
		}

		/// <summary>
		/// Fully self contained method that sends email by just sending
		/// without waiting for confirmation by starting a new thread
		/// </summary>
		/// <returns></returns>
		public void SendMailAsync() 
		{
			ThreadStart oDelegate = new ThreadStart(SendMailRun);
//			Thread myThread = new Thread(oDelegate);
//			myThread.Start();

			/// If you want to use the Thread Pool you can just use a delegate
			/// This will often be faster and more efficient especially for quick operations
			oDelegate.BeginInvoke(null,null);
		}

		protected void SendMailRun() 
		{
			// Create an extra reference to insure GC doesn't collect
			// the reference from the caller
			SmtpClientCustom Email = this;  
			Email.SendMail();
		}

		/// <summary>
		/// Low level SendMessage method. Requires that Connect() be called first to open
		/// a connection. You can call this method multiple times without reconnecting to
		/// send multiple messages.
		/// </summary>
		/// <returns>True or False</returns>
		public bool SendMessage() 
		{
            if (!string.IsNullOrEmpty(LogFile))
                LogString("\r\n*** Starting SMTP Send Operation - " + DateTime.Now.ToString());

			string lcResponse = string.Empty;

//			if (SenderEmail.IndexOf("<") > 0) 
//			{
//				int lnIndex=SenderEmail.IndexOf("<");
//				int lnIndex2 = SenderEmail.IndexOf(">");
//				string lcEmail = SenderEmail.Substring(lnIndex+1,lnIndex2-lnIndex -1);
//
//				lcResponse  = SendReceive("MAIL FROM: <" + lcEmail + ">\r\n");
//			}
//			else
//				lcResponse = SendReceive("MAIL FROM: <" + SenderEmail + ">\r\n");
//
			lcResponse = SendReceive("MAIL FROM: <" + GetEmailFromFullAddress(SenderEmail) + ">\r\n");

			if (!CheckResponseCode(lcResponse,"250") )
			{
				return false;
			}

//			lcResponse = SendReceive("rcpt to: <" + Recipient + ">\r\n");
//			if (!CheckResponseCode(lcResponse,"250") )
//			{
//				return false;
//			}

            if (!SendRecipients(Recipient))
                return false;

            if (!SendRecipients(CC))
                return false;

            if (!SendRecipients(BCC))
                return false;
            
			lcResponse = SendReceive("DATA\r\n");
			if (!CheckResponseCode(lcResponse,"354") )
			{
				lcResponse = Read();
				if (!CheckResponseCode(lcResponse,"354") ) 
				{
					return false;
				}
			}

			Send("to: " + Recipient + "\r\n");
			Send("cc: " + CC + "\r\n");
			

			string Email = SenderEmail;
			if (SenderName != null && SenderName.Length > 0) 
			{
				Email = "\"" + SenderName + "\" <" + Email + ">";
			}
			Send("from: " + Email + "\r\n");
			Send("subject: " + Subject + "\r\n");
			Send("x-mailer: wwSmtp .Net\r\n");
			Send("Importance: normal\r\n");
			Send("Mime-Version: 1.0\r\n");
			Send("Content-Type: " + ContentType + "\r\n");
			Send("Content-Transfer-Encoding:" + CharacterEncoding + "\r\n");
            Send("Date: " + TimeUtils.MimeDateTime(DateTime.Now) + "\r\n");

			Send("\r\n" + Message + "\r\n");

			lcResponse = SendReceive(".\r\n");
			if (!CheckResponseCode(lcResponse,"250") )
			{
				return false;
			}

			return true;
		}


		/// <summary>
		/// Sends all recipients from a comma or semicolon separated list.
		/// </summary> 
		/// <param name="lcRecipients"></param>
		/// <returns></returns>
		bool SendRecipients(string lcRecipients) 
		{
			if (lcRecipients == null ||lcRecipients.Length == 0)
				return true;

			string[] loRecips = lcRecipients.Split(',',';');

			for (int x = 0; x < loRecips.Length; x++) 
			{
				string lcResponse = SendReceive("RCPT TO: <"  + loRecips[x] + ">\r\n");
                if (!CheckResponseCode(lcResponse, "250"))
                    return false;
			}
			return true;
		}

		/// <summary>
		/// Strips out just the email address from a full email address that might contain a display name
		/// in the format of: "Web Monitor" &lt;rstrahl@west-wind.com&gt;
		/// </summary>
		/// <param name="fullEmail">Full email address to parse. Note currently only "<" and ">" tags are recognized as message delimiters</param>
		/// <returns>only the email address</returns>
		string GetEmailFromFullAddress(string fullEmail) 
		{
			if (fullEmail.IndexOf("<") > 0) 
			{
				int lnIndex=fullEmail.IndexOf("<");
				int lnIndex2 = fullEmail.IndexOf(">");
				string lcEmail = fullEmail.Substring(lnIndex+1,lnIndex2-lnIndex -1);
				return lcEmail;
			}

			return fullEmail;
		}

		/// <summary>
		/// Close SMTP Connection and sends QUIT command. Use after calls to SendMessage()
		/// </summary>
		void Close() 
		{
			SendReceive("quit\r\n");
			CloseConnection();

            if (!string.IsNullOrEmpty(LogFile))
                LogString("*** SMTP Connection closed: " + DateTime.Now.ToString());
		}

		/// <summary>
		/// Closes the mail server connection. Unlike Close() this method only resets
		/// the connection objects, but doesn't send any Session exit comands.
		/// </summary>
		void CloseConnection() 
		{
			NetStream.Close();
			Tcp.Close();
		}

        /// the following are worker methods for the TCP/IP handling and parsing of incoming content
        /// Note: This should eventually be extracted into a separate class.
        /// 
        void Send(string lcText)
        {
            if (!HandleExceptions)
            {
                if (!string.IsNullOrEmpty(LogFile))
                    LogString("CLT: " + lcText);
                WriteToStream(NetStream, lcText);
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(LogFile))
                        LogString("CLT: " + lcText);
                    WriteToStream(NetStream, lcText);
                }
                catch (Exception e)
                {
                    SetError(e.Message);
                    if (!string.IsNullOrEmpty(LogFile))
                        LogString("CLT: Error: " + e.Message);
                }
            }
        }
        
        
        string Read()
        {
            string Result = string.Empty;

            if (!HandleExceptions)
            {
                Result = ReadFromStream(NetStream);
                if (!string.IsNullOrEmpty(LogFile))
                    LogString("SVR: " + Result);
            }
            else
            {
                try
                {
                    Result = ReadFromStream(NetStream);
                    if (!string.IsNullOrEmpty(LogFile))
                        LogString("SVR: " + Result);
                }
                catch (Exception e)
                {
                    SetError(e.Message);
                    if (!string.IsNullOrEmpty(LogFile))
                        LogString("SVR: Receive Error " + e.Message);
                }
            }

            return Result;
        }


		string SendReceive(string lcText)
		{
			WriteToStream(NetStream,lcText);
			return ReadFromStream(NetStream);
		}
		bool CheckResponseCode(string lcResponse, string lcCode) 
		{
			int CodeLength = lcCode.Length;
			int ResponseLength = lcResponse.Length;
			if (ResponseLength < CodeLength)
				return false;

			if (lcResponse.Substring(0,lcCode.Length) != lcCode) 
			{
				ErrorMessage = lcResponse;
				return false;
			}

			return true;
		}
		private void WriteToStream(NetworkStream nw, string line)
		{
			
			try
			{
				Byte[] arrToSend = null;
				if (Encoding == null)
					arrToSend = Encoding.Default.GetBytes(line);
				else
					arrToSend = Encoding.GetBytes(line);

				nw.Write(arrToSend, 0, arrToSend.Length);
			}
            catch (System.IO.IOException e)
            {
                SetError(e.Message);
            }
			catch(Exception e)
			{
				SetError(e.Message);
			}
		}

		private string ReadFromStream(NetworkStream nw)
		{
			string returnMsg;
			try
			{
				byte[] readBuffer = new byte[1024];
				int length = nw.Read(readBuffer, 0, readBuffer.Length);
				returnMsg = Encoding.Default.GetString(readBuffer, 0, length);
			}
			catch(Exception e)
			{
				//throw new Exception("Read from Stream threw an exception: " + e.ToString());
				SetError(e.ToString());
				returnMsg = string.Empty;
			}

			return returnMsg;
		}


        /// <summary>
        /// Logs a message to the specified LogFile
        /// </summary>
        /// <param name="FormatString"></param>
        /// <param name="?"></param>
        protected void LogString(string Message)
        {
            if (string.IsNullOrEmpty(LogFile))
                return;

            if (!Message.EndsWith("\r\n"))
                Message += "\r\n";

            using (StreamWriter sw = new StreamWriter(LogFile, true))
            {
                sw.Write(Message);
            }
        }


		/// <summary>
		/// Error setting method.
		/// </summary>
		/// <param name="errorMessage"></param>
		private void SetError(string errorMessage)
		{
			if (errorMessage == null || errorMessage.Length == 0) 
			{
				ErrorMessage = string.Empty;
				Error = false;
				return;
			}

			ErrorMessage = errorMessage;
			Error = true;
		}
        
        

        


    
    }

    public delegate void delSmtpEvent(SmtpClientCustom Smtp);
}
