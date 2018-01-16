namespace Core
{
    using Core.IO;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class AccountState
    {
        private static string DefaultMsgConfig = Utility.RenderHashJson(new object[] { "LastReceivedTime", new DateTime(0x7d0, 1, 1) });
        private Hashtable m_Config = new Hashtable();
        private DateTime m_LastAccessTime = DateTime.Now;
        private Hashtable m_Sessions = new Hashtable();
        private string m_User;

        public AccountState(string user)
        {
            this.m_User = user;
            this.LoadConfig();
        }

        public Hashtable GetConfig(string type)
        {
            return (this.m_Config[type.ToUpper()] as Hashtable);
        }

        private void LoadConfig()
        {
            this.LoadConfig("message.conf", DefaultMsgConfig);
        }

        private void LoadConfig(string type, string def)
        {
            Hashtable hashtable;
            try
            {
                using (Stream stream = Core.IO.File.Open(string.Format("/{0}/Config/{1}", this.m_User, type), FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    string str = Encoding.UTF8.GetString(buffer);
                    if (string.IsNullOrEmpty(str))
                    {
                        str = def;
                    }
                    hashtable = Utility.ParseJson(str) as Hashtable;
                    this.m_Config[type.ToUpper()] = hashtable;
                }
            }
            catch
            {
                hashtable = Utility.ParseJson(def) as Hashtable;
                this.m_Config[type.ToUpper()] = hashtable;
            }
        }

        public void NewSession(string sessionId)
        {
            AccountSession session = null;
            lock (this)
            {
                if (!this.m_Sessions.ContainsKey(sessionId))
                {
                    this.m_Sessions[sessionId] = session = new AccountSession(this.m_User, sessionId);
                }
                session = this.m_Sessions[sessionId] as AccountSession;
                ServerImpl.Instance.WriteLog(string.Format("New Session:SessionID = \"{0}\", UserName='{1}'", sessionId, this.m_User));
            }
            if (session != null)
            {
                this.SendUnreadMessage(session);
                SessionManagement.Instance.Insert(session);
            }
            AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(this.UserName);
            SessionManagement.Instance.Send("UserStateChanged", Utility.RenderHashJson(new object[] { "User", userInfo.Name.ToUpper(), "State", "Online", "Details", userInfo.DetailsJson }));
        }

        public bool Receive(string sessionId, ResponsesListener listener)
        {
            AccountSession session = null;
            bool flag = false;
            lock (this)
            {
                if (!this.m_Sessions.ContainsKey(sessionId))
                {
                    this.m_Sessions[sessionId] = new AccountSession(this.m_User, sessionId);
                    flag = true;
                }
                this.m_LastAccessTime = DateTime.Now;
                session = this.m_Sessions[sessionId] as AccountSession;
            }
            if (flag)
            {
                ServerImpl.Instance.WriteLog(string.Format("Reset Session:SessionID = \"{0}\", UserName='{1}'", sessionId, this.m_User));
                session.Send("GLOBAL:SessionReset", "null");
            }
            if (session != null)
            {
                SessionManagement.Instance.Insert(session);
            }
            return session.Receive(listener);
        }

        public void Remove(string sessionId)
        {
            lock (this)
            {
                (this.m_Sessions[sessionId] as AccountSession).SendCache();
                this.m_Sessions.Remove(sessionId);
                if (this.m_Sessions.Count == 0)
                {
                    this.SaveConfig();
                }
            }
        }

        private void SaveConfig()
        {
            Hashtable config = this.GetConfig("message.conf");
            lock (config)
            {
                DateTime time = (DateTime) config["LastReceivedTime"];
                if (time > this.m_LastAccessTime)
                {
                    config["LastReceivedTime"] = this.m_LastAccessTime;
                }
            }
            this.SaveConfig("message.conf");
        }

        private void SaveConfig(string type)
        {
            try
            {
                using (Stream stream = Core.IO.File.Open(string.Format("/{0}/Config/{1}", this.m_User, type), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Hashtable hashtable = this.m_Config[type.ToUpper()] as Hashtable;
                    byte[] bytes = Encoding.UTF8.GetBytes(Utility.RenderJson(hashtable));
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch
            {
            }
        }

        public void Send(string commandId, string data)
        {
            List<string> list = new List<string>();
            lock (this)
            {
                foreach (DictionaryEntry entry in this.m_Sessions)
                {
                    (entry.Value as AccountSession).Send(commandId, data);
                }
            }
        }

        private void SendUnreadMessage(AccountSession session)
        {
            DateTime lastAccessTime;
            AccountState state;
            AccountState state2;
            lock ((state2 = state = this))
            {
                lastAccessTime = (DateTime) this.GetConfig("MESSAGE.CONF")["LastReceivedTime"];
                if (lastAccessTime > this.m_LastAccessTime)
                {
                    lastAccessTime = this.m_LastAccessTime;
                }
            }
            List<Message> list = MessageImpl.Instance.Find(this.m_User, "*", new DateTime?(lastAccessTime));
            string data = string.Empty;
            if (list.Count > 0)
            {
                data = Utility.RenderHashJson(new object[] { "Peer", "*", "Messages", list });
                Hashtable config = this.GetConfig("MESSAGE.CONF");
                lock (config)
                {
                    foreach (Message message in list)
                    {
                        DateTime time2 = (DateTime) config["LastReceivedTime"];
                        if (time2 < message.CreatedTime)
                        {
                            config["LastReceivedTime"] = message.CreatedTime;
                        }
                    }
                }
                lock ((state2 = state = this))
                {
                    this.SaveConfig();
                }
            }
            else
            {
                data = Utility.RenderHashJson(new object[] { "Peer", "*", "Messages", JsonText.EmptyArray });
            }
            session.Send("GLOBAL:IM_MESSAGE_NOTIFY", data);
        }

        public void Timeout(string sessionId)
        {
            lock (this)
            {
                (this.m_Sessions[sessionId] as AccountSession).SendCache();
            }
        }

        public bool IsOnline
        {
            get
            {
                return (this.m_Sessions.Count > 0);
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return this.m_LastAccessTime;
            }
        }

        public DateTime LastReceivedTime
        {
            get
            {
                DateTime lastAccessTime = (DateTime) (this.m_Config["MESSAGE.CONF"] as Hashtable)["LastReceivedTime"];
                if (lastAccessTime > this.m_LastAccessTime)
                {
                    lastAccessTime = this.m_LastAccessTime;
                }
                return lastAccessTime;
            }
        }

        public string UserName
        {
            get
            {
                return this.m_User;
            }
        }
    }
}

