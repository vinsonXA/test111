namespace Core
{
    using Core.IO;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.Configuration;

    public class ServerImpl
    {
        private string _fileRoot;
        private object _logLock = new object();
        private string _serviceUrl = "";
        private string m_BaseDirecotry = string.Empty;
        private static ServerImpl m_Instance = new ServerImpl();
        private int PublicSubItemsPermission = 4;
        private int RootDirectoryPermission = 4;
        private int RootPublicSubItemsPermission = 4;
        private int RootSubItemsPermission = 12;

        static ServerImpl()
        {
            Instance.Initialize(HttpContext.Current);
        }

        private ServerImpl()
        {
        }

        public void CheckPermission(HttpContext context, string path, int action)
        {
            string relativePath = Core.IO.Path.GetRelativePath(path);
            string strB = Core.IO.Path.GetRoot(relativePath).ToLower();
            string str4 = strB;
            if ((str4 == null) || ((str4 != "pub") && (str4 != "public")))
            {
                AccountInfo currentUser = Instance.GetCurrentUser(context);
                string user = Core.IO.Path.GetUser(path);
                if (string.IsNullOrEmpty(user))
                {
                    user = currentUser.Name;
                }
                AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(user);
                if ((userInfo.Type == 0L) || !userInfo.ContainsMember(currentUser.Name))
                {
                    if (userInfo.ID != currentUser.ID)
                    {
                        throw new PermissionException();
                    }
                    if (string.IsNullOrEmpty(relativePath) && ((this.RootDirectoryPermission & action) != action))
                    {
                        throw new PermissionException();
                    }
                    if (string.Compare(relativePath, strB, true) == 0)
                    {
                        if (strB == "public")
                        {
                            if ((this.RootPublicSubItemsPermission & action) != action)
                            {
                                throw new PermissionException();
                            }
                        }
                        else if ((this.RootSubItemsPermission & action) != action)
                        {
                            throw new PermissionException();
                        }
                    }
                    else if ((strB == "public") && ((this.PublicSubItemsPermission & action) != action))
                    {
                        throw new PermissionException();
                    }
                }
            }
        }

        public void Dispose(HttpApplication app)
        {
        }

        public AccountInfo GetCurrentUser(HttpContext context)
        {
            return AccountImpl.Instance.GetUserInfo(this.GetUserName(context));
        }

        public string GetFileRoot(HttpContext context)
        {
            string str;
            bool flag1 = context.Request.ApplicationPath == "/";
            string str2 = WebConfigurationManager.OpenWebConfiguration("/Lesktop").AppSettings.Settings["FileRoot"].Value;
            if (string.IsNullOrEmpty(str2))
            {
                str = context.Server.MapPath("~");
                while (str.EndsWith(@"\"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                return (System.IO.Path.GetDirectoryName(str) + @"\Lesktop\Files");
            }
            if (!System.IO.Path.IsPathRooted(str2))
            {
                str = context.Server.MapPath("~");
                while (str.EndsWith(@"\"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                str = str + @"\Lesktop";
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(str, str2));
                System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(System.IO.Path.Combine(str, str2));
                return info.FullName;
            }
            return str2;
        }

        public string GetFullPath(HttpContext context, string path)
        {
            return (string.IsNullOrEmpty(Core.IO.Path.GetUser(path)) ? string.Format("/{0}/{1}", this.GetUserName(context), path) : path);
        }

        public string GetResourceUrl(string path)
        {
            return string.Format("\"{0}/{1}\"", this.ResPath, path);
        }

        public string GetUserName(HttpContext context)
        {
            if (context.Request.Cookies["Lesktop"] != null)
            {
                return context.Request.Cookies["Lesktop"].Value;
            }
            return string.Empty;
        }

        public void Initialize(HttpContext context)
        {
            this.m_BaseDirecotry = context.Server.MapPath("~");
            this._fileRoot = this.GetFileRoot(context);
            this._serviceUrl = context.Request.ApplicationPath;
            if (!this._serviceUrl.EndsWith("/"))
            {
                this._serviceUrl = this._serviceUrl + "/";
            }
            this._serviceUrl = this._serviceUrl + "Lesktop";
            AccountImpl.Instance.Init();
        }

        public bool IsPublic(string path)
        {
            return (Core.IO.Path.GetRoot(Core.IO.Path.GetRelativePath(path)).ToLower() == "public");
        }

        public void Login(string sessionId, HttpContext context, string user, DateTime? expires)
        {
            this.Login(sessionId, context, user, expires, true);
        }

        public void Login(string sessionId, HttpContext context, string user, DateTime? expires, bool startSession)
        {
            HttpCookie cookie = new HttpCookie("Lesktop", user);
            if (expires.HasValue)
            {
                cookie.Expires = expires.Value;
            }
            context.Response.Cookies.Add(cookie);
            if (startSession)
            {
                SessionManagement.Instance.GetAccountState(user).NewSession(sessionId);
            }
        }

        public void Logout(HttpContext context)
        {
            HttpCookie cookie = new HttpCookie("Lesktop", "") {
                Expires = DateTime.Now.AddDays(-7.0)
            };
            context.Response.Cookies.Add(cookie);
        }

        public string MapPath(string path)
        {
            string[] pns = Core.IO.Path.GetRelativePath(path).Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if ((pns.Length > 0) && (pns[0].ToLower() == "public"))
            {
                return Core.IO.Path.Join(@"\", new string[] { this._fileRoot, "Public", Core.IO.Path.Join(@"\", pns, 1) });
            }
            if ((pns.Length > 0) && (pns[0].ToLower() == "temp"))
            {
                return Core.IO.Path.Join(@"\", new string[] { this._fileRoot, "Temp", Core.IO.Path.GetUser(path), Core.IO.Path.Join(@"\", pns, 1) });
            }
            string user = Core.IO.Path.GetUser(path);
            return string.Format(@"{0}\Users\{1}\{2}", this._fileRoot, user, Core.IO.Path.Join(@"\", pns, 0));
        }

        public string ReplaceVersion(string path)
        {
            return path.Replace("{VERSION}", "CurrentVersion");
        }

        public void WriteLog(string text)
        {
        }

        public string BaseDirecotry
        {
            get
            {
                return this.m_BaseDirecotry;
            }
        }

        public string Debug
        {
            get
            {
                return "true";
            }
        }

        public static ServerImpl Instance
        {
            get
            {
                return m_Instance;
            }
        }

        public string ResPath
        {
            get
            {
                bool flag1 = HttpContext.Current.Request.ApplicationPath == "/";
                return WebConfigurationManager.OpenWebConfiguration("/Lesktop").AppSettings.Settings["ResPath"].Value;
            }
        }

        public string ServiceUrl
        {
            get
            {
                return this._serviceUrl;
            }
        }

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
    }
}

