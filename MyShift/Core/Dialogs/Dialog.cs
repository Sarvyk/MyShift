using MyShift.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Core.Dialogs
{
    internal abstract class Dialog<TStage> : IDialog where TStage : Enum
    {
        protected TStage? _stage;
        protected readonly ITelegramBotClient _botClient;
        protected readonly Update _update;
        protected readonly IScheduleRequestService _scheduleRequestService;
        public Dialog(ITelegramBotClient botClient, Update update, IScheduleRequestService scheduleRequestService)
        {
            _botClient = botClient;
            _update = update;
            _scheduleRequestService = scheduleRequestService;
        }
        public abstract Task<bool> NextStep(string? message, CancellationToken ct);
        protected virtual void Validate(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Строка не должна быть пустой");
        }
    }
}