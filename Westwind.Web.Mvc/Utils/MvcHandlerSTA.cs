
// http://forums.asp.net/t/1302406.aspx/1

using System.Web;
using System.Web.UI;
using System.Web.Routing;
using System;
using System.Web.Mvc;

namespace Westwind.Web.Mvc
{

    public class MvcRouteHandlerSTA : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestContext)
        {
            return new MvcHandlerSTA(requestContext);
        }
    }


    public class MvcHandlerSTA : Page, IHttpAsyncHandler
    {

        public MvcHandlerSTA(RequestContext requestContext)
        {
            if (requestContext == null)
                throw new ArgumentNullException("requestContext is invalid");

            this.RequestContext = requestContext;
        }
        private ControllerBuilder _controllerBuilder;

        internal ControllerBuilder ControllerBuilder
        {
            get { return this._controllerBuilder ?? (this._controllerBuilder = ControllerBuilder.Current); }
        }

        public RequestContext RequestContext
        {
            get;
            set;
        }

        #region Make it STA

        protected override void OnInit(EventArgs e)
        {

            string requiredString = this.RequestContext.RouteData.GetRequiredString("controller");

            IControllerFactory controllerFactory = this.ControllerBuilder.GetControllerFactory();

            IController controller = controllerFactory.CreateController(this.RequestContext, requiredString);

            if (controller == null)
                throw new InvalidOperationException("Could not find controller: " + requiredString);

            try
            {
                ControllerBase controllerInst = controller as ControllerBase;
                ControllerContext controllerContext = new ControllerContext(RequestContext, controllerInst);
                controller.Execute(controllerContext.RequestContext);
            }
            finally
            {
                controllerFactory.ReleaseController(controller);
            }

            this.Context.ApplicationInstance.CompleteRequest();
        }

        public override void ProcessRequest(HttpContext httpContext)
        {

            throw new NotSupportedException("This should not get called for an STA");

        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {

            return this.AspCompatBeginProcessRequest(context, cb, extraData);

        }

        public void EndProcessRequest(IAsyncResult result)
        {

            this.AspCompatEndProcessRequest(result);

        }

        #endregion

        void IHttpHandler.ProcessRequest(HttpContext httpContext)
        {

            this.ProcessRequest(httpContext);

        }

    }
}