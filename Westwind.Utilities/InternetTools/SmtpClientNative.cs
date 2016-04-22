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
using System.Threading;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Security;
using System.Collections.Generic;

namespace Westwind.Utilities.InternetTools
{
    /// <summary>
    /// SMTP Wrapper around System.Net.Email.SmtpClient. Provided 
    /// here mainly to provide compatibility with existing wwSmtp code
    /// and to provide a slightly more user friendly front end interface
    /// on a single object.
    /// </summary>
    public class SmtpClientNative : IDisposable
    {

        /// <summary>
        /// Mail Server to send message through. Should be a domain name 
        /// (mail.yourserver.net) or IP Address (211.123.123.123).
        /// 
        /// You can also provide a port number as part of the string which will 
        /// override the ServerPort (yourserver.net:211)
        /// <seealso>Class wwSmtp</seealso>
        /// </summary>
        public string MailServer = string.Empty;

        /// <summary>
        /// Port on the mail server to send through. Defaults to port 25.
        /// </summary>
        public int ServerPort = 25;

        /// <summary>
        /// Use Tls Security
        /// </summary>
        public bool UseSsl = false;

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
        public string SenderName = String.Empty;

        /// <summary>
        /// The ReplyTo address
        /// </summary>
        public string ReplyTo = String.Empty;

        /// <summary>
        /// Message Subject.
        /// </summary>
        public string Subject = String.Empty;

        /// <summary>
        /// The body of the message.
        /// </summary>
        public string Message = String.Empty;

        /// <summary>
        /// Username to connect to the mail server.
        /// </summary>
        public string Username = String.Empty;

        /// <summary>
        /// Password to connect to the mail server.
        /// </summary>
        public string Password = String.Empty;

        /// <summary>
        /// Any attachments you'd like to send
        /// </summary>
        public string Attachments = String.Empty;

        /// <summary>
        /// List of attachment objects
        /// </summary>
        public List<Attachment> AttachmentList = new List<Attachment>();

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
        public System.Text.Encoding Encoding = Encoding.Default;

        /// <summary>
        /// 
        /// </summary>
        public string AlternateText = string.Empty;

        /// <summary>
        /// The content type for the alternate 
        /// </summary>
        public string AlternateTextContentType = "text/plain";

        /// <summary>
        /// The user agent for the x-mailer
        /// </summary>
        public string UserAgent = "";

        /// <summary>
        /// Determines the priority of the message
        /// </summary>
        public string Priority = "Normal";

        /// <summary>
        /// Determines whether a return receipt is sent
        /// </summary>
        public bool ReturnReceipt = false;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal List<AlternateView> AlternateViews = new List<AlternateView>();


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
        public int Timeout = 30;

        /// <summary>
        /// SMTP headers for this email request
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// Event that's fired after each message is sent. This
        /// event differs from SendComplete that it fires
        /// after each send operation of each message rather
        /// than before closing the connection.
        /// </summary>
        //public event delSmtpNativeEvent MessageSendComplete;

        /// <summary>
        /// Event that's fired after each message is sent. This
        /// event differs from SendComplete that it fires
        /// after each send operation of each message rather
        /// than before closing the connection.
        /// </summary>
        //public event delSmtpNativeEvent MessageSendError;

        /// <summary>
        /// Event fired when sending of a message or multiple messages
        /// is complete and the connection is to be closed. This event
        /// occurs only once per connection as opposed to the MessageSendComplete
        /// event which fires after each message is sent regardless of the
        /// number of SendMessage operations.
        /// </summary>
        public event delSmtpNativeEvent SendComplete;

        /// <summary>
        /// Event fired when an error occurs during processing and before
        /// the connection is closed down.
        /// </summary>
        public event delSmtpNativeEvent SendError;

        /// <summary>
        /// Internal instance of SmtpClient that holds the 'connection'
        /// effectively.
        /// </summary>
        private SmtpClient smtp = null;

        /// <summary>
        /// Adds an Smtp header to this email request. Headers are 
        /// always cleared after a message has been sent or failed.
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="value"></param>
        public void AddHeader(string headerName, string value)
        {
            if (headerName.ToLower() == "clear" || headerName.ToLower() == "reset")
                this.Headers.Clear();
            else
            {
                if (!Headers.ContainsKey(headerName))
                    this.Headers.Add(headerName, value);
                else
                    this.Headers[headerName] = value;
            }
        }

