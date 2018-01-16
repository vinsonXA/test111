namespace Core
{
    using System;
    using System.Web;

    public abstract class CommandHandler
    {
        private HttpContext _context = null;
        private string _data;
        private string _id;
        private string _sessionId;

        public CommandHandler(HttpContext context, string sessionId, string id, string data)
        {
            this._context = context;
            this._data = data;
            this._id = id;
            this._sessionId = sessionId;
        }

        public abstract string Process();
        public abstract void Process(object data);

        public string CommandID
        {
            get
            {
                return this._id;
            }
        }

        public HttpContext Context
        {
            get
            {
                return this._context;
            }
        }

        public string Data
        {
            get
            {
                return this._data;
            }
        }

        public string SessionID
        {
            get
            {
                return this._sessionId;
            }
        }

        public string UserName
        {
            get
            {
                return ServerImpl.Instance.GetUserName(this._context);
            }
        }
    }
}

