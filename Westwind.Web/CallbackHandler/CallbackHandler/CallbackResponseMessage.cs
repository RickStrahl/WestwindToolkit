namespace Westwind.Web
{
    /// <summary>
    /// A generic Callback/Service Response message object
    /// </summary>
    public class CallbackResponseMessage
    {
        public CallbackResponseMessage()
        {
            message = string.Empty;
        }
        public CallbackResponseMessage(string msg)
        {
            message = msg;
        }

        public bool isError { get; set; }
        public int statusCode { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public object resultData { get; set; }
        public string detail { get; set; }
        public string source { get; set; }
    }
}