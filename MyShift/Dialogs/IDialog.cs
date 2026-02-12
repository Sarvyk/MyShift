using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MyShift.Dialogs
{
    internal interface IDialog
    {
        Task<bool> NextStep(string? message, CancellationToken ct);
    }
}
