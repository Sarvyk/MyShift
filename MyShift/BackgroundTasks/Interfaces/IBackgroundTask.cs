using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.BackgroundTasks.Interfaces
{
    internal interface IBackgroundTask
    {
        Task Start(CancellationToken ct);
    }
}