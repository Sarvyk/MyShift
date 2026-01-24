using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Enums
{
    [Flags]
    public enum Role
    {
        None = 0,
        User = 1 << 1,
        Moderator = 1 << 2,
        Administrator = 1 << 3
    }
}
