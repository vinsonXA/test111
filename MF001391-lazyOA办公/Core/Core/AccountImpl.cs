namespace Core
{
    using Core.IO;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;

    public class AccountImpl
    {
        private IAccountStorage m_IAccountStorage = null;
        private static AccountImpl m_Instance = new AccountImpl();
        private LinkedList<AccountInfo> m_List = new LinkedList<AccountInfo>();
        private object m_Lock = new object();
        private Hashtable m_UserInfoCache = new Hashtable();
        private Hashtable m_UserInfoCacheByID = new Hashtable();
        private const int MAX_CACHE_COUNT = 2;

        private AccountImpl()
        {
            this.Init();
        }

        public void _RefreshUserInfo(long id)
        {
            lock (this.m_Lock)
            {
                this.RefreshUserInfo(id);
            }
        }

        public void AddFriend(string user, string friend)
        {
            lock (this.m_Lock)
            {
                AccountInfo userInfo = Instance.GetUserInfo(user);
                AccountInfo info2 = Instance.GetUserInfo(friend);
                if (!((string.Compare(user, friend, true) == 0) || userInfo.ContainsFriend(friend)))
                {
                    this.m_IAccountStorage.AddFriend(user, friend);
                    this.RefreshUserInfo(user);
                    this.RefreshUserInfo(friend);
                }
            }
        }

        public void AddFriend(string user, string friend, int index)
        {
            lock (this.m_Lock)
            {
                if (this.m_IAccountStorage.GetRelationship(user, friend) == -1L)
                {
                    this.AddFriend(user, friend);
                }
            }
        }

        public void AddUser(string name, string nickname, string password, string email, string phone, string telphone)
        {
            lock (this.m_Lock)
            {
                this.m_IAccountStorage.AddUser(name, nickname, password, email, phone, telphone);
                this.RefreshUserInfo("manager");
            }
        }

        public void CreateGroup(string creator, string name, string nickname, long isExitGroup)
        {
            lock (this.m_Lock)
            {
                this.m_IAccountStorage.CreateGroup(creator, name, nickname, isExitGroup);
                this.RefreshUserInfo(creator);
            }
        }

        public void CreateTempGroup(string creator, string name, string nickname, string deptId, string userlist)
        {
            lock (this.m_Lock)
            {
                this.m_IAccountStorage.CreateTempGroup(creator, name, nickname, deptId, userlist);
                this.RefreshUserInfo(creator);
                if (userlist != "")
                {
                    foreach (string str in userlist.Split(new char[] { ',' }))
                    {
                        this.RefreshUserInfo(str);
                    }
                }
            }
        }

        public void CreateUser(string name, string nickname, string password, string email)
        {
            lock (this.m_Lock)
            {
                this.m_IAccountStorage.CreateUser(name, nickname, password, email);
                this.RefreshUserInfo("manager");
            }
        }

        public void DeleteFriend(string user, string friend)
        {
            lock (this.m_Lock)
            {
                if (this.m_IAccountStorage.GetRelationship(user, friend) != -1L)
                {
                    AccountInfo userInfo = this.GetUserInfo(user);
                    AccountInfo info2 = this.GetUserInfo(friend);
                    this.m_IAccountStorage.DeleteFriend(userInfo.ID, info2.ID);
                    this.RefreshUserInfo(user);
                    this.RefreshUserInfo(friend);
                }
            }
        }

        public void DeleteGroup(string name, string creator)
        {
            AccountInfo userInfo = this.GetUserInfo(name);
            List<string> list = new List<string>();
            foreach (string str in userInfo.Friends)
            {
                list.Add(str);
            }
            lock (this.m_Lock)
            {
                this.m_IAccountStorage.DeleteGroup(userInfo.ID);
                foreach (string str2 in list)
                {
                    this.RefreshUserInfo(str2);
                }
            }
        }

        public void DeleteUser(string name)
        {
            AccountInfo userInfo = this.GetUserInfo(name);
            long iD = userInfo.ID;
            List<string> list = new List<string>();
            foreach (string str in userInfo.Friends)
            {
                list.Add(str);
            }
            this.m_IAccountStorage.DeleteUser(userInfo.ID);
            try
            {
                Directory.Delete(string.Format("/{0}", name));
            }
            catch
            {
            }
            foreach (string str2 in list)
            {
                this.RefreshUserInfo(str2);
            }
        }

        public DataRowCollection GetAllGroups()
        {
            return this.m_IAccountStorage.GetAllGroups();
        }

        public DataRowCollection GetAllUsers()
        {
            return this.m_IAccountStorage.GetAllUsers();
        }

        public string GetAllUsersByName(string filter)
        {
            DataTable allUserByName = this.m_IAccountStorage.GetAllUserByName(filter.Trim());
            if (allUserByName.Rows.Count > 0)
            {
                return Utility.DataTableToJSON(allUserByName, "Users");
            }
            return Utility.RenderHashJson(new object[] { "Users", "" });
        }

        private string[] GetGroupManagers(string name)
        {
            return this.m_IAccountStorage.GetGroupManagers(name);
        }

        public string GetGroupTempNameByDeptId(string deptId)
        {
            return this.m_IAccountStorage.GetGroupTempNameByDeptId(deptId);
        }

        public List<string> GetIMWindowRoles(string name)
        {
            return this.m_IAccountStorage.GetIMWindowRoles(name);
        }

        public AccountInfo GetUserInfo(long userId)
        {
            lock (this.m_Lock)
            {
                AccountInfo info = null;
                try
                {
                    if (this.m_UserInfoCacheByID.ContainsKey(userId))
                    {
                        info = this.m_UserInfoCacheByID[userId] as AccountInfo;
                        this.m_List.Remove(info.ListNode);
                        this.m_List.AddLast(info.ListNode);
                    }
                    else
                    {
                        info = this.RefreshUserInfo(userId);
                    }
                }
                catch
                {
                }
                return info;
            }
        }

        public AccountInfo GetUserInfo(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return null;
            }
            lock (this.m_Lock)
            {
                string key = user.ToUpper();
                AccountInfo info = null;
                try
                {
                    if (this.m_UserInfoCache.ContainsKey(key))
                    {
                        info = this.m_UserInfoCache[key] as AccountInfo;
                        this.m_List.Remove(info.ListNode);
                        this.m_List.AddLast(info.ListNode);
                    }
                    else
                    {
                        info = this.RefreshUserInfo(user);
                    }
                }
                catch
                {
                }
                return info;
            }
        }

        public void Init()
        {
            string[] strArray = Utility.GetConfig().AppSettings.Settings["AccountStorageImpl"].Value.Split(new char[] { ' ' });
            ConstructorInfo constructor = Assembly.Load(strArray[0]).GetType(strArray[1]).GetConstructor(new Type[0]);
            this.m_IAccountStorage = constructor.Invoke(new object[0]) as IAccountStorage;
        }

        private AccountInfo RefreshUserInfo(long id)
        {
            DataRow accountInfo = this.m_IAccountStorage.GetAccountInfo(id);
            if (accountInfo != null)
            {
                AccountInfo info;
                string name = accountInfo["Name"] as string;
                List<FriendInfo> list = new List<FriendInfo>();
                List<FriendInfo> list2 = new List<FriendInfo>();
                FriendInfo creator = null;
                foreach (DataRow row2 in this.m_IAccountStorage.GetFriends(name))
                {
                    string str2 = row2["Name"] as string;
                    DateTime renewTime = (DateTime) row2["RenewTime"];
                    FriendInfo item = new FriendInfo(str2, renewTime, Convert.ToInt64(row2["Relationship"]), Convert.ToInt64(row2["Type"]));
                    list.Add(item);
                    long num = Convert.ToInt64(row2["Relationship"]);
                    if ((num <= 3L) && (num >= 2L))
                    {
                        switch (((int) num))
                        {
                            case 2:
                                list2.Add(item);
                                break;

                            case 3:
                                list2.Add(item);
                                creator = item;
                                break;
                        }
                    }
                }
                if (this.m_UserInfoCache.ContainsKey(name.ToUpper()))
                {
                    info = this.m_UserInfoCache[name.ToUpper()] as AccountInfo;
                    info.Reset(accountInfo["Name"] as string, accountInfo["NickName"] as string, Convert.ToInt64(accountInfo["Key"]), Convert.ToInt64(accountInfo["Type"]), this.m_IAccountStorage.GetUserRoles(name), list.ToArray(), (Convert.ToInt64(accountInfo["Type"]) == 1L) ? list2.ToArray() : null, creator, accountInfo["EMail"] as string, accountInfo["InviteCode"] as string, Convert.ToInt64(accountInfo["AcceptStrangerIM"]) != 0L, Convert.ToInt64(accountInfo["MsgFileLimit"]), Convert.ToInt64(accountInfo["MsgImageLimit"]), Convert.ToInt64(accountInfo["DiskSize"]), Convert.ToInt64(accountInfo["IsTemp"]), (DateTime) accountInfo["RegisterTime"], accountInfo["HomePage"] as string, accountInfo["Password"] as string, accountInfo);
                    return info;
                }
                info = new AccountInfo(accountInfo["Name"] as string, accountInfo["NickName"] as string, Convert.ToInt64(accountInfo["Key"]), Convert.ToInt64(accountInfo["Type"]), this.m_IAccountStorage.GetUserRoles(name), list.ToArray(), (Convert.ToInt64(accountInfo["Type"]) == 1L) ? list2.ToArray() : null, creator, accountInfo["EMail"] as string, accountInfo["InviteCode"] as string, Convert.ToInt64(accountInfo["AcceptStrangerIM"]) != 0L, Convert.ToInt64(accountInfo["MsgFileLimit"]), Convert.ToInt64(accountInfo["MsgImageLimit"]), Convert.ToInt64(accountInfo["DiskSize"]), Convert.ToInt64(accountInfo["IsTemp"]), (DateTime) accountInfo["RegisterTime"], accountInfo["HomePage"] as string, accountInfo["Password"] as string, accountInfo);
                if (this.m_List.Count >= 2)
                {
                    AccountInfo info4 = this.m_List.First.Value;
                    this.m_UserInfoCache.Remove(info4.Name.ToUpper());
                    this.m_UserInfoCacheByID.Remove(info4.ID);
                    this.m_List.RemoveFirst();
                }
                this.m_UserInfoCache[name.ToUpper()] = info;
                this.m_UserInfoCacheByID[info.ID] = info;
                this.m_List.AddLast(info.ListNode);
                return info;
            }
            return null;
        }

        private AccountInfo RefreshUserInfo(string userName)
        {
            string key = userName.ToUpper();
            DataRow accountInfo = this.m_IAccountStorage.GetAccountInfo(userName);
            if (accountInfo != null)
            {
                AccountInfo info;
                List<FriendInfo> list = new List<FriendInfo>();
                List<FriendInfo> list2 = new List<FriendInfo>();
                FriendInfo creator = null;
                foreach (DataRow row2 in this.m_IAccountStorage.GetFriends(userName))
                {
                    string name = row2["Name"] as string;
                    DateTime renewTime = (DateTime) row2["RenewTime"];
                    FriendInfo item = new FriendInfo(name, renewTime, Convert.ToInt64(row2["Relationship"]), Convert.ToInt64(row2["Type"]));
                    list.Add(item);
                    long num = Convert.ToInt64(row2["Relationship"]);
                    if ((num <= 3L) && (num >= 2L))
                    {
                        switch (((int) num))
                        {
                            case 2:
                                list2.Add(item);
                                break;

                            case 3:
                                list2.Add(item);
                                creator = item;
                                break;
                        }
                    }
                }
                if (this.m_UserInfoCache.ContainsKey(key))
                {
                    info = this.m_UserInfoCache[key] as AccountInfo;
                    info.Reset(accountInfo["Name"] as string, accountInfo["NickName"] as string, Convert.ToInt64(accountInfo["Key"]), Convert.ToInt64(accountInfo["Type"]), this.m_IAccountStorage.GetUserRoles(userName), list.ToArray(), (Convert.ToInt64(accountInfo["Type"]) == 1L) ? list2.ToArray() : null, creator, accountInfo["EMail"] as string, accountInfo["InviteCode"] as string, Convert.ToInt64(accountInfo["AcceptStrangerIM"]) != 0L, Convert.ToInt64(accountInfo["MsgFileLimit"]), Convert.ToInt64(accountInfo["MsgImageLimit"]), Convert.ToInt64(accountInfo["DiskSize"]), Convert.ToInt64(accountInfo["IsTemp"]), (DateTime) accountInfo["RegisterTime"], accountInfo["HomePage"] as string, accountInfo["Password"] as string, accountInfo);
                    return info;
                }
                info = new AccountInfo(accountInfo["Name"] as string, accountInfo["NickName"] as string, Convert.ToInt64(accountInfo["Key"]), Convert.ToInt64(accountInfo["Type"]), this.m_IAccountStorage.GetUserRoles(userName), list.ToArray(), (Convert.ToInt64(accountInfo["Type"]) == 1L) ? list2.ToArray() : null, creator, accountInfo["EMail"] as string, accountInfo["InviteCode"] as string, Convert.ToInt64(accountInfo["AcceptStrangerIM"]) != 0L, Convert.ToInt64(accountInfo["MsgFileLimit"]), Convert.ToInt64(accountInfo["MsgImageLimit"]), Convert.ToInt64(accountInfo["DiskSize"]), Convert.ToInt64(accountInfo["IsTemp"]), (DateTime) accountInfo["RegisterTime"], accountInfo["HomePage"] as string, accountInfo["Password"] as string, accountInfo);
                if (this.m_List.Count >= 2)
                {
                    AccountInfo info4 = this.m_List.First.Value;
                    this.m_UserInfoCache.Remove(info4.Name.ToUpper());
                    this.m_UserInfoCacheByID.Remove(info4.ID);
                    this.m_List.RemoveFirst();
                }
                this.m_UserInfoCache[key] = info;
                this.m_UserInfoCacheByID[info.ID] = info;
                this.m_List.AddLast(info.ListNode);
                return info;
            }
            return null;
        }

        public void UpdateUserInfo(string name, Hashtable values)
        {
            lock (this.m_Lock)
            {
                this.m_IAccountStorage.UpdateUserInfo(name, values);
                this.RefreshUserInfo(name);
            }
        }

        public bool Validate(string userId, string password)
        {
            lock (this.m_Lock)
            {
                return this.m_IAccountStorage.Validate(userId, password);
            }
        }

        public static AccountImpl Instance
        {
            get
            {
                return m_Instance;
            }
        }
    }
}

