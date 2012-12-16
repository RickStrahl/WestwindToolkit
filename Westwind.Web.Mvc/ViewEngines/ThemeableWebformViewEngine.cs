namespace AspNetMvcThemeableViewEngines
{
    using System;
    using System.Web.Mvc;

    public class ThemeableWebformViewEngine : ThemeableBuildManagerViewEngine
    {
        public ThemeableWebformViewEngine()
        {
            MasterLocationFormats = new[]
                                        {
                                            "~/Views/Themes/{2}/{1}/{0}.master",
                                            "~/Views/Themes/{2}/Shared/{0}.master",

                                            "~/Views/{1}/{0}.master",
                                            "~/Views/Shared/{0}.master"
                                        };

            AreaMasterLocationFormats = new[]
                                            {
                                                "~/Areas/{2}/Views/themes/{3}/{1}/{0}.master",
                                                "~/Areas/{2}/Views/themes/{3}/Shared/{0}.master",

                                                "~/Areas/{2}/Views/{1}/{0}.master",
                                                "~/Areas/{2}/Views/Shared/{0}.master"
                                            };

            ViewLocationFormats = new[]
                                      {
                                          "~/Views/Themes/{2}/{1}/{0}.aspx",
                                          "~/Views/Themes/{2}/{1}/{0}.ascx",
                                          "~/Views/Themes/{2}/Shared/{0}.aspx",
                                          "~/Views/Themes/{2}/Shared/{0}.ascx",

                                          "~/Views/{1}/{0}.aspx",
                                          "~/Views/{1}/{0}.ascx",
                                          "~/Views/Shared/{0}.aspx",
                                          "~/Views/Shared/{0}.ascx"
                                      };

            AreaViewLocationFormats = new[]
                                          {
                                              "~/Areas/{2}/Views/themes/{3}/{1}/{0}.aspx",
                                              "~/Areas/{2}/Views/themes/{3}/{1}/{0}.ascx",
                                              "~/Areas/{2}/Views/themes/{3}/Shared/{0}.aspx",
                                              "~/Areas/{2}/Views/themes/{3}/Shared/{0}.ascx",

                                              "~/Areas/{2}/Views/{1}/{0}.aspx",
                                              "~/Areas/{2}/Views/{1}/{0}.ascx",
                                              "~/Areas/{2}/Views/Shared/{0}.aspx",
                                              "~/Areas/{2}/Views/Shared/{0}.ascx"
                                          };

            PartialViewLocationFormats = ViewLocationFormats;
            AreaPartialViewLocationFormats = AreaViewLocationFormats;
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return new WebFormView(controllerContext, partialPath, null);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return new WebFormView(controllerContext, viewPath, masterPath);
        }

        protected override bool IsValidCompiledType(ControllerContext controllerContext, string virtualPath, Type compiledType)
        {
            return typeof(ViewPage).IsAssignableFrom(compiledType) || typeof(ViewUserControl).IsAssignableFrom(compiledType);
        }
    }
}