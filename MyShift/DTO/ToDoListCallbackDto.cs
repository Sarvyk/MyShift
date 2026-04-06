using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.DTO
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        public int? ToDoListId { get; set; }
        public static new ToDoListCallbackDto FromString(string input)
        {//На вход принимает строку ввида "{action}|{toDoListId}|{prop2}...". Нужно создать ToDoListCallbackDto с Action = action и ToDoListId = toDoListId.
            string[] values = input.Split('|');
            ToDoListCallbackDto dto = new ToDoListCallbackDto();
            dto.Action = values[0];
            if (values.Length > 1)
                dto.ToDoListId = Int32.Parse(values[1]);
            return dto;
        }
        public override string ToString() => $"{base.ToString()}{(ToDoListId != null ? "|" + ToDoListId : "")}";
    }
}
