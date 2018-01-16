namespace Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    public interface IAccountStorage
    {
        void AddFriend(string user, string friend);
        void AddUser(string name, string nickname, string password, string email, string phone, string telphone);
        void CreateGroup(string creator, string name, string nickname, long isExitGroup);
        void CreateTempGroup(string creator, string name, string nickname, string deptId, string userlist);
        void CreateUser(string name, string nickname, string password, string email);
        void DeleteFriend(long user, long friend);
        void DeleteGroup(long id);
        void DeleteUser(long name);
        DataRow GetAccountInfo(long key);
        DataRow GetAccountInfo(string name);
        DataRowCollection GetAllGroups();
        DataTable GetAllUserByName(string name);
        DataRowCollection GetAllUsers();
        DataRowCollection GetFriends(string name);
        string[] GetGroupManagers(string name);
        string GetGroupTempNameByDeptId(string deptId);
        List<string> GetIMWindowRoles(string name);
        long GetRelationship(string account1, string account2);
        string[] GetUserRoles(string userId);
        void UpdateUserInfo(string name, Hashtable values);
        bool Validate(string userId, string password);
    }
}

