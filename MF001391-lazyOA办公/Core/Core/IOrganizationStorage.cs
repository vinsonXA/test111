namespace Core
{
    using System;
    using System.Data;

    public interface IOrganizationStorage
    {
        void AddDept(string name, long pdid, long cindex);
        void DeleteDept(long did);
        DataTable GetAllDepts();
        DataTable GetCompanyInfo();
        DataRowCollection GetDeptAllUser(string deptId);
        DataTable GetDeptById(long did);
        DataTable GetDeptList(string filter);
        DataRowCollection GetDepts();
        DataTable GetUsersByDeptId(long did);
        DataTable GetUsersByNoExistsDept(long did);
        void UpdateCompanyInfo(string id, string name, string tel, string address, string logo);
        void UpdateDept(long did, string name, long pdid, long cindex);
        void UpdateDeptMember(string ids, long did);
    }
}

