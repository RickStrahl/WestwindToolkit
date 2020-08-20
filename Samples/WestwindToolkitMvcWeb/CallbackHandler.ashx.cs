using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Westwind.Samples;
using Westwind.Web;

namespace WestwindToolkitMvcWeb
{
    /// <summary>
    /// Summary description for CallbackHandler
    /// </summary>
    public class CallbackHandler : Westwind.Web.CallbackHandler
    {
        [CallbackMethod(RouteUrl="quotes/{symbol}",AllowedHttpVerbs=HttpVerbs.GET | HttpVerbs.POST)]
        public StockQuote GetStockQuote(string symbol)
        {
            var stocks = new StockServer();
            var quote = stocks.GetStockQuote(symbol);
            if (quote == null)
                throw new ArgumentException("Invalid symbol passed.");

            return quote;
        }


        [CallbackMethod(RouteUrl = "test/postobject")]
        public PostObject PostObject(PostObject postObj)
        {
            if (postObj == null)
            {
                return new PostObject {Name = "Invalid - no post object received."};
            }

            return postObj;
        }

    }

    public class PostObject
    {
        public string Name { get; set;}
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }

    
}
