using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Entities
{
    internal class DayTemplate
    {
        public string Days { get; set; }//Дни недели
        public string Type { get; set; }//день, ночь
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

    }
}