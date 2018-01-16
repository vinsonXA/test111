namespace Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;

    public class OrganizationImpl
    {
        private static OrganizationImpl m_Instance = new OrganizationImpl();
        private IOrganizationStorage m_IOrganizationStorage = null;

        private OrganizationImpl()
        {
            this.Init();
        }

        public void AddDept(string name, long pdid, long cindex)
        {
            this.m_IOrganizationStorage.AddDept(name, pdid, cindex);
        }

        public void DeleteDept(long did)
        {
            this.m_IOrganizationStorage.DeleteDept(did);
        }

        public string GetAllDepts()
        {
            DataTable allDepts = this.m_IOrganizationStorage.GetAllDepts();
            if (allDepts.Rows.Count > 0)
            {
                return Utility.DataTableToJSON(allDepts, "Depts");
            }
            return Utility.RenderHashJson(new object[] { "Depts", "" });
        }

        public DataTable GetCompanyInfo()
        {
            return this.m_IOrganizationStorage.GetCompanyInfo();
        }

        public DataRowCollection GetDeptAllUser(string deptId)
        {
            return this.m_IOrganizationStorage.GetDeptAllUser(deptId);
        }

        public DataTable GetDeptById(long did)
        {
            return this.m_IOrganizationStorage.GetDeptById(did);
        }

        public DataTable GetDeptList(string filter)
        {
            return this.m_IOrganizationStorage.GetDeptList(filter);
        }

        public List<AccountInfo.Details> GetDeptsFirends()
        {
            List<AccountInfo.Details> list = new List<AccountInfo.Details>();
            foreach (DataRow row in this.m_IOrganizationStorage.GetDepts())
            {
                AccountInfo info = new AccountInfo(row["Name"] as string, row["NickName"] as string, Convert.ToInt64(row["uid"]), Convert.ToInt64(row["Type"]), null, null, null, null, row["EMail"] as string, row["InviteCode"] as string, Convert.ToInt64(row["AcceptStrangerIM"]) != 0L, Convert.ToInt64(row["MsgFileLimit"]), Convert.ToInt64(row["MsgImageLimit"]), Convert.ToInt64(row["DiskSize"]), Convert.ToInt64(row["IsTemp"]), (DateTime) row["RegisterTime"], row["HomePage"] as string, row["Password"] as string, row);
                list.Add(info.DetailsJson);
            }
            return list;
        }

        public DataTable GetUsersByDeptId(long did)
        {
            return this.m_IOrganizationStorage.GetUsersByDeptId(did);
        }

        public DataTable GetUsersByNoExistsDept(long did)
        {
            return this.m_IOrganizationStorage.GetUsersByNoExistsDept(did);
        }

        public void Init()
        {
            string[] strArray = Utility.GetConfig().AppSettings.Settings["OrganizationStorageImpl"].Value.Split(new char[] { ' ' });
            ConstructorInfo constructor = Assembly.Load(strArray[0]).GetType(strArray[1]).GetConstructor(new Type[0]);
            this.m_IOrganizationStorage = constructor.Invoke(new object[0]) as IOrganizationStorage;
        }

        public void UpdateCompanyInfo(string id, string name, string tel, string address, string logo)
        {
            this.m_IOrganizationStorage.UpdateCompanyInfo(id, name, tel, address, logo);
        }

        public void UpdateDept(long did, string name, long pdid, long cindex)
        {
            this.m_IOrganizationStorage.UpdateDept(did, name, pdid, cindex);
        }

        public void UpdateDeptMember(string ids, long did)
        {
            this.m_IOrganizationStorage.UpdateDeptMember(ids, did);
        }

        public static OrganizationImpl Instance
        {
            get
            {
                return m_Instance;
            }
        }
    }
}