        /// <summary>
        /// Adds headers from a CR/LF separate string that has key:value header pairs 
        /// defined.
        /// </summary>
        /// <param name="headers"></param>
        public void AddHeadersFromString(string headers)
        {
            string[] lines = headers.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] tokens = line.Split(':');
                if (tokens.Length != 2)
                    continue;

                this.AddHeader(tokens[0].Trim(), tokens[1].Trim());
            }
        }

        /// <summary>
        /// Lets you load the actual SMTP client instance
        /// prior to use so you can manipulate the actual
        /// Smtp instance.
        /// </summary>
        /// <returns></returns>
        public SmtpClient LoadSmtpClient()
        {            
            int serverPort = this.ServerPort;
            string server = this.MailServer;

            // if there's a port we need to split the address
            string[] parts = server.Split(':');
            if (parts.Length > 1)
            {
                server = parts[0];
                serverPort = int.Parse(parts[1]);
            }

            if (server == null || server == string.Empty)
            {
                this.SetError("No Mail Server specified.");
                this.Headers.Clear();
                return null;
            }

            smtp = null;
            try
            {
                smtp = new SmtpClient(server, serverPort);

                if (this.UseSsl)
                    smtp.EnableSsl = true;
            }
            catch (SecurityException)
            {
                this.SetError("Unable to create SmptClient due to missing permissions. If you are using a port other than 25 for your email server, SmtpPermission has to be explicitly added in Medium Trust.");
                this.Headers.Clear();
                return null;
            }

            // This is a Total Send Timeout not a Connection timeout!
            smtp.Timeout = this.Timeout * 1000;

            if (!string.IsNullOrEmpty(this.Username))
                smtp.Credentials = new NetworkCredential(this.Username, this.Password);

            return smtp;
        }

        /// <summary>
        /// Starts a new SMTP session. Note this doesn't actually open a connection
        /// but just configures and sets up the SMTP session. The actual connection
        /// is opened only when a message is actually sent
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            if (smtp == null)
                smtp = LoadSmtpClient();

            if (smtp == null)
                return false;

            return true;
        }

        /// <summary>
        /// Cleans up and closes the connection
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            this.smtp = null;

            // clear all existing headers
            this.Headers.Clear();

            return true;
        }

        /// <summary>
        /// Fully self contained mail sending method. Sends an email message by connecting 
        /// and disconnecting from the email server.
        /// </summary>
        /// <returns>true or false</returns>
        public bool SendMail()
        {
            if (!this.Connect())
                return false;

            try
            {
                // Create and configure the message 
                using (MailMessage msg = this.GetMessage())
                {
                    smtp.Send(msg);

                    if (this.SendComplete != null)
                        this.SendComplete(this);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg = ex.InnerException.Message;

                this.SetError(msg);
                if (this.SendError != null)
                    this.SendError(this);

                return false;
            }
            finally
            {
                // close connection and clear out headers
                this.Close();
            }

            return true;
        }

        /// <summary>
        /// Run mail sending operation on a separate thread and asynchronously
        /// Operation does not return any information about completion.
        /// </summary>
        /// <returns></returns>
        public void SendMailAsync()
        {
            //ThreadStart delSendMail = new ThreadStart(this.SendMailRun);
            //delSendMail.BeginInvoke(null, null);

            Thread mailThread = new Thread(this.SendMailRun);
            mailThread.Start();
        }

        protected void SendMailRun()
        {
            // Create an extra reference to insure GC doesn't collect
            // the reference from the caller
            SmtpClientNative Email = this;
            Email.SendMail();
        }

        /// <summary>
        /// Sends an individual message. Allows sending several messages
        /// on the same SMTP session without having to reconnect each time.
        /// 
        /// This version assigns default properties assigned from the main
        /// mail object and allows overriding only of recipients
        /// 
        /// Call after Connect() has been called and call Close() to 
        /// close the connection afterwards
        /// </summary>
        /// <returns></returns>
        public bool SendMessage(string recipient, string ccList, string bccList)
        {
            try
            {
                // Create and configure the message 
                using (MailMessage msg = this.GetMessage())
                {
                    this.AssignMailAddresses(msg.To, recipient);
                    this.AssignMailAddresses(msg.CC, ccList);
                    this.AssignMailAddresses(msg.Bcc, bccList);

                    smtp.Send(msg);
                }

                if (this.SendComplete != null)
                    this.SendComplete(this);
            }
            catch (Exception ex)
            {
                this.SetError(ex.Message);
                if (this.SendError != null)
                    this.SendError(this);

                return false;
            }

            return true;
        }





        /// <summary>
        /// Configures the message interface
        /// </summary>
        /// <param name="msg"></param>
        protected virtual MailMessage GetMessage()
        {
            MailMessage msg = new MailMessage();

            msg.Body = this.Message;
            msg.Subject = this.Subject;
            msg.From = new MailAddress(this.SenderEmail, this.SenderName);

            if (!string.IsNullOrEmpty(this.ReplyTo))
                msg.ReplyToList.Add(new MailAddress(this.ReplyTo));

            // Send all the different recipients
            this.AssignMailAddresses(msg.To, this.Recipient);
            this.AssignMailAddresses(msg.CC, this.CC);
            this.AssignMailAddresses(msg.Bcc, this.BCC);

            // add string attachments from comma delimited list
            if (!string.IsNullOrEmpty(this.Attachments))
            {
                string[] files = this.Attachments.Split(new char[2] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string file in files)
                {
                    msg.Attachments.Add(new Attachment(file));
                }
            }
            // add actual attachment objects
            foreach(var att in this.AttachmentList)
            {
                msg.Attachments.Add(att);
            }

            if (this.ContentType.StartsWith("text/html"))
                msg.IsBodyHtml = true;
            else
                msg.IsBodyHtml = false;

            msg.BodyEncoding = this.Encoding;


            msg.Priority = (MailPriority)Enum.Parse(typeof(MailPriority), this.Priority);
            if (!string.IsNullOrEmpty(this.ReplyTo))
                msg.ReplyToList.Add(new MailAddress(this.ReplyTo));

            if (this.ReturnReceipt)
                msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;

            if (!string.IsNullOrEmpty(this.UserAgent))
                this.AddHeader("x-mailer", this.UserAgent);


            if (!string.IsNullOrEmpty(this.AlternateText))
            {
                byte[] alternateBytes = Encoding.Default.GetBytes(this.AlternateText);
                MemoryStream ms = new MemoryStream(alternateBytes);
                ms.Position = 0;
                msg.AlternateViews.Add(new AlternateView(ms));
                //ms.Close();
            }
            if (this.AlternateViews.Count > 0)
            {
                foreach (var view in this.AlternateViews)
                {
                    msg.AlternateViews.Add(view);
                }
            }

            foreach (var header in this.Headers)
            {
                msg.Headers[header.Key] = header.Value;
            }

            return msg;
        }


        /// <summary>
        /// Assigns mail addresses from a string or comma delimited string list.
        /// Facilitates 
        /// </summary> 
        /// <param name="recipients"></param>
        /// <returns></returns>
        private void AssignMailAddresses(MailAddressCollection address, string recipients)
        {
            if (string.IsNullOrEmpty(recipients))
                return;

            string[] recips = recipients.Split(',', ';');

            for (int x = 0; x < recips.Length; x++)
            {
                address.Add(new MailAddress(recips[x]));
            }
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
                int lnIndex = fullEmail.IndexOf("<");
                int lnIndex2 = fullEmail.IndexOf(">");
                string lcEmail = fullEmail.Substring(lnIndex + 1, lnIndex2 - lnIndex - 1);
                return lcEmail;
            }

            return fullEmail;
        }

        /// <summary>
        /// Adds a new Alternate view to the request. Passed from FoxPro
        /// which sets up this object.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="contentType"></param>
        /// <param name="contentId"></param>
        public void AddAlternateView(AlternateView view)
        {
            this.AlternateViews.Add(view);
        }



        /// <summary>
        /// Logs a message to the specified LogFile
        /// </summary>
        /// <param name="FormatString"></param>
        /// <param name="?"></param>
        protected void LogString(string message)
        {
            if (string.IsNullOrEmpty(this.LogFile))
                return;

            if (!message.EndsWith("\r\n"))
                message += "\r\n";

            using (StreamWriter sw = new StreamWriter(this.LogFile, true))
            {
                sw.Write(message);
            }
        }


        /// <summary>
        /// Internally used to set errors
        /// </summary>
        /// <param name="errorMessage"></param>
        private void SetError(string errorMessage)
        {
            if (errorMessage == null || errorMessage.Length == 0)
            {
                this.ErrorMessage = string.Empty;
                this.Error = false;
                return;
            }

            ErrorMessage = errorMessage;
            Error = true;
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (this.smtp != null)
                this.smtp = null;

        }

        #endregion
    }

    /// <summary>
    /// Delegate used to handle Completion and failure events
    /// </summary>
    /// <param name="Smtp"></param>
    public delegate void delSmtpNativeEvent(SmtpClientNative Smtp);


}
