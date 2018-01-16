namespace Core
{
    using NHibernate;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using WC.Common;

    internal class SqlServerOrganizationStorage : IOrganizationStorage
    {
        private string m_ConnectionString = "";

        public SqlServerOrganizationStorage()
        {
            this.m_ConnectionString = "";
        }

        void IOrganizationStorage.AddDept(string name, long pdid, long cindex)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            string str = "INSERT INTO WM_DEPT(dname,pdid,cindex)values(@dname,@pdid,@cindex)";
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = str
                };
                command.Parameters.Add("dname", DbType.String).Value = name;
                command.Parameters.Add("pdid", DbType.Int64).Value = pdid;
                command.Parameters.Add("cindex", DbType.Int64).Value = cindex;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IOrganizationStorage.DeleteDept(long did)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            string str = "DELETE WM_DEPT WHERE did=@did ;  DELETE WM_UDD WHERE did=@did ";
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = str
                };
                command.Parameters.Add("did", DbType.Int64).Value = did;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        DataTable IOrganizationStorage.GetAllDepts()
        {
            DataTable table2;
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    try
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT CAST(id AS NVARCHAR) as ID,ParentID,DepName FROM Sys_Dep order by Orders asc ;";
                        SqlDataAdapter adapter = new SqlDataAdapter {
                            SelectCommand = command
                        };
                        adapter.Fill(dataTable);
                        adapter.Dispose();
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    return dataTable;
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
            return table2;
        }

        DataTable IOrganizationStorage.GetCompanyInfo()
        {
            DataTable table2;
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    try
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT Id,Name,Tel,Address,Logo FROM WM_Company ;";
                        SqlDataAdapter adapter = new SqlDataAdapter {
                            SelectCommand = command
                        };
                        adapter.Fill(dataTable);
                        adapter.Dispose();
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    return dataTable;
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
            return table2;
        }

        DataRowCollection IOrganizationStorage.GetDeptAllUser(string deptId)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    command.Connection = connection;
                    string str = string.Format("SELECT [Key],Name FROM dbo.WM_Users WHERE [KEY] IN (SELECT uid FROM dbo.WM_UDD WHERE did IN( SELECT * FROM dbo.FUN_GetChildList('Sys_Dep','{0}')))", deptId);
                    command.CommandText = str;
                    SqlDataAdapter adapter = new SqlDataAdapter {
                        SelectCommand = command
                    };
                    adapter.Fill(dataTable);
                    adapter.Dispose();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
            return dataTable.Rows;
        }

        DataTable IOrganizationStorage.GetDeptById(long did)
        {
            DataTable table2;
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    try
                    {
                        command.Connection = connection;
                        command.CommandText = string.Format("SELECT id,depname,orders,parentid FROM sys_dep WHERE id={0}", did);
                        SqlDataAdapter adapter = new SqlDataAdapter {
                            SelectCommand = command
                        };
                        adapter.Fill(dataTable);
                        adapter.Dispose();
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    return dataTable;
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
            return table2;
        }

        DataTable IOrganizationStorage.GetDeptList(string filter)
        {
            DataTable table2;
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    try
                    {
                        command.Connection = connection;
                        command.CommandText = string.Format("SELECT a.id,a.depname,a.orders,a.depname,a.parentid,ISNULL(b.depname,'') AS parentname                       FROM sys_dep a LEFT JOIN sys_dep b ON a.id=b.parentid WHERE 1=1 {0} ORDER BY a.orders;", filter);
                        SqlDataAdapter adapter = new SqlDataAdapter {
                            SelectCommand = command
                        };
                        adapter.Fill(dataTable);
                        adapter.Dispose();
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    return dataTable;
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
            return table2;
        }

        DataRowCollection IOrganizationStorage.GetDepts()
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT a.uid,a.did AS parentid,b.Name,'1' AS dtype,b.EMail,b.AcceptStrangerIM,b.DiskSize,b.HeadIMG,b.HomePage,b.InviteCode,b.IsTemp,b.MsgFileLimit,b.MsgImageLimit,b.Nickname,b.RegisterTime,b.Remark,b.Type,b.UpperName,b.password,b.phone,b.telphone,b.mobilephone,-1 AS orders  FROM WM_UDD a   LEFT JOIN WM_Users b ON a.uid=b.[key]  UNION ALL SELECT CAST(id AS NVARCHAR) AS id,parentid,DepName as name,'0' AS dtype,'' as EMail,'' as AcceptStrangerIM ,'0' as DiskSize,'' as HeadIMG,'' as HomePage,'' as InviteCode,'' as IsTemp,'0' as MsgFileLimit,'0' as MsgImageLimit, DepName as Nickname,GETDATE() as RegisterTime,'' as Remark,'0' as Type,'' as UpperName,''as password,'' as phone,'' as telphone,'' as mobilephone,Orders  FROM Sys_Dep order by Orders asc ;";
                    SqlDataAdapter adapter = new SqlDataAdapter {
                        SelectCommand = command
                    };
                    adapter.Fill(dataTable);
                    adapter.Dispose();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
            return dataTable.Rows;
        }

        DataTable IOrganizationStorage.GetUsersByDeptId(long did)
        {
            DataTable table2;
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    try
                    {
                        command.Connection = connection;
                        command.CommandText = string.Format("SELECT [Key],Name,Nickname FROM WM_Users WHERE  [type]=0 AND  [Key] IN(SELECT uid FROM dbo.WM_UDD WHERE did={0})", did);
                        SqlDataAdapter adapter = new SqlDataAdapter {
                            SelectCommand = command
                        };
                        adapter.Fill(dataTable);
                        adapter.Dispose();
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    return dataTable;
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
            return table2;
        }

        DataTable IOrganizationStorage.GetUsersByNoExistsDept(long did)
        {
            DataTable table2;
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    try
                    {
                        command.Connection = connection;
                        command.CommandText = string.Format("SELECT [Key],Name,Nickname FROM WM_Users WHERE  [type]=0 AND [Key] NOT IN(SELECT uid FROM dbo.WM_UDD WHERE did={0}) ", did);
                        SqlDataAdapter adapter = new SqlDataAdapter {
                            SelectCommand = command
                        };
                        adapter.Fill(dataTable);
                        adapter.Dispose();
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                    return dataTable;
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
            return table2;
        }

        void IOrganizationStorage.UpdateCompanyInfo(string id, string name, string tel, string address, string logo)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            string str = "";
            if (id == "")
            {
                str = "INSERT INTO WM_Company(name,tel,address,logo)VALUES(@name,@tel,@address,@logo)";
            }
            else
            {
                str = "UPDATE WM_Company SET name=@name,tel=@tel,address=@address,logo=@logo where id=@id";
            }
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = str
                };
                if (id != "")
                {
                    command.Parameters.Add("id", DbType.String).Value = id;
                }
                command.Parameters.Add("name", DbType.String).Value = name;
                command.Parameters.Add("tel", DbType.String).Value = tel;
                command.Parameters.Add("address", DbType.String).Value = address;
                command.Parameters.Add("logo", DbType.String).Value = logo;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IOrganizationStorage.UpdateDept(long did, string name, long pdid, long cindex)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            string str = "UPDATE WM_DEPT SET dname=@dname,pdid=@pdid,cindex=@cindex WHERE did=@did";
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = str
                };
                command.Parameters.Add("dname", DbType.String).Value = name;
                command.Parameters.Add("did", DbType.Int64).Value = did;
                command.Parameters.Add("pdid", DbType.Int64).Value = pdid;
                command.Parameters.Add("cindex", DbType.Int64).Value = cindex;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IOrganizationStorage.UpdateDeptMember(string ids, long did)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in ids.Split(new char[] { ',' }))
            {
                if (str != "")
                {
                    builder.AppendFormat("DELETE FROM WM_UDD WHERE did={0} AND uid={1} ; INSERT INTO WM_UDD(did,uid,tid)VALUES({0},{1},0) ", did, str);
                }
            }
            if (builder.Length > 0)
            {
                ISession session = SessionFactory.OpenSession("WC.Model");
                using (SqlConnection connection = (SqlConnection) session.Connection)
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = builder.ToString()
                    };
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        session.Close();
                        session.Dispose();
                    }
                }
            }
        }
    }
}

