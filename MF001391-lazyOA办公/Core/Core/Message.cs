namespace Core
{
    using System;
    using System.Text;

    public class Message : IRenderJson
    {
        public string Content;
        public DateTime CreatedTime;
        public long Key;
        public AccountInfo Receiver;
        public AccountInfo Sender;

        public Message(AccountInfo sender, AccountInfo receiver, string content, DateTime cteatedTime, long key)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Content = content;
            this.CreatedTime = cteatedTime;
            this.Key = key;
        }

        void IRenderJson.RenderJson(StringBuilder builder)
        {
            Utility.RenderHashJson(builder, new object[] { "Sender", this.Sender, "Receiver", this.Receiver, "CreatedTime", this.CreatedTime, "Key", this.Key, "Content", this.Content });
        }
    }
}

