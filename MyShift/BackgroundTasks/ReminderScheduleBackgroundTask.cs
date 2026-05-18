using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.BackgroundTasks
{
    internal class ReminderScheduleBackgroundTask : BackgroundTask
    {
        public ReminderScheduleBackgroundTask(TimeSpan delay) : base(delay, nameof(ReminderScheduleBackgroundTask))
        {
        }

        protected override Task Execute(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
