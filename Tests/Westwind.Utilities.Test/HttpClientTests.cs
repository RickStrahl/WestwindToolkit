using System;
using System.Collections.Generic;
using Westwind.Utilities.InternetTools;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.InternetTools
{
    [TestClass]
    public class HttpClientTests
    {
        [TestMethod]
        public void HttpTimingsTest()
        {
            var client = new HttpClient();

            var html = client.DownloadString("http://weblog.west-wind.com/posts/2015/Jan/06/Using-Cordova-and-Visual-Studio-to-build-iOS-Mobile-Apps");

            Console.WriteLine(client.WebResponse.ContentLength);
            Console.WriteLine(client.HttpTimings.StartedTime);
            Console.WriteLine("First Byte: " + client.HttpTimings.TimeToFirstByteMs);
            Console.WriteLine("Last Byte: " + client.HttpTimings.TimeToLastByteMs);            


        }
    }
}
