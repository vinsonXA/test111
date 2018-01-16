namespace Core
{
    using System;

    public class ClearSessionNode
    {
        private AccountSession _session;
        private string _sessionID;
        private string _userName;
        public DateTime InsertTime;

        public ClearSessionNode(string username, string sessionid, AccountSession session)
        {
            this._userName = username;
            this._sessionID = sessionid;
            this._session = session;
        }

        public AccountSession Session
        {
            get
            {
                return this._session;
            }
        }

        public string SessionID
        {
            get
            {
                return this._sessionID;
            }
        }

        public string UserName
        {
            get
            {
                return this._userName;
            }
        }
    }
}

