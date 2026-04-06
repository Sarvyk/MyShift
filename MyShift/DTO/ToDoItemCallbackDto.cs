using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.DTO
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public int ToDoItemId;
        public static new ToDoItemCallbackDto FromString(string input)
        {
            string[] values = input.Split('|');
            ToDoItemCallbackDto dto = new ToDoItemCallbackDto();
            dto.Action = values[0];
            dto.ToDoItemId = Int32.Parse(values[1]);
            return dto;
        }
        public override string ToString() => $"{base.ToString()}|{ToDoItemId}";
    }
}
