namespace Core.IO
{
    using Core;
    using System;
    using System.IO;

    public static class Directory
    {
        public static void Copy(string src, string des)
        {
            if (!Exists(des))
            {
                CreateDirectory(des);
            }
            foreach (Core.IO.FileSystemInfo info in new Core.IO.DirectoryInfo(src).GetFileSystemInfos())
            {
                if ((info.Attributes & Core.IO.FileAttributes.Directory) == Core.IO.FileAttributes.Directory)
                {
                    Copy(info.FullName, des + "/" + info.Name);
                }
                else
                {
                    Core.IO.File.Copy(info.FullName, des + "/" + info.Name);
                }
            }
        }

        public static void CreateDirectory(string path)
        {
            System.IO.Directory.CreateDirectory(ServerImpl.Instance.MapPath(path));
        }

        public static void Delete(string path)
        {
            Core.IO.FileSystemInfo[] fileSystemInfos = new Core.IO.DirectoryInfo(path).GetFileSystemInfos();
            foreach (Core.IO.FileSystemInfo info in fileSystemInfos)
            {
                if ((Core.IO.File.GetAttributes(info.FullName) & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                {
                    Delete(info.FullName);
                }
                else
                {
                    Core.IO.File.Delete(info.FullName);
                }
            }
            System.IO.Directory.Delete(ServerImpl.Instance.MapPath(path));
        }

        public static bool Exists(string path)
        {
            return System.IO.Directory.Exists(ServerImpl.Instance.MapPath(path));
        }

        public static void Move(string src, string des)
        {
            if (!Exists(des))
            {
                CreateDirectory(des);
            }
            foreach (Core.IO.FileSystemInfo info in new Core.IO.DirectoryInfo(src).GetFileSystemInfos())
            {
                if ((info.Attributes & Core.IO.FileAttributes.Directory) == Core.IO.FileAttributes.Directory)
                {
                    Move(info.FullName, des + "/" + info.Name);
                }
                else
                {
                    Core.IO.File.Move(info.FullName, des + "/" + info.Name);
                }
            }
            Delete(src);
        }

        public static void Rename(string path, string name)
        {
            string sourceDirName = ServerImpl.Instance.MapPath(path);
            System.IO.Directory.Move(sourceDirName, System.IO.Path.GetDirectoryName(sourceDirName) + @"\" + name);
        }
    }
}

