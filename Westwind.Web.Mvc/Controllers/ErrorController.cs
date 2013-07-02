using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Westwind.Web.Mvc.Properties;

namespace Westwind.Web.Mvc
{

    /// <summary>
    /// A generic Error Display class that can be used to display standalone 
    /// error messages with a single line of code.
    /// 
    /// Depends on a Error.cshtml view which is passed
    /// an ErrorViewModel as a parameter
    /// </summary>
    public class ErrorController : BaseController
    {
        /// <summary>
        /// Displays a generic error message
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="redirectTo"></param>
        /// <returns></returns>
        public ActionResult ShowError(string title, string message, string redirectTo = null, bool isHtml = true)
        {
            if (string.IsNullOrEmpty(message))
                message = Resources.WeAreSorryButAnUnspecifiedErrorOccurredInT;

            ErrorViewModel model = new ErrorViewModel
            {
                Message = message,
                Title = title,
                RedirectTo = redirectTo != null ? Url.Content(redirectTo) : "",
                MessageIsHtml = isHtml,
                IsMessage = false
            };

            // Explicitly point at Error View regardless of action
            return View("Error", model);
        }

        public ActionResult ShowMessage(string title, string message, string redirectTo = null, bool isHtml = true)
        {
            if (string.IsNullOrEmpty(message))
                message = Resources.WeAreSorryButAnUnspecifiedErrorOccurredInT; 

            ErrorViewModel model = new ErrorViewModel
            {
                Message = message,
                Title = title,
                RedirectTo = redirectTo != null ? Url.Content(redirectTo) : "",
                MessageIsHtml = isHtml,
                IsMessage = true
            };

            // Explicitly point at Error View regardless of action
            return View("Error", model);
        }

        /// <summary>
        /// Displays a generic error message but allows passing a view model directly for 
        /// additional flexibility
        /// </summary>
        /// <param name="errorModel"></param>
        /// <returns></returns>
        public ActionResult ShowErrorFromModel(ErrorViewModel errorModel)
        {
            return View("Error", errorModel);
        }

        /// <summary>
        /// This method allows displaying an error page you specify
        /// and pass an optional ErrorViewModel as a parameter
        /// </summary>
        /// <param name="viewPage"></param>
        /// <param name="errorModel"></param>
        /// <returns></returns>
        public ActionResult ShowErrorViewPage(string viewPage, ErrorViewModel errorModel = null)
        {
            return View(viewPage, errorModel);
        }
        
        
        public ActionResult CauseError()
        {
            ErrorController controller = null;
            controller.CauseError();  // cause exception
            // never called
            return View("Error");
        }

        /// <summary>
        /// Static method that can be called from outside of MVC requests
        /// (like in Application_Error) to display an error View.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="redirectTo"></param>
        /// <param name="IsHtml"></param>
        public static ActionResult ShowErrorPageResult(string title, string message, string redirectTo=null, bool isHtml = false)
        {
            var model = new ErrorViewModel();
            model.Title = title;
            model.Message = message;
            model.RedirectTo = redirectTo;
            model.MessageIsHtml = isHtml;
            return ShowErrorPageResult(model);
        }

        /// <summary>
        /// Static method that can be called from outside of MVC requests
        /// (like in Application_Error) to display an error View.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="redirectTo"></param>
        /// <returns></returns>
        public static ActionResult ShowMessagePageResult(string title, string message, string redirectTo = null, bool isHtml = false)
        {            
            var model = new ErrorViewModel()
            {
                Title = title,
                Message = message,
                RedirectTo = redirectTo,
                MessageIsHtml= isHtml,
                IsMessage = true
            };
                                        
            return ShowErrorPageResult(model);
        }

        /// <summary>
        /// Static method that can be called from outside of MVC requests
        /// (like in Application_Error) to display an error View.
        /// </summary>
        public static ActionResult ShowErrorPageResult(ErrorViewModel errorModel)
        {
            ErrorController controller = new ErrorController();
            return controller.ShowErrorFromModel(errorModel);
        }

        public static ActionResult ShowMessagePageResult(ErrorViewModel errorModel)
        {
            errorModel.IsMessage = true;
            ErrorController controller = new ErrorController();
            return controller.ShowErrorFromModel(errorModel);
        }

        /// <summary>
        /// Static method that can be called from outside of MVC requests
        /// (like in Application_Error) to display an error View.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="redirectTo"></param>
        /// <param name="messageIsHtml"></param>
        public static void ShowErrorPage(string title, string message, string redirectTo = null)
        {
            ErrorController controller = new ErrorController();

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("action", "ShowError");
            routeData.Values.Add("title", title);
            routeData.Values.Add("message", message);            
            routeData.Values.Add("redirectTo", redirectTo);

            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(System.Web.HttpContext.Current), routeData));
        }

        /// <summary>
        /// Static method that can be called from outside of MVC requests
        /// (like in Application_Error) to display an error View.
        /// </summary>
        public static void ShowErrorPage(ErrorViewModel errorModel)
        {
            ErrorController controller = new ErrorController();

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("action", "ShowErrorFromModel");
            routeData.Values.Add("errorModel", errorModel);

            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(System.Web.HttpContext.Current), routeData));
        }

        /// <summary>
        /// Allows you to display an arbitrary view and pass an optional
        /// ErrorViewModel for it. In short it's a shortcut way to just
        /// execute a custom view.
        /// </summary>
        /// <param name="viewPage">Path to a ViewPage</param>
        /// <param name="errorModel">Optional ErrorViewModel to pass to the view</param>
        public static void ShowErrorPageFromView(string viewPage, ErrorViewModel errorModel = null)
        {
            if (errorModel == null)
                errorModel = new ErrorViewModel();

            ErrorController controller = new ErrorController();

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("action", "ShowErrorViewPage");
            routeData.Values.Add("errorModel", errorModel);

            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(System.Web.HttpContext.Current), routeData));            
        }

    }
}