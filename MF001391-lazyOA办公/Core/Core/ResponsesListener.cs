namespace Core
{
    using System;
    using System.Threading;
    using System.Web;

    public class ResponsesListener : IAsyncResult
    {
        private DateTime _createTime;
        private string _sessionId;
        private AsyncCallback m_AsyncCallback = null;
        private string m_Cache = "";
        private object m_Data = null;
        private bool m_IsCompleted = false;

        public ResponsesListener(string sessionId, AsyncCallback callback, object extraData)
        {
            this.m_Data = extraData;
            this.m_AsyncCallback = callback;
            this._createTime = DateTime.Now;
            this._sessionId = sessionId;
        }

        public void Cache(object content)
        {
            this.m_Cache = content.ToString();
        }

        public void Complete(object data)
        {
            this.m_AsyncCallback(this);
            this.m_IsCompleted = true;
        }

        public void Send(HttpContext context)
        {
            ServerImpl.Instance.WriteLog(string.Format("Send Responses:SessionID = \"{0}\", Content='{1}'", this._sessionId, this.m_Cache.ToString()));
            context.Response.Write(this.m_Cache.ToString());
        }

        public DateTime CreateTime
        {
            get
            {
                return this._createTime;
            }
        }

        public string SessionID
        {
            get
            {
                return this._sessionId;
            }
        }

        object IAsyncResult.AsyncState
        {
            get
            {
                return this.m_Data;
            }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get
            {
                return null;
            }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        bool IAsyncResult.IsCompleted
        {
            get
            {
                return this.m_IsCompleted;
            }
        }
    }
}

