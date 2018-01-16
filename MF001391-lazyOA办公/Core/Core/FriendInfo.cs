namespace Core
{
    using System;

    public class FriendInfo
    {
        public string Name;
        public long PeerType;
        public long Relationthip;
        public DateTime RenewTime;

        public FriendInfo(string name, DateTime renewTime, long relationthip, long peerType)
        {
            this.Name = name;
            this.RenewTime = renewTime;
            this.Relationthip = relationthip;
            this.PeerType = peerType;
        }
    }
}

