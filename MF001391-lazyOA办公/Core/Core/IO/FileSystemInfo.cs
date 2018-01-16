namespace Core.IO
{
    using System;

    public abstract class FileSystemInfo
    {
        protected FileSystemInfo()
        {
        }

        public abstract FileAttributes Attributes { get; }

        public abstract DateTime CreationTime { get; }

        public abstract DateTime CreationTimeUtc { get; }

        public abstract string FullName { get; }

        public abstract DateTime LastAccessTime { get; }

        public abstract DateTime LastAccessTimeUtc { get; }

        public abstract DateTime LastWriteTime { get; }

        public abstract DateTime LastWriteTimeUtc { get; }

        public abstract string Name { get; }
    }
}

