namespace Core.IO
{
    using Core;
    using System;
    using System.IO;

    public static class File
    {
        public static void Copy(string src, string des)
        {
            System.IO.File.Copy(ServerImpl.Instance.MapPath(src), ServerImpl.Instance.MapPath(des));
        }

        public static Stream Create(string path)
        {
            return System.IO.File.Create(ServerImpl.Instance.MapPath(path));
        }

        public static void Delete(string path)
        {
            System.IO.File.Delete(ServerImpl.Instance.MapPath(path));
        }

        public static bool Exists(string path)
        {
            return System.IO.File.Exists(ServerImpl.Instance.MapPath(path));
        }

        public static System.IO.FileAttributes GetAttributes(string path)
        {
            return System.IO.File.GetAttributes(ServerImpl.Instance.MapPath(path));
        }

        public static void Move(string src, string des)
        {
            System.IO.File.Move(ServerImpl.Instance.MapPath(src), ServerImpl.Instance.MapPath(des));
        }

        public static Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return System.IO.File.Open(ServerImpl.Instance.MapPath(path), mode, access, share);
        }

        public static void Rename(string path, string name)
        {
            string sourceFileName = ServerImpl.Instance.MapPath(path);
            System.IO.File.Move(sourceFileName, System.IO.Path.GetDirectoryName(sourceFileName) + @"\" + name);
        }
    }
}

