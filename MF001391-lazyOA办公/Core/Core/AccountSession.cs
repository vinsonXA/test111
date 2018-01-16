namespace Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class AccountSession
    {
        private List<CommandResponse> _cache = new List<CommandResponse>();
        private DateTime _createdTime = DateTime.Now;
        private ResponsesListener _listener = null;
        private object _lock = new object();
        private LinkedListNode<ClearSessionNode> _node = null;
        private string _sessionId = string.Empty;

        public AccountSession(string username, string id)
        {
            this._sessionId = id;
            this._node = new LinkedListNode<ClearSessionNode>(new ClearSessionNode(username, id, this));
        }

        public bool Receive(ResponsesListener listener)
        {
            lock (this._lock)
            {
                this._createdTime = DateTime.Now;
                if (this._cache.Count > 0)
                {
                    string content = Utility.RenderHashJson(new object[] { "IsSucceed", true, "Responses", this._cache });
                    listener.Cache(content);
                    this._cache.Clear();
                    return true;
                }
                this._listener = listener;
                return false;
            }
        }

        public void Send(string commandId, string data)
        {
            lock (this._lock)
            {
                this._cache.Add(new CommandResponse(commandId, data));
                this.SendCache();
            }
        }

        public void SendCache()
        {
            lock (this._lock)
            {
                if (this._listener != null)
                {
                    try
                    {
                        string content = Utility.RenderHashJson(new object[] { "IsSucceed", true, "Responses", this._cache });
                        this._listener.Cache(content);
                        this._cache.Clear();
                    }
                    finally
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this._listener.Complete));
                        this._listener = null;
                    }
                }
            }
        }

        public DateTime LatestAccessTime
        {
            get
            {
                return this._createdTime;
            }
        }

        public LinkedListNode<ClearSessionNode> ListNode
        {
            get
            {
                return this._node;
            }
        }

        public string SessionID
        {
            get
            {
                return this._sessionId;
            }
        }
    }
}

