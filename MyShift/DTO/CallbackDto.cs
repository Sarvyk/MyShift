using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.DTO
{
    internal class CallbackDto
    {
        public string Action { get; set; }

        public static CallbackDto FromString(string input)
        {
            string[] values = input.Split('|');
            CallbackDto dto = new CallbackDto();
            if (values.Length == 0)
                dto.Action = input;
            else
                dto.Action = values[0];
            return dto;
        }
        public override string ToString() => Action;
    }
}
