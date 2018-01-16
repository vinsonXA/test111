namespace Core
{
    using System;
    using System.Threading;
    using System.Web;

    public class ReceiveResponsesHandler : IHttpAsyncHandler, IHttpHandler
    {
        private HttpContext m_Context = null;

        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            this.m_Context = context;
            string sessionId = context.Request.Params["SessionID"];
            string str2 = context.Request.Params["ClientVersion"];
            string str3 = context.Request.Params["ServerVersion"];
            ResponsesListener listener = new ResponsesListener(sessionId, cb, extraData);
            try
            {
                if (str3 != ServerImpl.Instance.Version)
                {
                    throw new IncompatibleException();
                }
                if (!(string.IsNullOrEmpty(str2) || !(str2 != "1.0.1.7")))
                {
                    throw new IncompatibleException();
                }
                string userName = ServerImpl.Instance.GetUserName(context);
                if (string.IsNullOrEmpty(userName))
                {
                    throw new UnauthorizedException();
                }
                if (SessionManagement.Instance.GetAccountState(userName).Receive(sessionId, listener))
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(listener.Complete));
                }
            }
            catch (Exception exception)
            {
                listener.Cache(Utility.RenderHashJson(new object[] { "IsSucceed", false, "Exception", exception }));
                ThreadPool.QueueUserWorkItem(new WaitCallback(listener.Complete));
            }
            return listener;
        }

        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        {
            (result as ResponsesListener).Send(this.m_Context);
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
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

