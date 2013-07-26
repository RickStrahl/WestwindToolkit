using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Westwind.Web.Mvc;

namespace WestwindToolkitMvcWeb.Controllers
{
    public class UtilitiesApiController : ApiController
    {
        [HttpGet]
        public string ViewRenderer()
        {
            var renderer = new ViewRenderer();
            return renderer.RenderView("~/Views/Utilities/ScriptVariables.cshtml", null);
        }
    }
}
