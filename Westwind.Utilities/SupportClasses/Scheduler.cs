using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace Westwind.Utilities
{
    /// <summary>
    /// A generic scheduling service that runs on a background
    /// thread and fires events in a given check frequency.
    /// 
    /// 
    /// </summary>
    public class Scheduler : IDisposable
    {
        /// <summary>
        /// Determines the status  the Scheduler
        /// </summary>        
        public bool Cancelled
        {
            get { return _Cancelled; }
            private set { _Cancelled = value; }
        }
        private bool _Cancelled = true;

        /// <summary>
        /// The frequency in how often the main method is executed.
        /// Given in milli-seconds.
        /// </summary>
        public int CheckFrequency
        {
            get { return _CheckFrequency; }
            set { _CheckFrequency = value; }
        }
        private int _CheckFrequency = 60000;

        /// <summary>
        /// Optional URL that is pinged occasionally to
        /// ensure the server stays alive.
        /// 
        /// If empty hits root web page (~/yourapp/)
        /// </summary>
        public string WebServerPingUrl
        {
            get { return _WebServerPingUrl; }
            set { _WebServerPingUrl = value; }
        }
        private string _WebServerPingUrl = "";


        /// <summary>
        /// Event that is fired when
        /// </summary>
        public event EventHandler ExecuteScheduledEvent;

        AutoResetEvent _WaitHandle = new AutoResetEvent(false);

        /// <summary>
        ///  Internal property used for cross thread locking
        /// </summary>
        object _SyncLock = new Object();

        /// <summary>
        /// Memory based queue that contains items and allows
        /// retrieval of items.
        /// 
        /// Note memory based! This means if app crashses
        /// or is shut down messages might get lost.
        /// 
        /// If message persistence is important your scheduling store
        /// should be a database. You can use the QueueMessageManager
        /// object for example.
        /// </summary>    
        public virtual Queue<object> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }
        private Queue<object> _Items = new Queue<object>();


        /// <summary>
        /// Starts the background thread processing       
        /// </summary>
        /// <param name="CheckFrequency">Frequency that checks are performed in seconds</param>
        public void Start(int checkFrequency)
        {
            // Ensure that any waiting instances are shut down
            //this.WaitHandle.Set();

            this.CheckFrequency = checkFrequency;
            this.Cancelled = false;

            Thread t = new Thread(Run);
            t.Start();
        }
        /// <summary>
        /// Starts the background Thread processing
        /// </summary>
        public void Start()
        {
            this.Start(this.CheckFrequency);
        }

        /// <summary>
        /// Causes the processing to stop. If the operation is still
        /// active it will stop after the current message processing
        /// completes
        /// </summary>
        public void Stop()
        {
            lock (this._SyncLock)
            {
                if (Cancelled)
                    return;

                this.Cancelled = true;
                this._WaitHandle.Set();
            }
        }

        /// <summary>
        /// Runs the actual processing loop by checking the mail box
        /// </summary>
        private void Run()
        {
            // Start out waiting for the timeout period defined 
            // on the scheduler
            this._WaitHandle.WaitOne(this.CheckFrequency, true);

            while (!Cancelled)
            {
                try
                {
                    // Call whatever logic is attached to the scheduler
                    this.OnExecuteScheduledEvent();
                }
                // always eat the exception and notify listener
                catch (Exception ex)
                {
                    this.OnError(ex);
                }

                // If execution caused cancelling we want exit now
                if (this.Cancelled)
                    break;

                // if a keep alive ping is required fire it
                if (!string.IsNullOrEmpty(this.WebServerPingUrl))
                    this.PingServer();

                // Wait for the specified time out
                this._WaitHandle.WaitOne(this.CheckFrequency, true);
            }
        }

        /// <summary>
        /// Handles a scheduled operation. Checks to see if an event handler
        /// is set up and if so calls it. 
        /// 
        /// This method can also be overrriden in a subclass to implemnent
        /// custom functionality.
        /// </summary>
        protected virtual void OnExecuteScheduledEvent()
        {
            if (this.ExecuteScheduledEvent != null)
                this.ExecuteScheduledEvent(this, EventArgs.Empty);
        }

        /// <summary>
        /// This method is called if an error occurs during processing
        /// of the OnExecuteScheduledEvent request
        /// 
        /// Override this method in your own implementation to provide
        /// for error logging or other handling of an error that occurred
        /// in processing.
        /// 
        /// Ideally this shouldn't be necessary - your OnexecuteScheduledEvent
        /// code should handle any errors internally and provide for its own 
        /// logging mechanism but this is here as an additional point of
        /// control.
        /// </summary>
        /// <param name="ex">Exception occurred during item execution</param>
        protected virtual void OnError(Exception ex)
        {

        }

        /// <summary>
        /// Adds an item to the queue.
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(object item)
        {
            lock (this._SyncLock)
            {
                this.Items.Enqueue(item);
            }
        }

        public void AddItem(SchedulerItem item)
        {
            this.AddItem(item);
        }

        public void AddItem(string textData, string type)
        {
            SchedulerItem item = new SchedulerItem();
            item.TextData = textData;
            item.Type = type;
            this.AddItem(item as object);
        }

        public void AddItem(byte[] data, string type)
        {
            SchedulerItem item = new SchedulerItem();
            item.Data = data;
            item.Type = type;
            this.AddItem(item as object);
        }

        /// <summary>
        /// Returns the next queued item or null on failure.
        /// </summary>
        /// <returns></returns>
        public object GetNextItem()
        {
            lock (this._SyncLock)
            {
                if (this.Items.Count > 0)
                    return this.Items.Dequeue();
            }

            return null;
        }

        /// <summary>
        /// Optional routine that pings a Url on the server
        /// to keep the server alive. 
        /// 
        /// Use this to avoid IIS shutting down your AppPools
        /// </summary>
        public void PingServer(string url = null)
        {
            if (string.IsNullOrEmpty(url))
                url = this.WebServerPingUrl;

            //if (Url.StartsWith("~") && HttpContext.Current != null)
            //    Url = wwUtils.ResolveUrl(Url);

            try
            {
                WebClient http = new WebClient();
                string Result = http.DownloadString(url);
            }
            catch {}
        }


        #region IDisposable Members

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }

    /// <summary>
    /// A simple item wrapper that allows separating items
    /// by type.
    /// </summary>
    public class SchedulerItem
    {
        /// <summary>
        /// Allows identifying items by type
        /// </summary>
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        private string _Type = "";

        /// <summary>
        /// Any text data you want to submit
        /// </summary>
        public string TextData
        {
            get { return _TextData; }
            set { _TextData = value; }
        }
        private string _TextData = "";

        /// <summary>
        /// Any binary data you want to submit
        /// </summary>
        public byte[] Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
        private byte[] _Data = null;


        /// <summary>
        /// The initial date when the item was
        /// created or submitted.
        /// </summary>
        public DateTime Entered
        {
            get { return _Entered; }
            set { _Entered = value; }
        }
        private DateTime _Entered = DateTime.UtcNow;
    }

}