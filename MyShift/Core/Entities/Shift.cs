using Microsoft.EntityFrameworkCore;
using MyShift.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Models
{
    [Index(nameof(UserScheduleId), Name = "idx_Shift_UserScheduleId")]
    public class Shift
    {
        public int Id { get; set; }
        public int UserScheduleId { get; set; }
        public UserSchedule UserSchedule { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public ShiftType ShiftType { get; set; }
        public bool Status { get; set; }
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