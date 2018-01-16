namespace Core.IO
{
    using Core;
    using System;
    using System.IO;
    using System.Text;

    public class FileInfo : Core.IO.FileSystemInfo, IRenderJson
    {
        private string _fullName;
        private System.IO.FileInfo _info = null;
        private string _name;
        private string _path;

        public FileInfo(string fullName)
        {
            this._fullName = fullName;
            this._name = Core.IO.Path.GetFileName(fullName);
            this._path = ServerImpl.Instance.MapPath(fullName);
            this._info = new System.IO.FileInfo(this._path);
        }

        void IRenderJson.RenderJson(StringBuilder builder)
        {
            Utility.RenderHashJson(builder, new object[] { "FullName", this.FullName, "Name", this.Name, "Type", "F", "Size", this.Length, "LastModifiedTime", this.LastWriteTime });
        }

        public override Core.IO.FileAttributes Attributes
        {
            get
            {
                return (Core.IO.FileAttributes) this._info.Attributes;
            }
        }

        public override DateTime CreationTime
        {
            get
            {
                return this._info.CreationTime;
            }
        }

        public override DateTime CreationTimeUtc
        {
            get
            {
                return this._info.CreationTimeUtc;
            }
        }

        public override string FullName
        {
            get
            {
                return this._fullName;
            }
        }

        public override DateTime LastAccessTime
        {
            get
            {
                return this._info.LastAccessTime;
            }
        }

        public override DateTime LastAccessTimeUtc
        {
            get
            {
                return this._info.LastAccessTimeUtc;
            }
        }

        public override DateTime LastWriteTime
        {
            get
            {
                return this._info.LastWriteTime;
            }
        }

        public override DateTime LastWriteTimeUtc
        {
            get
            {
                return this._info.LastWriteTimeUtc;
            }
        }

        public long Length
        {
            get
            {
                return this._info.Length;
            }
        }

        public override string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

