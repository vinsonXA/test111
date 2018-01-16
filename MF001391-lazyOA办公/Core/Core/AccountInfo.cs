namespace Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    public class AccountInfo : IRenderJson
    {
        private Details _detailsJson = null;
        private bool m_AcceptStrangerIM;
        private FriendInfo m_Creator;
        private long m_DiskSize;
        private string m_DType;
        private string m_EMail;
        private Hashtable m_FriendIndex = null;
        private FriendInfo[] m_Friends;
        private string m_HeadIMG;
        private string m_HomePage = string.Empty;
        private long m_ID;
        private long m_IMFileLimit;
        private long m_IMImageLimit;
        private string m_InviteCode;
        private bool m_IsExitGroup = false;
        private bool m_IsTemp;
        private LinkedListNode<AccountInfo> m_ListNode;
        private object m_Lock = new object();
        private Hashtable m_Managers = null;
        private string m_MobilePhone;
        private string m_Name;
        private string m_NickName;
        private string m_ParentId;
        private string m_Password;
        private string m_Phone;
        private DateTime m_RegisterTime = DateTime.Now;
        private string m_Remark;
        private string[] m_Roles;
        private string m_TelPhone;
        private long m_Type;

        public AccountInfo(string name, string nickname, long id, long type, string[] roles, FriendInfo[] friends, FriendInfo[] managers, FriendInfo creator, string email, string inviteCode, bool acceptStrangerIM, long imf_limite, long imimage_limit, long diskSize, long isTemp, DateTime registerTime, string homePage, string pwd, DataRow data)
        {
            this._detailsJson = new Details(this);
            this.m_ListNode = new LinkedListNode<AccountInfo>(this);
            this.Reset(name, nickname, id, type, roles, friends, managers, creator, email, inviteCode, acceptStrangerIM, imf_limite, imimage_limit, diskSize, isTemp, registerTime, homePage, pwd, data);
        }

        public bool ContainsFriend(string name)
        {
            lock (this.m_Lock)
            {
                return this.m_FriendIndex.ContainsKey(name.ToUpper());
            }
        }

        public bool ContainsMember(string name)
        {
            lock (this.m_Lock)
            {
                return this.m_FriendIndex.ContainsKey(name.ToUpper());
            }
        }

        void IRenderJson.RenderJson(StringBuilder builder)
        {
            Utility.RenderHashJson(builder, new object[] { "ID", this.m_ID, "Name", this.m_Name, "Nickname", this.m_NickName, "Type", this.m_Type });
        }

        public DateTime GetGroupMemberRenewTime(string user)
        {
            lock (this.m_Lock)
            {
                string str = user.ToUpper();
                return (this.m_FriendIndex[str] as FriendInfo).RenewTime;
            }
        }

        public bool IsCreatedBy(string name)
        {
            lock (this.m_Lock)
            {
                return ((this.Type == 1L) && (string.Compare(name, this.m_Creator.Name, true) == 0));
            }
        }

        public bool IsManagedBy(string name)
        {
            lock (this.m_Lock)
            {
                return ((this.Type == 1L) && this.m_Managers.ContainsKey(name.ToUpper()));
            }
        }

        public bool IsRole(string role)
        {
            lock (this.m_Lock)
            {
                foreach (string str in this.Roles)
                {
                    if (str == role)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void Reset(string name, string nickname, long id, long type, string[] roles, FriendInfo[] friends, FriendInfo[] managers, FriendInfo creator, string email, string inviteCode, bool acceptStrangerIM, long imf_limite, long imimage_limit, long diskSize, long isTemp, DateTime registerTime, string homePage, string pwd, DataRow data)
        {
            lock (this.m_Lock)
            {
                this.m_FriendIndex = new Hashtable();
                this.m_Managers = new Hashtable();
                this.m_Name = name;
                this.m_NickName = nickname;
                this.m_ID = id;
                this.m_Type = type;
                this.m_Roles = roles;
                this.m_Friends = friends;
                this.m_Creator = creator;
                this.m_EMail = email;
                this.m_InviteCode = inviteCode;
                if (data["HeadIMG"] != null)
                {
                    this.m_HeadIMG = data["HeadIMG"].ToString();
                }
                else
                {
                    this.m_HeadIMG = "";
                }
                if (data["Remark"] != null)
                {
                    this.m_Remark = data["Remark"].ToString();
                }
                else
                {
                    this.m_Remark = "这个人很懒,什么都没留下.";
                }
                if (data.Table.Columns.Contains("ParentId") && (data["ParentId"] != null))
                {
                    this.m_ParentId = data["ParentId"].ToString();
                }
                if (data.Table.Columns.Contains("DType") && (data["DType"] != null))
                {
                    this.m_DType = data["DType"].ToString();
                }
                this.m_Password = pwd;
                if (data.Table.Columns.Contains("Phone") && (data["Phone"] != null))
                {
                    this.m_Phone = data["Phone"].ToString();
                }
                if (data.Table.Columns.Contains("TelPhone") && (data["TelPhone"] != null))
                {
                    this.m_TelPhone = data["TelPhone"].ToString();
                }
                if (data.Table.Columns.Contains("MobilePhone") && (data["MobilePhone"] != null))
                {
                    this.m_MobilePhone = data["MobilePhone"].ToString();
                }
                if (data.Table.Columns.Contains("IsExitGroup") && (data["IsExitGroup"] != null))
                {
                    this.m_IsExitGroup = Convert.ToInt64(data["IsExitGroup"]) == 1L;
                }
                this.m_AcceptStrangerIM = acceptStrangerIM;
                this.m_IMFileLimit = imf_limite;
                this.m_IMImageLimit = imimage_limit;
                this.m_IsTemp = isTemp != 0L;
                this.m_HomePage = homePage;
                this.m_DiskSize = diskSize;
                if (friends != null)
                {
                    foreach (FriendInfo info in friends)
                    {
                        this.m_FriendIndex.Add(info.Name.ToUpper(), info);
                    }
                }
                if (managers != null)
                {
                    foreach (FriendInfo info in managers)
                    {
                        this.m_Managers.Add(info.Name.ToUpper(), info);
                    }
                }
                this.m_RegisterTime = registerTime;
            }
        }

        public bool AcceptStrangerIM
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_AcceptStrangerIM;
                }
            }
        }

        public string Creator
        {
            get
            {
                lock (this.m_Lock)
                {
                    string name = string.Empty;
                    if (this.m_Creator != null)
                    {
                        name = this.m_Creator.Name;
                    }
                    return name;
                }
            }
        }

        public Details DetailsJson
        {
            get
            {
                return this._detailsJson;
            }
        }

        public long DiskSize
        {
            get
            {
                if (this.Type == 1L)
                {
                    return AccountImpl.Instance.GetUserInfo(this.m_Creator.Name).DiskSize;
                }
                lock (this.m_Lock)
                {
                    return ((this.m_DiskSize * 0x400L) * 0x400L);
                }
            }
        }

        public string DType
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_DType;
                }
            }
        }

        public string EMail
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_EMail;
                }
            }
        }

        public string[] Friends
        {
            get
            {
                lock (this.m_Lock)
                {
                    string[] strArray = new string[this.m_Friends.Length];
                    for (int i = 0; i < this.m_Friends.Length; i++)
                    {
                        strArray[i] = this.m_Friends[i].Name;
                    }
                    return strArray;
                }
            }
        }

        public string[] Groups
        {
            get
            {
                lock (this.m_Lock)
                {
                    List<string> list = new List<string>();
                    foreach (FriendInfo info in this.m_Friends)
                    {
                        if (info.PeerType == 1L)
                        {
                            list.Add(info.Name);
                        }
                    }
                    return list.ToArray();
                }
            }
        }

        public string HeadIMG
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_HeadIMG;
                }
            }
        }

        public string HomePage
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_HomePage;
                }
            }
        }

        public long ID
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_ID;
                }
            }
        }

        public string InviteCode
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_InviteCode;
                }
            }
        }

        public bool IsExitGroup
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_IsExitGroup;
                }
            }
        }

        public bool IsTemp
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_IsTemp;
                }
            }
        }

        public LinkedListNode<AccountInfo> ListNode
        {
            get
            {
                return this.m_ListNode;
            }
        }

        public string MobilePhone
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_MobilePhone;
                }
            }
        }

        public long MsgFileLimit
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_IMFileLimit;
                }
            }
        }

        public long MsgImageLimit
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_IMImageLimit;
                }
            }
        }

        public string Name
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_Name;
                }
            }
        }

        public string Nickname
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_NickName;
                }
            }
        }

        public string ParentId
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_ParentId;
                }
            }
        }

        public string Password
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_Password;
                }
            }
        }

        public string Phone
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_Phone;
                }
            }
        }

        public DateTime RegisterTime
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_RegisterTime;
                }
            }
        }

        public string Remark
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_Remark;
                }
            }
        }

        public string[] Roles
        {
            get
            {
                lock (this.m_Lock)
                {
                    string[] array = new string[this.m_Roles.Length];
                    this.m_Roles.CopyTo(array, 0);
                    return array;
                }
            }
        }

        public string TelPhone
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_TelPhone;
                }
            }
        }

        public long Type
        {
            get
            {
                lock (this.m_Lock)
                {
                    return this.m_Type;
                }
            }
        }

        public class Details : IRenderJson
        {
            private AccountInfo _info;

            public Details(AccountInfo info)
            {
                this._info = info;
            }

            void IRenderJson.RenderJson(StringBuilder builder)
            {
                bool flag = SessionManagement.Instance.IsOnline(this._info.Name);
                if (this._info.Type == 0L)
                {
                    Utility.RenderHashJson(builder, new object[] { 
                        "ID", this._info.m_ID, "Name", this._info.m_Name, "Nickname", this._info.m_NickName, "Type", this._info.m_Type, "EMail", this._info.m_EMail, "InviteCode", this._info.m_InviteCode, "HeadIMG", this._info.m_HeadIMG, "HomePage", this._info.m_HomePage, 
                        "Remark", this._info.m_Remark, "ParentId", this._info.m_ParentId, "DType", this._info.m_DType, "State", flag ? "Online" : "Offline", "GroupCreator", "", "Phone", this._info.Phone, "TelPhone", this._info.TelPhone, "MobilePhone", this._info.MobilePhone, 
                        "IsExitGroup", this._info.IsExitGroup
                     });
                }
                else
                {
                    Utility.RenderHashJson(builder, new object[] { 
                        "ID", this._info.m_ID, "Name", this._info.m_Name, "Nickname", this._info.m_NickName, "Type", this._info.m_Type, "EMail", this._info.m_EMail, "InviteCode", this._info.m_InviteCode, "HeadIMG", this._info.m_HeadIMG, "HomePage", this._info.m_HomePage, 
                        "Remark", this._info.m_Remark, "ParentId", this._info.m_ParentId, "DType", this._info.m_DType, "State", flag ? "Online" : "Offline", "GroupCreator", this._info.Creator, "Phone", this._info.Phone, "TelPhone", this._info.TelPhone, "MobilePhone", this._info.MobilePhone, 
                        "IsExitGroup", this._info.IsExitGroup
                     });
                }
            }
        }
    }
}

