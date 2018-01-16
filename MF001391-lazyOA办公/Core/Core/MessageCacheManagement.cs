namespace Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class MessageCacheManagement
    {
        private Hashtable m_Cache = new Hashtable();
        private int m_Count = 0;
        private static MessageCacheManagement m_Instance = new MessageCacheManagement();

        private MessageCacheManagement()
        {
        }

        public void Clear()
        {
            lock (this.m_Cache)
            {
                List<Message> list = new List<Message>();
                foreach (DictionaryEntry entry in this.m_Cache)
                {
                    (entry.Value as List<Message>).Clear();
                }
                this.m_Count = 0;
            }
        }

        public List<Message> Find(string user, string sender, DateTime from)
        {
            int num;
            Hashtable hashtable;
            List<Message> list;
            Hashtable hashtable2;
            List<Message> list6;
            List<Message> list2 = new List<Message>();
            List<Message> userMessageCache = null;
            lock ((hashtable2 = hashtable = this.m_Cache))
            {
                userMessageCache = this.GetUserMessageCache(user);
            }
            lock ((list6 = list = userMessageCache))
            {
                num = 0;
                while ((num < userMessageCache.Count) && (userMessageCache[num].CreatedTime <= from))
                {
                    num++;
                }
                while (num < userMessageCache.Count)
                {
                    if (((sender == null) || (sender == "*")) || (sender == userMessageCache[num].Sender.Name))
                    {
                        list2.Add(userMessageCache[num]);
                    }
                    num++;
                }
            }
            if ((sender == null) || (sender == "*"))
            {
                AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(user);
                foreach (string str in userInfo.Groups)
                {
                    AccountInfo info2 = AccountImpl.Instance.GetUserInfo(str);
                    List<Message> list4 = null;
                    lock ((hashtable2 = hashtable = this.m_Cache))
                    {
                        list4 = this.GetUserMessageCache(str);
                    }
                    lock ((list6 = list = list4))
                    {
                        if (from < info2.GetGroupMemberRenewTime(user))
                        {
                            from = info2.GetGroupMemberRenewTime(user);
                        }
                        num = 0;
                        while ((num < list4.Count) && (list4[num].CreatedTime <= from))
                        {
                            num++;
                        }
                        while (num < list4.Count)
                        {
                            list2.Add(list4[num]);
                            num++;
                        }
                    }
                }
            }
            return list2;
        }

        public List<Message> GetAll()
        {
            lock (this.m_Cache)
            {
                List<Message> list = new List<Message>();
                foreach (DictionaryEntry entry in this.m_Cache)
                {
                    foreach (Message message in entry.Value as List<Message>)
                    {
                        list.Add(message);
                    }
                }
                return list;
            }
        }

        public DateTime? GetMinCreatedTime(string user)
        {
            lock (this.m_Cache)
            {
                List<Message> userMessageCache = this.GetUserMessageCache(user);
                return ((userMessageCache.Count == 0) ? null : new DateTime?(userMessageCache[0].CreatedTime));
            }
        }

        private List<Message> GetUserMessageCache(string user)
        {
            if (!this.m_Cache.ContainsKey(user))
            {
                this.m_Cache.Add(user, new List<Message>());
            }
            return (this.m_Cache[user] as List<Message>);
        }

        public void Insert(string user, Message msg)
        {
            List<Message> userMessageCache = null;
            lock (this.m_Cache)
            {
                userMessageCache = this.GetUserMessageCache(user);
            }
            lock (userMessageCache)
            {
                userMessageCache.Add(msg);
                this.m_Count++;
            }
        }

        public int Count
        {
            get
            {
                return this.m_Count;
            }
        }

        public static MessageCacheManagement Instance
        {
            get
            {
                return m_Instance;
            }
        }
    }
}

