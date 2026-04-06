using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.DTO
{
    internal class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page;
        public PagedListCallbackDto()
        { }
        public PagedListCallbackDto(int page)
        {
            Page = page;
        }
        public static new PagedListCallbackDto FromString(string input)
        {
            string[] values = input.Split('|');
            PagedListCallbackDto dto = new PagedListCallbackDto();
            dto.Action = values[0];
            if (values.Length > 1)
            {
                dto.ToDoListId = (string.IsNullOrWhiteSpace(values[1].Trim()) ? null : Int32.Parse(values[1]));
                dto.Page = Int32.Parse(values[2]);
            }
            return dto;
        }
        public override string ToString() => $"{base.ToString()}{(ToDoListId == null ? "|" : "")}|{Page}";
    }
}