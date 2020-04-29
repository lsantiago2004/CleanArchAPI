using System;

namespace K2SO.Auth
{
    public class Constants
    {
        [Flags]
        public enum PermissionAccessType
        {
            NONE = 0,
            VIEW = 1,
            CREATE = 2,
            EDIT = 4,
            DELETE = 8,
            ADMIN = VIEW | CREATE | EDIT | DELETE
        }
    }
}
