using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Enums
{
    [Flags]
    public enum Role
    {
        [Display(Name = "🚫нет роли🚫")]
        None = 0,
        [Display(Name = "👤Пользователь👤")]
        User = 1,
        [Display(Name = "🛡️Модератор🛡️")]
        Moderator = 1 << 1,
        [Display(Name = "⭐Администратор⭐")]
        Administrator = 1 << 2,
        [Display(Name = "👑Супер администратор👑")]
        SuperAdministrator = 1 << 3
    }
}