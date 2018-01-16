namespace Core
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Xml;

    public class SendCommandHandler : IHttpHandler
    {
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            Exception exception = null;
            string str = null;
            try
            {
                Stream inputStream = context.Request.InputStream;
                byte[] buffer = new byte[inputStream.Length];
                inputStream.Read(buffer, 0, (int) inputStream.Length);
                string xml = context.Request.ContentEncoding.GetString(buffer);
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                string[] strArray = document.DocumentElement.GetAttribute("Handler").Split(new char[] { ' ' });
                string attribute = document.DocumentElement.GetAttribute("ID");
                string str4 = document.DocumentElement.GetAttribute("SessionID");
                bool flag = bool.Parse(document.DocumentElement.GetAttribute("IsAsyn"));
                CommandHandler handler = Assembly.Load(strArray[0]).GetType(strArray[1]).GetConstructor(new Type[] { typeof(HttpContext), typeof(string), typeof(string), typeof(string) }).Invoke(new object[] { context, str4, attribute, document.DocumentElement.InnerXml }) as CommandHandler;
                if (flag)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(handler.Process));
                }
                else
                {
                    str = handler.Process();
                }
            }
            catch (Exception exception2)
            {
                exception = exception2;
            }
            if (exception == null)
            {
                context.Response.Write(Utility.RenderHashJson(new object[] { "IsSucceed", true, "Data", new JsonText(str) }));
            }
            else
            {
                context.Response.Write(Utility.RenderHashJson(new object[] { "IsSucceed", false, "Exception", exception }));
            }
        }

        bool IHttpHandler.IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}

