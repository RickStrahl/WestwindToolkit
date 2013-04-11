using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;


namespace Westwind.Samples
{

    /// <summary>
    /// Class that retrieves stock data from Yahoo's standard site Urls in form
    /// of .csv style comma delimited lists. This class republishes this data in
    /// object format for retrieving single quotes, groups of quotes
    /// 
    /// For info on single quote parameters:
    /// http://www.gummy-stuff.org/Yahoo-data.htm
    /// </summary>
public class StockServer
{

    private const string STR_YAHOOFINANCE_STOCK_BASEURL = "http://download.finance.yahoo.com/d/quotes.csv?s=";

    /// <summary>
    /// s - Symbol l1 - last trade d1 - last trade date t1 - last trade time c1 - change 
    /// o - open price h - day's high g - day's low n - name p2 - change in % 
    /// j1 - Market Cap r - P/E  y - Dividend Yield
    /// </summary>
    private const string STR_STOCK_FORMATTING ="&f=sl1d1t1c1ohgnp2j1ry";

    /// <summary>
    /// Retrieves an individual Stock quote based on a ticker symbol
    /// </summary>
    /// <param name="symbol">Stock Ticker Symbol (ie. MSFT, INTC, YUM)</param>
    /// <returns>Quote object or null</returns>
    public StockQuote GetStockQuote(string symbol)
    {
        StockQuote quote = null;
        WebClient http = new WebClient();
        string quoteString = http.DownloadString(STR_YAHOOFINANCE_STOCK_BASEURL + symbol +
                                                    STR_STOCK_FORMATTING);
        quote = this.ParseStockQuote(quoteString);
        return quote;
    }

    /// <summary>
    /// Retrieves a set of Stockquote objects for a given number of stock symbols.
    /// </summary>
    /// <param name="symbols">A string array of Stock Ticker Symbols</param>
    /// <returns></returns>
    public StockQuote[] GetStockQuotes(string[] symbols)
    {
        string url = STR_YAHOOFINANCE_STOCK_BASEURL;

        WebClient http = new WebClient();

        // Stocks are concatenated with commas
        foreach (string symbol in symbols)
        {
            url += symbol + ",";
        }
        url = url.TrimEnd(',');
        url += STR_STOCK_FORMATTING;

        string rawQuoteString = http.DownloadString(url);

        // Break up into each individual quote
        string[] quoteStrings = rawQuoteString.Split(new string[1] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

        // Break up each quote CSV component and parse it into object
        StockQuote[] quotes = new StockQuote[quoteStrings.Length];

        for (int x = 0; x < quoteStrings.Length; x++)
        {
            quotes[x] = this.ParseStockQuote(quoteStrings[x]);
        }

        return quotes;
    }


    /// <summary>
    /// Parses an individual QUote String
    /// </summary>
    /// <param name="QuoteString"></param>
    /// <returns></returns>
    private StockQuote ParseStockQuote(string QuoteString)
    {
        // "MSFT",27.17,"3/10/2006","4:00pm",+0.17,27.04,27.22,26.88,"MICROSOFT CP"
        //   0      1       2          3        4    5     6     7     8   
        StockQuote Quote = new StockQuote();

        string[] Details = QuoteString.Split(',');

        Quote.Symbol = Details[0].Replace("\"", "");
        Quote.Company = Details[8].Replace("\"", "").Replace("\n", "").Replace("\r", "").Trim();

        string Work = Details[1];
        decimal WorkNumber = 0M;
        decimal.TryParse(Work, out WorkNumber);
        Quote.LastPrice = WorkNumber;

        Work = Details[4];
        WorkNumber = 0.00M;
        decimal.TryParse(Work, out WorkNumber);
        Quote.NetChange = WorkNumber;

        // Percent Change "+0.57%"
        Work = Details[9].Trim(new char[4] { '%','+',' ','"'} );        
        WorkNumber = 0.00M;
        decimal.TryParse(Work, out WorkNumber);
        Quote.NetChangePercent = WorkNumber;

        Work = Details[10]; 
        WorkNumber = 0.00M;
        decimal.TryParse(Work, out WorkNumber);
        Quote.MarketCap = WorkNumber;

        Work = Details[11];
        WorkNumber = 0.00M;
        decimal.TryParse(Work, out WorkNumber);
        Quote.ProfitEarnings = WorkNumber;

        Work = Details[12];
        WorkNumber = 0.00M;
        decimal.TryParse(Work, out WorkNumber);
        Quote.DividendYield = WorkNumber;
        
        Work = Details[2] + " " + Details[3];
        Work = Work.Replace("\"", "");
        DateTime WorkDate = DateTime.UtcNow;
        DateTime.TryParse(Work, out WorkDate); // CultureInfo.GetCultureInfo("en-us"), DateTimeStyles.AssumeLocal, out WorkDate);

        if (WorkDate < StockQuote.DATE_EMPTY)
            return null;
        else
            Quote.LastQuoteTime = WorkDate;

        return Quote;
    }
}

   

public class StockQuote
{
    public static DateTime DATE_EMPTY = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public string Symbol {get; set; }
    public string Company {get; set; }
    public decimal LastPrice {get; set; }
    public decimal NetChange { get; set; }
    public decimal NetChangePercent { get; set; }
    public decimal MarketCap { get; set; }
    public decimal ProfitEarnings { get; set; }
    public decimal DividendYield { get; set; }
        
    public DateTime LastQuoteTime
    {
        get { return _LastQuoteTime; }
        set { _LastQuoteTime = value; }
    }
    private DateTime _LastQuoteTime = DATE_EMPTY;
    
    public string LastQuoteTimeString
    {
        get { return LastQuoteTime.ToString("MMM d, h:mmtt"); }        
    }
}
}