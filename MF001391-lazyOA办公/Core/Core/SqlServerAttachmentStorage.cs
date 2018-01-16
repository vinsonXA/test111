namespace Core
{
    using NHibernate;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using WC.Common;

    public class SqlServerAttachmentStorage : IAttachmentStorage
    {
        private string m_ConnectionString = "";

        public SqlServerAttachmentStorage()
        {
            this.m_ConnectionString = "";
        }

        void IAttachmentStorage.DeleteAttachment(long id)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            string str = string.Format("DELET FROM WM_Attachment WHERE id={0}", id);
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = str
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

        DataTable IAttachmentStorage.GetAttachmentById(long id)
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
                        command.CommandText = string.Format("SELECT  id,uid,uplaodId,oldname,savename,savetime,size FROM WM_Attachment WHERE id={0}", id);
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

        DataTable IAttachmentStorage.GetListByGroupId(long id)
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
                        command.CommandText = string.Format("SELECT id,uid,uplaodId,oldname,savename,savetime,size FROM WM_Attachment WHERE uid={0}", id);
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

        void IAttachmentStorage.InsertAttachment(long uid, long uplaodId, string oldName, string saveName, double size)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            string str = "INSERT INTO WM_Attachment(uid,uplaodid,oldname,savename,savetime,size)values(@uid,@uplaodid,@oldname,@savename,getdate(),@size)";
            using (SqlConnection connection = (SqlConnection) session.Connection)
            {
                SqlCommand command = new SqlCommand {
                    Connection = connection,
                    CommandText = str
                };
                command.Parameters.Add("uid", DbType.Int64).Value = uid;
                command.Parameters.Add("uplaodid", DbType.Int64).Value = uplaodId;
                command.Parameters.Add("oldname", DbType.String).Value = oldName;
                command.Parameters.Add("savename", DbType.String).Value = saveName;
                command.Parameters.Add("size", DbType.Double).Value = size;
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

