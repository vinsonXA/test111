namespace Core
{
    using System;
    using System.Web;
    using System.Web.Configuration;

    public class DataBase
    {
        public static string GetSqlServerConnectionString()
        {
            bool flag1 = HttpContext.Current.Request.ApplicationPath == "/";
            return WebConfigurationManager.OpenWebConfiguration("/Lesktop").ConnectionStrings.ConnectionStrings["Lesktop_ConnectString"].ConnectionString;
        }
    }
}

