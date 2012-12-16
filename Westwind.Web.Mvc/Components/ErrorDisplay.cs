using System.Web;
using System.Text;
using System.Web.Mvc;
using Westwind.Utilities;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// An error display component that allows rich rendering of individual messages
    /// as well as validation error messages.
    /// 
    /// Object should be passed in to view end rendered with 
    /// &lt;%= ((ErrorDisplay) ViewData["ErrorDisplay"]).Show(450,true) %&gt;
    /// </summary>
    public class ErrorDisplay
    {        
        /// <summary>
        /// The message that is displayed
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        private string _message = "";

        /// <summary>
        /// Flag that determines whether the message is displayed
        /// as HTML or as text. By default message is encoded as text (true).
        /// </summary>
        public bool HtmlEncodeMessage
        {
            get { return _HtmlEncodeMessage; }
            set { _HtmlEncodeMessage = value; }
        }
        private bool _HtmlEncodeMessage = true;


        /// <summary>
        /// Holds a modelstate errors collection
        /// </summary>
        public ValidationErrorCollection DisplayErrors
        {
            get 
            {
                if (_DisplayErrors == null)
                    _DisplayErrors = new ValidationErrorCollection();
            
                return _DisplayErrors; 
            }
        }
        private ValidationErrorCollection _DisplayErrors = null;


        StringBuilder writer = new StringBuilder();
        bool visible = false;
        private ErrorDisplayTypes ErrorDisplayType = ErrorDisplayTypes.Error;

        protected void RenderTop(int width, bool center)
        {
            writer.Length = 0;
            if (center)            
                writer.AppendLine("<center>");            

            writer.Append("<div class=\"errordisplay\" ");            

            if (width != 0)
                writer.Append("style=\"width: " + width.ToString() +"px;\"");

            writer.Append(" />\r\n");
        }

        protected void RenderBottom(bool center)
        {            
            // close out the dialog
            writer.AppendLine("</div>");
            if (center)
                writer.AppendLine("</center>");
        }

        protected void RenderDisplayErrors()
        {
            if (this.DisplayErrors.Count > 0)
            {
                writer.Append("<div style=\"margin-left: 30px;\"><hr/>");
                writer.Append(this.DisplayErrors.ToHtml());
                writer.Append("</div>");
            }
        }

        /// <summary>
        /// Method 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public HtmlString Render(int width = 400, bool center = true)
        {
            if (!visible) 
                return new HtmlString(string.Empty);

            RenderTop(width, center);

            if (ErrorDisplayType == ErrorDisplayTypes.Error)
                writer.Append(" <img src=\"" + VirtualPathUtility.ToAbsolute("~/css/images/warning.gif") + "\" style=\"float: left; margin: 2px 12px;\" />");
            else
                writer.Append(" <img src=\"" + VirtualPathUtility.ToAbsolute("~/css/images/info.gif") + "\" style=\"float: left; margin: 2px 12px;\" />");

            writer.Append("<div style=\"margin-left: 40px;\">");
            
            writer.Append(  this.HtmlEncodeMessage ? HttpUtility.HtmlEncode(this.Message) : this.Message);
            RenderDisplayErrors();
            
            writer.Append("</div>");

            RenderBottom(center);

            return new HtmlString(writer.ToString());
        }


        public void ShowError(string errorMessage)
        {
            this.ErrorDisplayType = ErrorDisplayTypes.Error;
            this.Message = errorMessage;
            this.visible = true;
        }


        public void ShowMessage(string message)
        {
            this.ErrorDisplayType = ErrorDisplayTypes.Message;
            this.Message = message;
            this.visible = true;
        }

        /// <summary>
        /// Adds ModelState errors to the validationErrors
        /// </summary>
        /// <param name="modelErrors"></param>
        public void AddMessages(ModelStateDictionary modelErrors, string fieldPrefix = null)
        {
            fieldPrefix = fieldPrefix ?? string.Empty;            

            foreach(var state in modelErrors)
            {
                if ((state.Value.Errors.Count > 0))
                    this.DisplayErrors.Add(state.Value.Errors[0].ErrorMessage,fieldPrefix + state.Key);
            }
            this.visible = true;
        }

        /// <summary>
        /// Adds an existing set of Validation Errors to the DisplayErrors
        /// </summary>
        /// <param name="validationErrors"></param>
        public void AddMessages(ValidationErrorCollection validationErrors,string fieldPrefix = null)
        {
            fieldPrefix = fieldPrefix ?? string.Empty;            

            foreach (ValidationError error in validationErrors)
            {                
                this.DisplayErrors.Add(error.Message,fieldPrefix + error.ControlID);
            }
            this.visible = true;
        }

        /// <summary>
        /// Adds an individual model error
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="control"></param>
        public void AddMessage(string errorMessage, string control = null)
        {            
            DisplayErrors.Add(errorMessage,control);
            visible = true;
        }
    }

    public enum ErrorDisplayTypes
    {
        Error,
        Message
    }
}
