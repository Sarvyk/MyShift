using Microsoft.EntityFrameworkCore;
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
        Task CreateScheduleAsync();
        Task EditShiftScheduleAsync();
        Task DeleteScheduleAsync();
    }
}