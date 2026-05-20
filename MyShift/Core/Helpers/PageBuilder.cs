using MyShift.Core.Extensions;
using MyShift.DTO;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift.Core.Helpers
{
    internal static class PageBuilder
    {
        private const int _pageSize = 2;
        /// <summary>
        /// Создать кнопки страницы
        /// </summary>
        /// <param name="callbackData"></param>
        /// <param name="listDto"></param>
        /// <returns></returns>
        public static InlineKeyboardMarkup BuildPagedButtons(IReadOnlyList<KeyValuePair<string, string>> callbackData, PagedListCallbackDto listDto)
        {
            InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup();
            int allCount = callbackData.Count;
            // расчёт количества страниц.
            int totalPage = (int)Math.Round((decimal)callbackData.Count / _pageSize, MidpointRounding.ToPositiveInfinity);
            // берём только те элементы, где страница равна той, которая указана во втором параметре.
            callbackData = callbackData.GetBatchByNumber(_pageSize, listDto.Page).ToList();
            foreach (KeyValuePair<string, string> keyVal in callbackData)
            {
                keyboardMarkup.AddNewRow(new InlineKeyboardButton(keyVal.Key, keyVal.Value));
            }
            if (allCount > _pageSize)
            {
                if (listDto.Page == 0)
                {//настраиваем кнопки перехода по страницам
                    keyboardMarkup.AddNewRow(new InlineKeyboardButton("➡️", PagedListCallbackDto.FromString($"{listDto.Action}|{listDto.ToDoListId}|{listDto.Page + 1}").ToString()));
                }
                else if (listDto.Page > 0 && listDto.Page < totalPage - 1)
                {
                    keyboardMarkup.AddNewRow(new InlineKeyboardButton[]
                    {
                        new InlineKeyboardButton("⬅️",PagedListCallbackDto.FromString($"{listDto.Action}|{listDto.ToDoListId}|{listDto.Page - 1}").ToString()),
                        new InlineKeyboardButton("➡️",PagedListCallbackDto.FromString($"{listDto.Action}|{listDto.ToDoListId}|{listDto.Page + 1}").ToString())
                    });
                }
                else
                {
                    keyboardMarkup.AddNewRow(new InlineKeyboardButton("⬅️", PagedListCallbackDto.FromString($"{listDto.Action}|{listDto.ToDoListId}|{listDto.Page - 1}").ToString()));
                }
            }
            return keyboardMarkup;
        }

    }
}