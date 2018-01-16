namespace Core
{
    using System;

    public class PermissionException : Exception
    {
        public PermissionException() : base("权限不足")
        {
        }
    }
}

