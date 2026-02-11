using MyShift.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Abstracts
{
    internal abstract class Dialog
    {
        protected readonly ITelegramBotClient _botClient;
        protected readonly Update _update;
        public Dialog(ITelegramBotClient botClient, Update update)
        {
            _botClient = botClient;
            _update = update;
        }
        public abstract Task<bool> NextStep(string? message, CancellationToken ct);
        protected virtual void Validate(string? str)
        {//временная реализация т.к. пока не решил где сделать этот метод. В сервисах он тоже есть и это очевидно дубль, который не нужен.
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Строка не должна быть пустой");
        }
    }
}