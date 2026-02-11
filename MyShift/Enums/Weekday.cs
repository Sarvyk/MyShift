using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Enums
{
    [Flags]
    public enum Weekday
    {
        none = 0,
        [Display(Name = "Понедельник", ShortName = "Пн")]
        Mondey = 1 << 0,
        [Display(Name = "Вторник", ShortName = "Вт")]
        Tuesday = 1 << 1,
        [Display(Name = "Среда", ShortName = "Ср")]
        Wednesday = 1 << 2,
        [Display(Name = "Четверг", ShortName = "Чт")]
        Thursday = 1 << 3,
        [Display(Name = "Пятница", ShortName = "Пт")]
        Friday = 1 << 4,
        [Display(Name = "Суббота", ShortName = "Сб")]
        Saturday = 1 << 5,
        [Display(Name = "Воскресенье", ShortName = "Вс")]
        Sunday = 1 << 6
    }
}