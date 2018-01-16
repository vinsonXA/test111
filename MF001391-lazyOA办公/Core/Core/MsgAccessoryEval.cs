namespace Core
{
    using Core.IO;
    using Microsoft.JScript;
    using System;
    using System.Collections;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    internal class MsgAccessoryEval
    {
        private Hashtable m_Data;
        private string m_MsgDir;
        private string m_Receiver;
        private string m_ReceiverMsgDir;
        private string m_Sender;
        private string m_SenderMsgDir;

        public MsgAccessoryEval(long key, string receiver, string sender, Hashtable data)
        {
            this.m_Receiver = receiver;
            this.m_Sender = sender;
            this.m_Data = data;
            this.m_ReceiverMsgDir = string.Format("/{0}/Message/MSG{1:00000000}", receiver, key);
            this.m_SenderMsgDir = string.Format("/{0}/Message/MSG{1:00000000}", sender, key);
            this.m_MsgDir = (AccountImpl.Instance.GetUserInfo(receiver).Type > 0L) ? string.Format("/{1}/Message/MSG{0:00000000}", key, receiver) : string.Format("Message/MSG{0:00000000}", key);
        }

        public string Replace(Match match)
        {
            string fileName;
            XmlDocument document = new XmlDocument();
            string str2 = match.Value;
            document.LoadXml(string.Format("<{0} />", str2.Substring(1, str2.Length - 2)));
            string path = GlobalObject.unescape(document.DocumentElement.GetAttribute("src"));
            string str4 = document.DocumentElement.GetAttribute("type").ToLower();
            string attribute = document.DocumentElement.GetAttribute("data");
            if (ServerImpl.Instance.IsPublic(path))
            {
                return string.Format("{0}", Core.IO.Path.GetRelativePath(path));
            }
            if (!Core.IO.Directory.Exists(this.m_ReceiverMsgDir))
            {
                Core.IO.Directory.CreateDirectory(this.m_ReceiverMsgDir);
            }
            if (!((AccountImpl.Instance.GetUserInfo(this.m_Receiver).Type != 0L) || Core.IO.Directory.Exists(this.m_SenderMsgDir)))
            {
                Core.IO.Directory.CreateDirectory(this.m_SenderMsgDir);
            }
            if (string.IsNullOrEmpty(Core.IO.Path.GetUser(path)))
            {
                string sender = this.m_Sender;
                path = string.Format("/{0}/{1}", sender, path);
            }
            bool flag = true;
            try
            {
                ServerImpl.Instance.CheckPermission(HttpContext.Current, path, 4);
            }
            catch
            {
                flag = false;
            }
            if (!(!(attribute == "") || flag))
            {
                return path;
            }
            Hashtable hashtable = new Hashtable();
            if (!hashtable.ContainsKey(path))
            {
                fileName = Core.IO.Path.GetFileName(path);
                for (int i = 1; hashtable.ContainsValue(fileName); i++)
                {
                    fileName = string.Format("{0}({1}){2}", System.IO.Path.GetFileNameWithoutExtension(fileName), i.ToString(), System.IO.Path.GetExtension(fileName));
                }
                hashtable.Add(path, fileName);
                try
                {
                    string str7;
                    byte[] buffer;
                    Stream stream;
                    Stream stream2;
                    if (AccountImpl.Instance.GetUserInfo(this.m_Receiver).Type == 0L)
                    {
                        if (attribute == "")
                        {
                            Core.IO.File.Copy(path, this.m_SenderMsgDir + "/" + fileName);
                        }
                        else
                        {
                            str7 = this.m_Data[attribute] as string;
                            buffer = System.Convert.FromBase64String(str7);
                            using (stream2 = stream = Core.IO.File.Open(this.m_SenderMsgDir + "/" + fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                try
                                {
                                    stream.Write(buffer, 0, buffer.Length);
                                }
                                finally
                                {
                                    stream.Close();
                                }
                            }
                        }
                    }
                    if (attribute == "")
                    {
                        Core.IO.File.Copy(path, this.m_ReceiverMsgDir + "/" + fileName);
                    }
                    else
                    {
                        str7 = this.m_Data[attribute] as string;
                        buffer = System.Convert.FromBase64String(str7);
                        using (stream2 = stream = Core.IO.File.Open(this.m_ReceiverMsgDir + "/" + fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            try
                            {
                                stream.Write(buffer, 0, buffer.Length);
                            }
                            finally
                            {
                                stream.Close();
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                fileName = hashtable[path] as string;
            }
            return GlobalObject.escape(string.Format("{0}/{1}", this.m_MsgDir, fileName));
        }
    }
}

