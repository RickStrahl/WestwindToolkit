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

    }

    
}