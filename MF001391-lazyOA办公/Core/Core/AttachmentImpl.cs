namespace Core
{
    using System;
    using System.Data;
    using System.Reflection;

    public class AttachmentImpl
    {
        private IAttachmentStorage m_IAttachmentStorage = null;
        private static AttachmentImpl m_Instance = new AttachmentImpl();

        private AttachmentImpl()
        {
            this.Init();
        }

        public void DeleteAttachment(long id)
        {
            this.m_IAttachmentStorage.DeleteAttachment(id);
        }

        public DataTable GetAttachmentById(long id)
        {
            return this.m_IAttachmentStorage.GetAttachmentById(id);
        }

        public DataTable GetListByGroupId(long id)
        {
            return this.m_IAttachmentStorage.GetListByGroupId(id);
        }

        public void Init()
        {
            string[] strArray = Utility.GetConfig().AppSettings.Settings["AttachmentImpl"].Value.Split(new char[] { ' ' });
            ConstructorInfo constructor = Assembly.Load(strArray[0]).GetType(strArray[1]).GetConstructor(new Type[0]);
            this.m_IAttachmentStorage = constructor.Invoke(new object[0]) as IAttachmentStorage;
        }

        public void InsertAttachment(long uid, long uplaodId, string oldName, string saveName, double size)
        {
            this.m_IAttachmentStorage.InsertAttachment(uid, uplaodId, oldName, saveName, size);
        }

        public static AttachmentImpl Instance
        {
            get
            {
                return m_Instance;
            }
        }
    }
}

