using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Enums
{
    public enum Weekday
    {
        none = 0,
        [Display(Name = "Понедельник", ShortName = "Пн")]
        Mondey = 1,
        [Display(Name = "Вторник", ShortName = "Вт")]
        Tuesday = 2,
        [Display(Name = "Среда", ShortName = "Ср")]
        Wednesday = 3,
        [Display(Name = "Четверг", ShortName = "Чт")]
        Thursday = 4,
        [Display(Name = "Пятница", ShortName = "Пт")]
        Friday = 5,
        [Display(Name = "Суббота", ShortName = "Сб")]
        Saturday = 6,
        [Display(Name = "Воскресенье", ShortName = "Вс")]
        Sunday = 7
    }
}