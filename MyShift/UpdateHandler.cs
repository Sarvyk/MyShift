using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MyShift
{
    internal class UpdateHandler : IUpdateHandler
    {
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            botClient.SendMessage(update.Message.Chat, $"Повторяю:{update.Message.Text}");
            return Task.CompletedTask;
        }
    }
}