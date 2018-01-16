namespace Core.IO
{
    using System;
    using System.Text;

    public static class Path
    {
        public static string Format(string path, char split)
        {
            string[] strArray = path.Split(new char[] { split }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new StringBuilder();
            if (path[0] == split)
            {
                builder.Append(split);
            }
            foreach (string str in strArray)
            {
                string str2 = str.Trim();
                if (!string.IsNullOrEmpty(str2) && IsValidNode(str2))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(split);
                    }
                    builder.Append(str2);
                }
            }
            return builder.ToString();
        }

        public static string GetDirectoryName(string path)
        {
            int length = path.LastIndexOf('/');
            return ((length == 0) ? "" : path.Substring(0, length));
        }

        public static string GetExtension(string path)
        {
            string fileName = GetFileName(path);
            int startIndex = fileName.LastIndexOf('.');
            return ((startIndex == -1) ? "." : fileName.Substring(startIndex));
        }

        public static string GetFileName(string path)
        {
            int num = path.LastIndexOf('/');
            return ((num == 0) ? "" : path.Substring(num + 1));
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            string fileName = GetFileName(path);
            int length = fileName.LastIndexOf('.');
            return ((length == -1) ? fileName : fileName.Substring(0, length));
        }

        public static string GetRelativePath(string path)
        {
            if (path.StartsWith("/"))
            {
                int index = path.IndexOf('/', 1);
                return ((index == -1) ? "" : path.Substring(index + 1));
            }
            return path;
        }

        public static string GetRelativePath(string parent, string child)
        {
            if (string.IsNullOrEmpty(parent))
            {
                return child;
            }
            if (parent.Length == child.Length)
            {
                return "";
            }
            return child.Substring(parent.Length + 1);
        }

        public static string GetRoot(string relativePath)
        {
            int index = relativePath.IndexOf("/");
            return ((index == -1) ? relativePath : relativePath.Substring(0, index));
        }

        public static string GetUser(string path)
        {
            if (path.StartsWith("/"))
            {
                int index = path.IndexOf('/', 1);
                return path.Substring(1, (index == -1) ? (path.Length - 1) : (index - 1));
            }
            return string.Empty;
        }

        public static bool IsEqual(string p1, string p2)
        {
            return (string.Compare(p1, p2, true) == 0);
        }

        public static bool IsParent(string parent, string child)
        {
            return (string.IsNullOrEmpty(parent) || (((parent.Length < child.Length) && child.StartsWith(parent, StringComparison.OrdinalIgnoreCase)) && (child[parent.Length] == '/')));
        }

        private static bool IsValidNode(string n)
        {
            foreach (char ch in n)
            {
                if (ch != '.')
                {
                    return true;
                }
            }
            return false;
        }

        public static string Join(string split, params string[] pns)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in pns)
            {
                string str2 = str.Trim();
                if (!string.IsNullOrEmpty(str2) && IsValidNode(str2))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(split);
                    }
                    builder.Append(str2);
                }
            }
            return builder.ToString();
        }

        public static string Join(string split, string[] pns, int index)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = index; i < pns.Length; i++)
            {
                string str = pns[i].Trim();
                if (!string.IsNullOrEmpty(str) && IsValidNode(str))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(split);
                    }
                    builder.Append(str);
                }
            }
            return builder.ToString();
        }
    }
}

