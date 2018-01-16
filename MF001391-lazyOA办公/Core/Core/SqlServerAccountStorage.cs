namespace Core
{
    using NHibernate;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using WC.Common;

    internal class SqlServerAccountStorage : IAccountStorage
    {
        private string m_ConnectionString = "";

        public SqlServerAccountStorage()
        {
            this.m_ConnectionString = "";
        }

        private void AddMemberToGroup(SqlConnection conn, SqlTransaction tran, string user, string friend)
        {
            SqlCommand command = new SqlCommand {
                Connection = conn,
                CommandText = "WM_AddFriend",
                Transaction = tran,
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("user", DbType.String).Value = user;
            command.Parameters.Add("friend", DbType.String).Value = friend;
            command.ExecuteNonQuery();
        }

        void IAccountStorage.AddFriend(string user, string friend)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = "WM_AddFriend",
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("user", DbType.String).Value = user;
                command.Parameters.Add("friend", DbType.String).Value = friend;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    command.ExecuteNonQuery();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IAccountStorage.AddUser(string name, string nickname, string password, string email, string phone, string telphone)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    SqlCommand command = new SqlCommand("WM_AddUser", connection) {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("name", DbType.String).Value = name;
                    command.Parameters.Add("password", DbType.String).Value = password;
                    command.Parameters.Add("nickname", DbType.String).Value = nickname;
                    command.Parameters.Add("email", DbType.String).Value = email;
                    command.Parameters.Add("inviteCode", DbType.String).Value = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
                    command.Parameters.Add("phone", DbType.String).Value = phone;
                    command.Parameters.Add("telphone", DbType.String).Value = telphone;
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

        void IAccountStorage.CreateGroup(string creator, string name, string nickname, long isExitGroup)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    SqlCommand command = new SqlCommand("WM_CreateGroup", connection) {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("creator", DbType.String).Value = creator;
                    command.Parameters.Add("name", DbType.String).Value = name;
                    command.Parameters.Add("nickname", DbType.String).Value = nickname;
                    command.Parameters.Add("isExitGroup", DbType.Int64).Value = isExitGroup;
                    command.Parameters.Add("inviteCode", DbType.String).Value = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
                    command.ExecuteNonQuery();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IAccountStorage.CreateTempGroup(string creator, string name, string nickname, string deptId, string userlist)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlTransaction tran = null;
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    tran = connection.BeginTransaction();
                    SqlCommand command = new SqlCommand("WM_CreateTempGroup", connection) {
                        CommandType = CommandType.StoredProcedure,
                        Transaction = tran
                    };
                    command.Parameters.Add("creator", DbType.String).Value = creator;
                    command.Parameters.Add("name", DbType.String).Value = name;
                    command.Parameters.Add("nickname", DbType.String).Value = nickname;
                    command.Parameters.Add("TempGroupCreateDept", DbType.String).Value = deptId;
                    command.Parameters.Add("inviteCode", DbType.String).Value = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
                    command.ExecuteNonQuery();
                    string[] strArray = userlist.Split(new char[] { ',' });
                    foreach (string str in strArray)
                    {
                        if (str.ToUpper() != creator.ToUpper())
                        {
                            this.AddMemberToGroup(connection, tran, str, name);
                        }
                    }
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IAccountStorage.CreateUser(string name, string nickname, string password, string email)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    SqlCommand command = new SqlCommand("WM_CreateUser", connection) {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("name", DbType.String).Value = name;
                    command.Parameters.Add("password", DbType.String).Value = password;
                    command.Parameters.Add("nickname", DbType.String).Value = nickname;
                    command.Parameters.Add("email", DbType.String).Value = email;
                    command.Parameters.Add("inviteCode", DbType.String).Value = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
                    command.ExecuteNonQuery();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IAccountStorage.DeleteFriend(long userId, long friendId)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = "WM_DeleteFriend",
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("user", DbType.Int64).Value = userId;
                command.Parameters.Add("friend", DbType.Int64).Value = friendId;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    command.ExecuteNonQuery();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IAccountStorage.DeleteGroup(long id)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    SqlCommand command = new SqlCommand("WM_DeleteGroup") {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("id", DbType.Int64).Value = id;
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        void IAccountStorage.DeleteUser(long id)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    SqlCommand command = new SqlCommand("WM_DeleteUser") {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("id", DbType.Int64).Value = id;
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        DataRow IAccountStorage.GetAccountInfo(long key)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = "WM_GetAccountInfoByID",
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("id", DbType.Int64).Value = key;
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
            return ((dataTable.Rows.Count > 0) ? dataTable.Rows[0] : null);
        }

        DataRow IAccountStorage.GetAccountInfo(string name)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "WM_GetAccountInfoByName"
                    };
                    command.Parameters.Add("name", DbType.String).Value = name;
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
            return ((dataTable.Rows.Count > 0) ? dataTable.Rows[0] : null);
        }

        DataRowCollection IAccountStorage.GetAllGroups()
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = "WM_GetAllGroups",
                        CommandType = CommandType.StoredProcedure
                    };
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

        DataTable IAccountStorage.GetAllUserByName(string Name)
        {
            DataTable table2;
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    try
                    {
                        SqlCommand command = new SqlCommand {
                            Connection = connection,
                            CommandText = "WM_GetAllUsersByName",
                            CommandType = CommandType.StoredProcedure
                        };
                        command.Parameters.Add("@Name", DbType.String).Value = Name;
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

        DataRowCollection IAccountStorage.GetAllUsers()
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = "WM_GetAllUsers",
                        CommandType = CommandType.StoredProcedure
                    };
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

        DataRowCollection IAccountStorage.GetFriends(string name)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = "WM_GetFriends",
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("name", DbType.String).Value = name;
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

        string[] IAccountStorage.GetGroupManagers(string name)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = "WM_GetGroupManagers",
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("name", DbType.String).Value = name;
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
            List<string> list = new List<string>();
            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(row["Name"] as string);
            }
            return list.ToArray();
        }

        string IAccountStorage.GetGroupTempNameByDeptId(string deptId)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = string.Format("SELECT Name FROM dbo.WM_Users WHERE TempGroupCreateDept='{0}'", deptId)
                };
                SqlCommand command2 = command;
                SqlDataAdapter adapter = new SqlDataAdapter {
                    SelectCommand = command2
                };
                SqlDataAdapter adapter2 = adapter;
                try
                {
                    adapter2.Fill(dataTable);
                    adapter2.Dispose();
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
            return ((dataTable.Rows.Count > 0) ? dataTable.Rows[0][0].ToString() : "");
        }

        List<string> IAccountStorage.GetIMWindowRoles(string name)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = string.Format("SELECT DISTINCT Action FROM WM_Control WHERE GId IN( SELECT gid FROM DUG WHERE uid=(SELECT uid FROM DUSER WHERE uname='{0}'))", name)
                };
                SqlCommand command2 = command;
                SqlDataAdapter adapter = new SqlDataAdapter {
                    SelectCommand = command2
                };
                SqlDataAdapter adapter2 = adapter;
                try
                {
                    adapter2.Fill(dataTable);
                    adapter2.Dispose();
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
            List<string> list = new List<string>();
            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(row["Action"] as string);
            }
            return list;
        }

        long IAccountStorage.GetRelationship(string account1, string account2)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = "WM_GetRelationship",
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("account1", DbType.String).Value = account1;
                    command.Parameters.Add("account2", DbType.String).Value = account2;
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
            return ((dataTable.Rows.Count > 0) ? Convert.ToInt64(dataTable.Rows[0]["Relationship"]) : -1L);
        }

        string[] IAccountStorage.GetUserRoles(string name)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = "WM_GetUserRoles",
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("name", DbType.String).Value = name;
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
            List<string> list = new List<string>();
            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(row["RoleName"] as string);
            }
            return list.ToArray();
        }

        void IAccountStorage.UpdateUserInfo(string name, Hashtable values)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                try
                {
                    if (values.ContainsKey("Password"))
                    {
                        if (!values.ContainsKey("PreviousPassword"))
                        {
                            throw new Exception("原密码错误！");
                        }
                        SqlCommand command = new SqlCommand {
                            Connection = connection,
                            CommandText = "select [Key] from WM_Users where UpperName = @Name and Password = @Password"
                        };
                        command.Parameters.Add("Name", DbType.String).Value = name.ToUpper();
                        command.Parameters.Add("Password", DbType.String).Value = Utility.MD5(values["PreviousPassword"].ToString());
                        if (command.ExecuteScalar() == null)
                        {
                            throw new Exception("原密码错误！");
                        }
                    }
                    StringBuilder builder = new StringBuilder();
                    builder.Append("update WM_Users set Name = Name");
                    if (values.ContainsKey("Nickname"))
                    {
                        builder.Append(",Nickname = @Nickname");
                    }
                    if (values.ContainsKey("Password"))
                    {
                        builder.Append(",Password = @Password");
                    }
                    if (values.ContainsKey("EMail"))
                    {
                        builder.Append(",EMail = @EMail");
                    }
                    if (values.ContainsKey("InviteCode"))
                    {
                        builder.Append(",InviteCode = @InviteCode");
                    }
                    if (values.ContainsKey("AcceptStrangerIM"))
                    {
                        builder.Append(",AcceptStrangerIM = @AcceptStrangerIM");
                    }
                    if (values.ContainsKey("MsgFileLimit"))
                    {
                        builder.Append(",MsgFileLimit = @MsgFileLimit");
                    }
                    if (values.ContainsKey("MsgImageLimit"))
                    {
                        builder.Append(",MsgImageLimit = @MsgImageLimit");
                    }
                    if (values.ContainsKey("HomePage"))
                    {
                        builder.Append(",HomePage = @HomePage");
                    }
                    if (values.ContainsKey("HeadIMG"))
                    {
                        builder.Append(",HeadIMG = @HeadIMG");
                    }
                    if (values.ContainsKey("Remark"))
                    {
                        builder.Append(",Remark = @Remark");
                    }
                    if (values.ContainsKey("Phone"))
                    {
                        builder.Append(",Phone = @Phone");
                    }
                    if (values.ContainsKey("TelPhone"))
                    {
                        builder.Append(",TelPhone = @TelPhone");
                    }
                    if (values.ContainsKey("MobilePhone"))
                    {
                        builder.Append(",MobilePhone = @MobilePhone");
                    }
                    if (values.ContainsKey("IsExitGroup"))
                    {
                        builder.Append(",IsExitGroup = @IsExitGroup");
                    }
                    builder.Append(" where UpperName=@UpperName");
                    if (values.ContainsKey("PreviousPassword"))
                    {
                        builder.Append(" and Password = @PreviousPassword");
                    }
                    SqlCommand command2 = new SqlCommand {
                        Connection = connection,
                        CommandText = builder.ToString()
                    };
                    if (values.ContainsKey("Nickname"))
                    {
                        command2.Parameters.Add("Nickname", DbType.String).Value = values["Nickname"];
                    }
                    if (values.ContainsKey("Password"))
                    {
                        command2.Parameters.Add("Password", DbType.String).Value = Utility.MD5(values["Password"] as string);
                    }
                    if (values.ContainsKey("EMail"))
                    {
                        command2.Parameters.Add("EMail", DbType.String).Value = values["EMail"];
                    }
                    if (values.ContainsKey("InviteCode"))
                    {
                        command2.Parameters.Add("InviteCode", DbType.String).Value = values["InviteCode"];
                    }
                    if (values.ContainsKey("AcceptStrangerIM"))
                    {
                        command2.Parameters.Add("AcceptStrangerIM", DbType.Int64).Value = ((bool) values["AcceptStrangerIM"]) ? 1 : 0;
                    }
                    if (values.ContainsKey("MsgFileLimit"))
                    {
                        command2.Parameters.Add("MsgFileLimit", DbType.Int64).Value = Convert.ToInt64((double) values["MsgFileLimit"]);
                    }
                    if (values.ContainsKey("MsgImageLimit"))
                    {
                        command2.Parameters.Add("MsgImageLimit", DbType.Int64).Value = Convert.ToInt64((double) values["MsgImageLimit"]);
                    }
                    if (values.ContainsKey("HomePage"))
                    {
                        command2.Parameters.Add("HomePage", DbType.String).Value = values["HomePage"];
                    }
                    if (values.ContainsKey("HeadIMG"))
                    {
                        command2.Parameters.Add("HeadIMG", DbType.String).Value = values["HeadIMG"];
                    }
                    if (values.ContainsKey("Remark"))
                    {
                        command2.Parameters.Add("Remark", DbType.String).Value = values["Remark"];
                    }
                    if (values.ContainsKey("Phone"))
                    {
                        command2.Parameters.Add("Phone", DbType.String).Value = values["Phone"];
                    }
                    if (values.ContainsKey("TelPhone"))
                    {
                        command2.Parameters.Add("TelPhone", DbType.String).Value = values["TelPhone"];
                    }
                    if (values.ContainsKey("MobilePhone"))
                    {
                        command2.Parameters.Add("MobilePhone", DbType.String).Value = values["MobilePhone"];
                    }
                    if (values.ContainsKey("IsExitGroup"))
                    {
                        command2.Parameters.Add("IsExitGroup", DbType.Int64).Value = ((bool) values["IsExitGroup"]) ? 1 : 0;
                    }
                    command2.Parameters.Add("UpperName", DbType.String).Value = name.ToUpper();
                    if (values.ContainsKey("PreviousPassword"))
                    {
                        command2.Parameters.Add("PreviousPassword", DbType.String).Value = Utility.MD5(values["PreviousPassword"] as string);
                    }
                    command2.ExecuteNonQuery();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        bool IAccountStorage.Validate(string name, string password)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                try
                {
                    SqlCommand command = new SqlCommand {
                        Connection = connection,
                        CommandText = "WM_Validate",
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add("name", DbType.String).Value = name;
                    command.Parameters.Add("password", DbType.String).Value = password;
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
            return (dataTable.Rows.Count > 0);
        }
    }
}

