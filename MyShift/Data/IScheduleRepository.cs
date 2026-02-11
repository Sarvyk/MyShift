using Microsoft.EntityFrameworkCore;
using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MyShift.Data
{
    public interface IScheduleRepository
    {
        Task CreateScheduleAsync(CancellationToken ct);
        Task CreateScheduleTemplateAsync(Schedule_Template schTemplate, CancellationToken ct);
        Task EditShiftScheduleAsync( CancellationToken ct);
        Task DeleteScheduleAsync( CancellationToken ct);
    }
}