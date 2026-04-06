using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Enums
{
    [Flags]
    public enum Role
    {
        None = 0,
        User = 1,
        Moderator = 1 << 1,
        Administrator = 1 << 2
    }
}