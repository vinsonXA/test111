namespace Core
{
    using NHibernate;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using WC.Common;

    public class SqlServerMessageStorage : IMessageStorage
    {
        private string m_ConnectionString = "";
        private DateTime m_MaxCreatedTime = DateTime.Now;
        private long m_MaxKey = 1L;

        public SqlServerMessageStorage()
        {
            this.m_ConnectionString = "";
            ISession session = SessionFactory.OpenSession("WC.Model");
            SqlConnection connection = (SqlConnection) session.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            try
            {
                SqlDataReader reader = new SqlCommand("select max([Key]) as MaxKey, max(CreatedTime) as MaxCreatedTime from WM_Message", connection).ExecuteReader();
                if (reader.Read())
                {
                    this.m_MaxKey = (reader[0] == DBNull.Value) ? 1L : Convert.ToInt64(reader[0]);
                    this.m_MaxCreatedTime = (reader[1] == DBNull.Value) ? DateTime.Now : Convert.ToDateTime(reader[1]);
                }
            }
            finally
            {
                session.Close();
                session.Dispose();
            }
        }

        List<Message> IMessageStorage.Find(long receiver, long sender, DateTime? from)
        {
            List<Message> list;
            ISession session = SessionFactory.OpenSession("WC.Model");
            SqlConnection connection = (SqlConnection) session.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            try
            {
                if (!from.HasValue)
                {
                    from = new DateTime(0x7d0, 1, 1);
                }
                SqlCommand command = new SqlCommand("WM_FindMessages", connection) {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("user", DbType.Int32).Value = receiver;
                command.Parameters.Add("peer", DbType.Int32).Value = sender;
                command.Parameters.Add("from", DbType.DateTime).Value = from.Value;
                List<Message> list2 = new List<Message>();
                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult);
                try
                {
                    while (reader.Read())
                    {
                        Message item = new Message(AccountImpl.Instance.GetUserInfo(Convert.ToInt64(reader[2])), AccountImpl.Instance.GetUserInfo(Convert.ToInt64(reader[1])), reader.GetString(3), Convert.ToDateTime(reader[4]), Convert.ToInt64(reader[0]));
                        list2.Add(item);
                    }
                }
                finally
                {
                    reader.Close();
                }
                list = list2;
            }
            finally
            {
                session.Close();
                session.Dispose();
            }
            return list;
        }

        List<Message> IMessageStorage.FindHistory(long user, long peer, DateTime from, DateTime to)
        {
            List<Message> list;
            ISession session = SessionFactory.OpenSession("WC.Model");
            SqlConnection connection = (SqlConnection) session.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            try
            {
                SqlCommand command = new SqlCommand("WM_FindHistory", connection) {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("user", DbType.Int64).Value = user;
                command.Parameters.Add("peer", DbType.Int64).Value = peer;
                command.Parameters.Add("from", DbType.DateTime).Value = from;
                command.Parameters.Add("to", DbType.DateTime).Value = to;
                List<Message> list2 = new List<Message>();
                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult);
                try
                {
                    while (reader.Read())
                    {
                        Message item = new Message(AccountImpl.Instance.GetUserInfo(Convert.ToInt64(reader[2])), AccountImpl.Instance.GetUserInfo(Convert.ToInt64(reader[1])), reader.GetString(3), Convert.ToDateTime(reader[4]), Convert.ToInt64(reader[0]));
                        list2.Add(item);
                    }
                }
                finally
                {
                    reader.Close();
                }
                list = list2;
            }
            finally
            {
                session.Close();
                session.Dispose();
            }
            return list;
        }

        DateTime IMessageStorage.GetCreatedTime()
        {
            return this.m_MaxCreatedTime;
        }

        long IMessageStorage.GetMaxKey()
        {
            return this.m_MaxKey;
        }

        void IMessageStorage.Write(List<Message> messages)
        {
            ISession session = SessionFactory.OpenSession("WC.Model");
            SqlConnection connection = (SqlConnection) session.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            DataTable dataTable = this.GetDataTable();
            foreach (Message message in messages)
            {
                DataRow row = dataTable.NewRow();
                row["Receiver"] = message.Receiver.ID;
                row["Sender"] = message.Sender.ID;
                row["Content"] = message.Content;
                row["CreatedTime"] = message.CreatedTime;
                row["Key"] = message.Key;
                dataTable.Rows.Add(row);
            }
            try
            {
                SqlTransaction externalTransaction = connection.BeginTransaction();
                try
                {
                    using (SqlBulkCopy copy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, externalTransaction))
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            copy.ColumnMappings.Add(dataTable.Columns[i].ColumnName, dataTable.Columns[i].ColumnName);
                        }
                        copy.DestinationTableName = "WM_Message";
                        copy.WriteToServer(dataTable);
                    }
                    externalTransaction.Commit();
                }
                catch
                {
                    externalTransaction.Rollback();
                }
            }
            finally
            {
                session.Close();
                session.Dispose();
            }
        }

        public List<Message> FindHistory(string peerType, string content, DateTime from, DateTime to, string user)
        {
            List<Message> list;
            ISession session = SessionFactory.OpenSession("WC.Model");
            SqlConnection connection = (SqlConnection) session.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            try
            {
                SqlCommand command = new SqlCommand("WM_FindHistoryB", connection) {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("peerType", DbType.Int64).Value = peerType;
                command.Parameters.Add("content", DbType.String).Value = content;
                command.Parameters.Add("user", DbType.String).Value = user;
                command.Parameters.Add("from", DbType.DateTime).Value = from;
                command.Parameters.Add("to", DbType.DateTime).Value = to;
                List<Message> list2 = new List<Message>();
                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult);
                try
                {
                    while (reader.Read())
                    {
                        Message item = new Message(AccountImpl.Instance.GetUserInfo(Convert.ToInt64(reader[2])), AccountImpl.Instance.GetUserInfo(Convert.ToInt64(reader[1])), reader.GetString(3), Convert.ToDateTime(reader[4]), Convert.ToInt64(reader[0]));
                        list2.Add(item);
                    }
                }
                finally
                {
                    reader.Close();
                }
                list = list2;
            }
            finally
            {
                session.Close();
                session.Dispose();
            }
            return list;
        }

        private DataTable GetDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Receiver");
            table.Columns.Add("Sender");
            table.Columns.Add("Content");
            table.Columns.Add("CreatedTime");
            table.Columns.Add("Key");
            return table;
        }

        private string ConnectionString
        {
            get
            {
                return this.m_ConnectionString;
            }
        }
    }
}

