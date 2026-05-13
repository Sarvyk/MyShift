using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using MyShift.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MyShift.Core.Scenarios
{
    internal class Edit_Schedule : IScenario
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        public Edit_Schedule(IUserService userService, IScheduleRequestService scheduleRequestService)
        {
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Edit_Schedule;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            var callbackData = new List<KeyValuePair<string,string>>();
            switch (context.CurrentStep)
            {
                case null:
                    IReadOnlyList<UserSchedule> schedules = await _scheduleRequestService.GetActiveSchedulesAsync(ct);
                    if (schedules.Count == 0)
                    {
                        await botClient.SendMessage(message.Chat, "Пользователи с действующими графиками не обнаружены🔍❌", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    foreach (UserSchedule userSchedule in schedules)
                    {
                        callbackData.Add(new KeyValuePair<string, string>($"{userSchedule.User.Id}){userSchedule.User.FirstName} {userSchedule.User.LastName}", ToDoItemCallbackDto.FromString($"showUser|{userSchedule.User.Id}").ToString()));
                    }
                    await botClient.SendMessage(message.Chat, "Выберите пользователя.📋", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showUserPageNext||0")), cancellationToken: ct);
                    context.CurrentStep = "selectUser";
                    return ScenarioResult.Transition;
                case "selectUser":
                    callbackData = new List<KeyValuePair<string, string>>();
                    if (!context.Data["Callback"].ToString().StartsWith("showUser|"))
                    {
                        schedules = await _scheduleRequestService.GetActiveSchedulesAsync(ct);
                        foreach (UserSchedule userSchedule in schedules)
                        {
                            callbackData.Add(new KeyValuePair<string, string>($"{userSchedule.User.Id}){userSchedule.User.FirstName} {userSchedule.User.LastName}", ToDoItemCallbackDto.FromString($"showUser|{userSchedule.User.Id}").ToString()));
                        }
                    }
                    Message callbackMessage = (Message)context.Data["CallbackMessage"];
                    if (context.Data["Callback"].ToString().StartsWith("showUserPageNext"))
                    {
                        // Если мы получаем в колбеке данные о кнопках смены страниц, то попадаем сюда и меняем страницу, а дальше сохраняем шаг тем же.
                        await botClient.EditMessageText(callbackMessage.Chat, callbackMessage.MessageId, "Выберите пользователя.👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    UserSchedule selectedUserSchedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId,ct);
                    context.Data.Add("scheduleId", selectedUserSchedule.Id);
                    context.Data.Add("userId", selectedUserSchedule.UserId);
                    await botClient.EditMessageText(callbackMessage.Chat, callbackMessage.MessageId, "Что вы хотите сделать с графиком данного пользователя❓", replyMarkup: new InlineKeyboardMarkup().AddNewRow(new InlineKeyboardButton[]{new InlineKeyboardButton("Отменить график❌", "cancelSchedule"), new InlineKeyboardButton("Отменить смену❌", "cancelShift")}), cancellationToken: ct);
                    context.CurrentStep = "scheduleActions";
                    return ScenarioResult.Transition;
                case "scheduleActions":
                    callbackMessage = (Message)context.Data["CallbackMessage"];
                    if (context.Data["Callback"].ToString() == "cancelSchedule")
                    {
                        await botClient.SendMessage(message.Chat, "Вы уверены, что хотите отменить выбранный график❓", replyMarkup: new InlineKeyboardMarkup().AddNewRow(new InlineKeyboardButton[] { new InlineKeyboardButton("Да✅", "yes"), new InlineKeyboardButton("Нет❌", "no") }), cancellationToken: ct);
                        context.CurrentStep = "deleteUserSchedule";
                        return ScenarioResult.Transition;
                    }
                    callbackData.Clear();
                    ToDoUser user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
                    UserSchedule schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
                    foreach (Shift shift in schedule.Shifts)
                    {
                        callbackData.Add(new KeyValuePair<string,string>(shift.ShiftDate.ToString("d"),ToDoItemCallbackDto.FromString($"showShift|{shift.Id}").ToString()));
                    }
                    await botClient.EditMessageText(callbackMessage.Chat, callbackMessage.MessageId, "Выберите смену📋 для удаления.🗑️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showShiftPageNext||0")), cancellationToken: ct);
                    context.CurrentStep = "selectShift";
                    return ScenarioResult.Transition;
                case "selectShift":
                    callbackData.Clear();
                    if (!context.Data["Callback"].ToString().StartsWith("showShift|"))
                    {
                        user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
                        schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
                        foreach (Shift shift in schedule.Shifts)
                        {
                            callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShift|{shift.Id}").ToString()));
                        }
                    }
                    callbackMessage = (Message)context.Data["CallbackMessage"];
                    if (context.Data["Callback"].ToString().StartsWith("showShiftPageNext"))
                    {
                        // Если мы получаем в колбеке данные о кнопках смены страниц, то попадаем сюда и меняем страницу, а дальше сохраняем шаг тем же.
                        await botClient.EditMessageText(callbackMessage.Chat, callbackMessage.MessageId, "Выберите смену📋 для удаления.🗑️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    await botClient.SendMessage(message.Chat, "Вы уверены, что хотите отменить выбранную смену?", replyMarkup: new InlineKeyboardMarkup().AddNewRow(new InlineKeyboardButton[] { new InlineKeyboardButton("Да✅", "yes"), new InlineKeyboardButton("Нет❌", "no") }), cancellationToken: ct);
                    context.Data.Add("shiftId", ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId);
                    context.CurrentStep = "deleteShift";
                    return ScenarioResult.Transition;
                case "deleteShift":
                    if (context.Data["Callback"].ToString() == "no")
                    {
                        await botClient.SendMessage(message.Chat, "Удаление отменено↩️", cancellationToken: ct);
                        break;
                    }
                    await _scheduleRequestService.DeleteShiftByShiftIdAsync(Int32.Parse(context.Data["shiftId"].ToString()), ct);
                    await botClient.SendMessage(message.Chat, "Выбранная смена удалёна✅", cancellationToken: ct);
                    break;
                case "deleteUserSchedule":
                    if (context.Data["Callback"].ToString() == "no")
                    {
                        await botClient.SendMessage(message.Chat, "Удаление отменено↩️", cancellationToken: ct);
                        break;
                    }
                    await _scheduleRequestService.DeleteScheduleByScheduleIdAsync(Int32.Parse(context.Data["scheduleId"].ToString()), ct);
                    await botClient.SendMessage(message.Chat, "Выбранный график удалён✅", cancellationToken: ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}