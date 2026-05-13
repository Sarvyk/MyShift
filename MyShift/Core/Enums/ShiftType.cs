using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Enums
{
    public enum ShiftType
    {
        none,
        [Display(Name = "дневная смена☀️", ShortName = "день")]
        day,
        [Display(Name = "ночная смена🌙", ShortName = "ночь")]
        night,
        [Display(Name = "выходной🛌", ShortName = "выходной")]
        off
    }
}