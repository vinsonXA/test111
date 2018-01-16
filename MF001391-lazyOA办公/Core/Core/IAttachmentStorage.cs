namespace Core
{
    using System;
    using System.Data;

    public interface IAttachmentStorage
    {
        void DeleteAttachment(long id);
        DataTable GetAttachmentById(long id);
        DataTable GetListByGroupId(long id);
        void InsertAttachment(long uid, long uplaodId, string oldName, string saveName, double size);
    }
}

