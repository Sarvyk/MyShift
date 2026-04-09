using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Entities
{
    internal class CycleTemplate : List<CycleItem>
    { }
    internal class CycleItem
    {
        public string TypeShift { get; set; }
        public TimeSpan? Start { get; set; }
        public TimeSpan? End { get; set; }
    }
}