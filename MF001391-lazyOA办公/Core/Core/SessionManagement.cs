namespace Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    public class SessionManagement
    {
        private Hashtable m_Accounts = new Hashtable();
        private LinkedList<ClearSessionNode> m_ClearSessionList = new LinkedList<ClearSessionNode>();
        private static SessionManagement m_Instance = new SessionManagement();
        private Timer m_Timer = null;
        public const int SESSION_ONLINE_TIMEOUT = 0x1d4c0;
        public const int SESSION_TIMEOUT = 0xd6d8;
        public const int TIMER_PERIOD = 0x4e20;

        private SessionManagement()
        {
            this.m_Timer = new Timer(new TimerCallback(this.TimerProc));
            this.m_Timer.Change(0, 0x4e20);
        }

        public AccountState GetAccountState(string user)
        {
            lock (this.m_Accounts)
            {
                string key = user.ToUpper();
                if (!this.m_Accounts.ContainsKey(key))
                {
                    this.m_Accounts[key] = new AccountState(user);
                }
                return (this.m_Accounts[key] as AccountState);
            }
        }

        public void Insert(AccountSession session)
        {
            lock (this.m_ClearSessionList)
            {
                if (this.m_ClearSessionList != session.ListNode.List)
                {
                    this.m_ClearSessionList.AddLast(session.ListNode);
                }
                else
                {
                    this.m_ClearSessionList.Remove(session.ListNode);
                    this.m_ClearSessionList.AddLast(session.ListNode);
                }
                session.ListNode.Value.InsertTime = DateTime.Now;
            }
        }

        public bool IsOnline(string user)
        {
            lock (this.m_Accounts)
            {
                string key = user.ToUpper();
                if (!this.m_Accounts.ContainsKey(key))
                {
                    return false;
                }
                return (this.m_Accounts[key] as AccountState).IsOnline;
            }
        }

        public void Send(string command, string data)
        {
            List<AccountState> list = new List<AccountState>();
            lock (this.m_Accounts)
            {
                foreach (DictionaryEntry entry in this.m_Accounts)
                {
                    list.Add(entry.Value as AccountState);
                }
            }
            foreach (AccountState state in list)
            {
                state.Send(command, data);
            }
        }

        public void Send(string user, string command, string data)
        {
            AccountState accountState = this.GetAccountState(user);
            if (accountState != null)
            {
                accountState.Send(command, data);
            }
        }

        private void TimerProc(object state)
        {
            try
            {
                AccountState accountState;
                AccountInfo userInfo;
                Hashtable hashtable;
                Hashtable hashtable3;
                List<LinkedListNode<ClearSessionNode>> list = new List<LinkedListNode<ClearSessionNode>>();
                List<ClearSessionNode> list2 = new List<ClearSessionNode>();
                StringBuilder builder = new StringBuilder();
                lock (this.m_ClearSessionList)
                {
                    DateTime now = DateTime.Now;
                    for (LinkedListNode<ClearSessionNode> node = this.m_ClearSessionList.First; node != null; node = node.Next)
                    {
                        TimeSpan span = (TimeSpan) (now - node.Value.InsertTime);
                        double totalMilliseconds = span.TotalMilliseconds;
                        if (totalMilliseconds > 120000.0)
                        {
                            list.Add(node);
                        }
                        else
                        {
                            if (totalMilliseconds <= 55000.0)
                            {
                                break;
                            }
                            list2.Add(node.Value);
                        }
                    }
                    foreach (LinkedListNode<ClearSessionNode> node2 in list)
                    {
                        this.m_ClearSessionList.Remove(node2);
                        if (builder.Length > 0)
                        {
                            builder.Append(",");
                        }
                        builder.AppendFormat("({0},{1})", node2.Value.UserName, node2.Value.SessionID);
                    }
                    ServerImpl.Instance.WriteLog(string.Format("Clear Sessions Timer:Session Count = {0}", this.m_ClearSessionList.Count));
                }
                lock ((hashtable3 = hashtable = this.m_Accounts))
                {
                    ServerImpl.Instance.WriteLog(string.Format("Clear Sessions Timer:Account Count = {0}", this.m_Accounts.Count));
                }
                ServerImpl.Instance.WriteLog(string.Format("Clear Sessions Timer:Clear = \"{0}\"", builder));
                foreach (ClearSessionNode node3 in list2)
                {
                    try
                    {
                        accountState = this.GetAccountState(node3.UserName);
                        if (accountState != null)
                        {
                            accountState.Timeout(node3.SessionID);
                        }
                    }
                    catch
                    {
                    }
                }
                Hashtable hashtable2 = new Hashtable();
                foreach (LinkedListNode<ClearSessionNode> node2 in list)
                {
                    try
                    {
                        accountState = this.GetAccountState(node2.Value.UserName);
                        userInfo = AccountImpl.Instance.GetUserInfo(node2.Value.UserName);
                        hashtable2[userInfo.ID] = node2.Value.UserName;
                        if (accountState != null)
                        {
                            accountState.Remove(node2.Value.SessionID);
                        }
                        if (!accountState.IsOnline)
                        {
                            lock ((hashtable3 = hashtable = this.m_Accounts))
                            {
                                this.m_Accounts.Remove(accountState.UserName.ToUpper());
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                foreach (DictionaryEntry entry in hashtable2)
                {
                    if (!Instance.IsOnline(entry.Value as string))
                    {
                        userInfo = AccountImpl.Instance.GetUserInfo(Convert.ToInt64(entry.Key));
                        this.Send("UserStateChanged", Utility.RenderHashJson(new object[] { "User", userInfo.Name.ToUpper(), "State", "Offline", "Details", userInfo.DetailsJson }));
                    }
                }
            }
            catch
            {
            }
        }

        public static SessionManagement Instance
        {
            get
            {
                return m_Instance;
            }
        }
    }
}

