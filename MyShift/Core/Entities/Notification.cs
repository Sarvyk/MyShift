using Microsoft.EntityFrameworkCore;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Entities
{
    [Index(nameof(Type), Name = "idx_Notification_Type")]
    public class Notification
    {
        public int id { get; set; }
        public ToDoUser user { get; set; }
        public string Type { get; set; }
        public string Text {  get; set; }
        public DateTime ScheduledAt { get; set; }
        public bool IsNotified { get; set; }
        public DateTime? NotifiedAt { get; set; }
    }
}