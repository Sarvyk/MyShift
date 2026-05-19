using MyShift.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Models
{
    public class Shift
    {
        public int Id { get; set; }
        public int UserScheduleId { get; set; }
        public UserSchedule UserSchedule { get; set; }
        public DateTime ShiftDate { get; set; } // дата смены
        public TimeSpan? StartTime { get; set; } // время начала (например, 09:00)
        public TimeSpan? EndTime { get; set; }   // время окончания (например, 18:00)
        public ShiftType ShiftType { get; set; } // "Day", "Night", "Off"
        public bool Status { get; set; } // 0 = активна, 1 = отменена
        public string? CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Shift() { }
        public Shift(int userScheduleId, DateTime shiftDate, TimeSpan? start, TimeSpan? end, ShiftType shiftType, bool status)
        {
            UserScheduleId = userScheduleId;
            ShiftDate = shiftDate;
            StartTime = start;
            EndTime = end;
            ShiftType = shiftType;
            Status = status;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}