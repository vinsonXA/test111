namespace Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;

    public class MessageImpl
    {
        private IMessageStorage m_IMessageStorage = null;
        private static MessageImpl m_Instance = new MessageImpl();
        private object m_Lock = new object();
        private DateTime m_MaxCreatedTime = DateTime.Now;
        private long m_MaxKey = 1L;
        private Timer m_Timer = null;
        private const int MAX_CACHE_COUNT = 4;

        static MessageImpl()
        {
            Instance.Initialize(HttpContext.Current);
        }

        private MessageImpl()
        {
            this.m_Timer = new Timer(new TimerCallback(this.TimerProc));
            this.m_Timer.Change(0, 0x1d4c0);
        }

        public void Dispose()
        {
        }

        public List<Message> Find(string receiver, string sender, DateTime? from)
        {
            lock (this.m_Lock)
            {
                DateTime? minCreatedTime = MessageCacheManagement.Instance.GetMinCreatedTime(receiver);
                List<Message> list = new List<Message>();
                if ((!minCreatedTime.HasValue || !from.HasValue) || (from.Value < minCreatedTime.Value))
                {
                    list.AddRange(this.FindInDatabase(receiver, sender, from));
                    if (AccountImpl.Instance.GetUserInfo(receiver).Type == 0L)
                    {
                        list.AddRange(this.FindInDatabase(sender, receiver, from));
                    }
                }
                list.AddRange(MessageCacheManagement.Instance.Find(receiver, sender, from.Value));
                if (AccountImpl.Instance.GetUserInfo(receiver).Type == 0L)
                {
                    list.AddRange(MessageCacheManagement.Instance.Find(sender, receiver, from.Value));
                }
                return list;
            }
        }

        public List<Message> FindHistory(string receiver, string sender, DateTime from, DateTime to)
        {
            return this.m_IMessageStorage.FindHistory(AccountImpl.Instance.GetUserInfo(receiver).ID, AccountImpl.Instance.GetUserInfo(sender).ID, from, to);
        }

        public List<Message> FindHistory(string peerType, string content, DateTime from, DateTime to, string user)
        {
            return this.m_IMessageStorage.FindHistory(peerType, content, from, to, user);
        }

        public List<Message> FindInDatabase(string receiver, string sender, DateTime? from)
        {
            lock (this.m_Lock)
            {
                return this.m_IMessageStorage.Find((receiver == "*") ? 0L : AccountImpl.Instance.GetUserInfo(receiver).ID, (sender == "*") ? 0L : AccountImpl.Instance.GetUserInfo(sender).ID, from);
            }
        }

        public void Initialize(HttpContext context)
        {
            lock (this.m_Lock)
            {
                string[] strArray = Utility.GetConfig().AppSettings.Settings["MessageStorageImpl"].Value.Split(new char[] { ' ' });
                ConstructorInfo constructor = Assembly.Load(strArray[0]).GetType(strArray[1]).GetConstructor(new Type[0]);
                this.m_IMessageStorage = constructor.Invoke(new object[0]) as IMessageStorage;
                this.m_MaxKey = this.m_IMessageStorage.GetMaxKey();
                this.m_MaxCreatedTime = this.m_IMessageStorage.GetCreatedTime();
            }
        }

        public Message NewMessage(string receiver, string sender, string content, Hashtable data)
        {
            lock (this.m_Lock)
            {
                Hashtable config;
                DateTime time;
                string str;
                Hashtable hashtable2;
                Hashtable hashtable3;
                long key = this.m_MaxKey += 1L;
                content = HtmlUtil.ReplaceHtml(content);
                MsgAccessoryEval eval = new MsgAccessoryEval(key, receiver, sender, data);
                content = new Regex("{Accessory [^\f\n\r\t\v<>]+}").Replace(content, new MatchEvaluator(eval.Replace));
                Message item = new Message(AccountImpl.Instance.GetUserInfo(sender), AccountImpl.Instance.GetUserInfo(receiver), content, new DateTime((DateTime.Now.Ticks / 0x2710L) * 0x2710L), key);
                new List<Message>().Add(item);
                if (AccountImpl.Instance.GetUserInfo(receiver).Type == 0L)
                {
                    if (SessionManagement.Instance.IsOnline(receiver))
                    {
                        try
                        {
                            config = SessionManagement.Instance.GetAccountState(receiver).GetConfig("message.conf");
                            lock ((hashtable3 = hashtable2 = config))
                            {
                                time = (DateTime) config["LastReceivedTime"];
                                config["LastReceivedTime"] = item.CreatedTime;
                            }
                            str = Utility.RenderHashJson(new object[] { "Peer", sender, "Message", item });
                            SessionManagement.Instance.Send(receiver, "GLOBAL:IM_MESSAGE_NOTIFY", str);
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(receiver);
                    AccountInfo info2 = AccountImpl.Instance.GetUserInfo(sender);
                    foreach (string str2 in userInfo.Friends)
                    {
                        try
                        {
                            AccountInfo info3 = AccountImpl.Instance.GetUserInfo(str2);
                            if (((info2.Name.ToLower() == "administrator") || (info3.ID != info2.ID)) && SessionManagement.Instance.IsOnline(str2))
                            {
                                config = SessionManagement.Instance.GetAccountState(info3.Name).GetConfig("message.conf");
                                lock ((hashtable3 = hashtable2 = config))
                                {
                                    time = (DateTime) config["LastReceivedTime"];
                                    config["LastReceivedTime"] = item.CreatedTime;
                                }
                                str = Utility.RenderHashJson(new object[] { "Peer", userInfo.Name, "Message", item });
                                SessionManagement.Instance.Send(str2, "GLOBAL:IM_MESSAGE_NOTIFY", str);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                MessageCacheManagement.Instance.Insert(receiver, item);
                if (MessageCacheManagement.Instance.Count >= 4)
                {
                    this.WriteCache();
                }
                return item;
            }
        }

        private void TimerProc(object state)
        {
            this.WriteCache();
        }

        public void WriteCache()
        {
            if (MessageCacheManagement.Instance.Count > 0)
            {
                List<Message> all = MessageCacheManagement.Instance.GetAll();
                this.m_IMessageStorage.Write(all);
                MessageCacheManagement.Instance.Clear();
            }
        }

        public static MessageImpl Instance
        {
            get
            {
                return m_Instance;
            }
        }
    }
}

