using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Routing;

namespace Westwind.Web.Mvc
{
    public class ErrorViewModel
    {
        public string Title = string.Empty;
        public string Message = string.Empty;
        public string RedirectTo = string.Empty;
        public int RedirectToTimeout = 10; // seconds
        public bool MessageIsHtml = false;
        public bool IsMessage = false;
    }
}
