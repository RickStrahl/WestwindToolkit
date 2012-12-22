using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;


namespace Westwind.Web.Mvc
{

    /// <summary>
    /// Base Controller implementation that holds UserState,
    /// ErrorDisplay objects that are preinitialized.
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>
        /// Contains User state information retrieved from the authentication system
        /// </summary>
        protected UserState UserState = new UserState();

        /// <summary>
        /// ErrorDisplay control that holds page level error information
        /// </summary>
        protected ErrorDisplay ErrorDisplay = new ErrorDisplay();


        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            // Grab the user's login information from FormsAuth            
            if (this.User.Identity != null && this.User.Identity is FormsIdentity)
                this.UserState = UserState.CreateFromString(((FormsIdentity)this.User.Identity).Ticket.UserData);
            else
                this.UserState = new UserState();
                        
            // have to explicitly add this so Master can see untyped value
            ViewBag.UserSate = this.UserState;
            ViewBag.ErrorDisplay = this.ErrorDisplay;

            // custom views should also add these as properties
        }

   
        /// <summary>
        /// Creates or updates a ViewModel and adds values to some of the
        /// stock properties of the Controller. 
        /// 
        /// This default implementation initializes the ErrorDisplay and UserState
        /// objects after creation.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>        
        public virtual TViewModel CreateViewModel<TViewModel>()
             where TViewModel : class, new()
        {
            var model = new TViewModel();
            
            if (model is BaseViewModel)
            {
                BaseViewModel baseModel = model as BaseViewModel;
                baseModel.ErrorDisplay = ErrorDisplay;
                baseModel.UserState = UserState;                
            }

            return model;
        }

        /// <summary>
        /// Updates a ViewModel and adds values to some of the
        /// stock properties of the Controller. 
        /// 
        /// This default implementation initializes the ErrorDisplay and UserState
        /// objects after creation.
        /// </summary>
        public virtual void InitializeViewModel(BaseViewModel model)
        {
            if (model == null)
                return;

            BaseViewModel baseModel = model as BaseViewModel;
            baseModel.ErrorDisplay = ErrorDisplay;
            baseModel.UserState = UserState;                
        }


        /// <summary>
        /// Allow external initialization of this controller by explicitly
        /// passing in a request context
        /// </summary>
        /// <param name="requestContext"></param>
        public void InitializeExplicit(RequestContext requestContext)
        {
            Initialize(requestContext);
        }


        /// <summary>
        /// Displays a self contained error page without redirecting.
        /// Depends on ErrorController.ShowError() to exist
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="redirectTo"></param>
        /// <returns></returns>
        protected internal ActionResult DisplayErrorPage(string title, string message, string redirectTo = null, bool isHtml = true)
        {
            ErrorController controller = new ErrorController();            
            controller.InitializeExplicit(this.ControllerContext.RequestContext);
            return controller.ShowError(title, message, redirectTo,isHtml);
        }
    }
}