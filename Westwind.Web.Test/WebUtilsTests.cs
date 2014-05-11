using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Web.Test
{
    [TestClass]
    public class WebUtilsTests
    {
        [TestMethod]
        public void SetUserLocaleTest()
        {
            

            var curculture = Thread.CurrentThread.CurrentCulture;
            var curuiculture = Thread.CurrentThread.CurrentUICulture;

            WebUtils.SetUserLocale("de-de", "de-de");
            var culture = Thread.CurrentThread.CurrentCulture;
            var uiCulture = Thread.CurrentThread.CurrentUICulture;

            Console.WriteLine(culture.IetfLanguageTag);
            Assert.IsTrue(culture.IetfLanguageTag == "de-DE");

            curculture = Thread.CurrentThread.CurrentCulture;
            curuiculture = Thread.CurrentThread.CurrentUICulture;

            WebUtils.SetUserLocale("de-de", "de-de",allowedLocales: "en,de");
            culture = Thread.CurrentThread.CurrentCulture;
            uiCulture = Thread.CurrentThread.CurrentUICulture;

            Console.WriteLine(culture.IetfLanguageTag);
            Assert.IsTrue(culture.IetfLanguageTag == "de-DE", "Invalid: " + culture.IetfLanguageTag);

            // reset 
            Thread.CurrentThread.CurrentUICulture = curuiculture;
            Thread.CurrentThread.CurrentCulture = curculture;

            WebUtils.SetUserLocale("es-MX", "es-MX");
            culture = Thread.CurrentThread.CurrentCulture;
            uiCulture = Thread.CurrentThread.CurrentUICulture;

            Console.WriteLine(culture.IetfLanguageTag);
            Assert.IsTrue(culture.IetfLanguageTag == "es-MX");

            // reset 
            Thread.CurrentThread.CurrentUICulture = curuiculture;
            Thread.CurrentThread.CurrentCulture = curculture;


            // this should fail to set to es-mx but leave at default language
            WebUtils.SetUserLocale("es-MX", "es-MX",allowedLocales: "en,de,fr");
            culture = Thread.CurrentThread.CurrentCulture;
            uiCulture = Thread.CurrentThread.CurrentUICulture;

            // should ALWAYS be writable even if we return the default locale
            Console.WriteLine(culture.NumberFormat.CurrencySymbol);
            culture.NumberFormat.CurrencySymbol = "#";
            Console.WriteLine(culture.NumberFormat.CurrencySymbol);

            Console.WriteLine(culture.IetfLanguageTag);
            Assert.IsTrue(culture.IetfLanguageTag == curculture.IetfLanguageTag);

            // reset 
            Thread.CurrentThread.CurrentUICulture = curuiculture;
            Thread.CurrentThread.CurrentCulture = curculture;

            // this should fail to set to es-mx but leave at default
            WebUtils.SetUserLocale("fr", setUiCulture: true, allowedLocales: "en,de,fr");
            culture = Thread.CurrentThread.CurrentCulture;
            uiCulture = Thread.CurrentThread.CurrentUICulture;

            Console.WriteLine(culture.IetfLanguageTag);
            Assert.IsTrue(culture.IetfLanguageTag == "fr");

            Console.WriteLine(uiCulture.NumberFormat.CurrencySymbol);
            uiCulture.NumberFormat.CurrencySymbol = "$";
            Console.WriteLine(uiCulture.NumberFormat.CurrencySymbol);

            // reset 
            Thread.CurrentThread.CurrentUICulture = curuiculture;
            Thread.CurrentThread.CurrentCulture = curculture;

        }
    }
}
