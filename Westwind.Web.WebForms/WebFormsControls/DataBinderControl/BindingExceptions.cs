using System;
using System.Collections.Generic;
using System.Text;

namespace Westwind.Web.Controls
{

    /// <summary>
    /// Exception thrown when a required field is not filled in. Used internally
    /// for catching these errors and rendering the error.
    /// </summary>
    public class RequiredFieldException : ApplicationException
    {
        public RequiredFieldException() : base() { }
        public RequiredFieldException(string Message) : base(Message) { }
    }

    /// <summary>
    /// Exception thrown when a BindingError is encountered
    /// </summary>
    public class BindingErrorException : ApplicationException
    {
        public BindingErrorException() : base() { }
        public BindingErrorException(string Message) : base(Message) { }
        public BindingErrorException(string Message, Exception originalException) : base(Message,originalException) { }
    }

    /// <summary>
    /// An exception fired if a validation error occurs in DataBinding
    /// </summary>
    public class ValidationErrorException : BindingErrorException
    {
        public ValidationErrorException() : base() { }
        public ValidationErrorException(string Message) : base(Message) { }
        public ValidationErrorException(string Message, Exception originalException) : base(Message,originalException) { }
    }

    
}
