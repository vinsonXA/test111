namespace Core
{
    using System;
    using System.Collections.Generic;

    public interface IMessageStorage
    {
        List<Message> Find(long receiver, long sender, DateTime? from);
        List<Message> FindHistory(long user, long peer, DateTime from, DateTime to);
        List<Message> FindHistory(string peerType, string content, DateTime from, DateTime to, string user);
        DateTime GetCreatedTime();
        long GetMaxKey();
        void Write(List<Message> messages);
    }
}

