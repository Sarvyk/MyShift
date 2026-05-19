using MyShift.Core.Enums;
using MyShift.Core.Extensions;
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
            Role roleCurrentUser = (Role)Int32.Parse(context.Data["userRole"].ToString());
            ToDoUser user = null;
            UserSchedule schedule = null;
            string requestResult = "Ваша заявка выполнена. ";
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
                    await botClient.SendMessage(message.Chat, "Процесс редактировани графика", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                    await botClient.SendMessage(message.Chat, "Выберите пользователя.📋", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showUserPageNext||0")), cancellationToken: ct);
                    context.CurrentStep = "selectUser";
                    return ScenarioResult.Transition;
                case "selectUser":
                    callbackData = new List<KeyValuePair<string, string>>();
                    if (context.Data["Callback"].ToString().StartsWith("showUserPageNext|"))
                    {
                        schedules = await _scheduleRequestService.GetActiveSchedulesAsync(ct);
                        foreach (UserSchedule userSchedule in schedules)
                        {
                            callbackData.Add(new KeyValuePair<string, string>($"{userSchedule.User.Id}){userSchedule.User.FirstName} {userSchedule.User.LastName}", ToDoItemCallbackDto.FromString($"showUser|{userSchedule.User.Id}").ToString()));
                        }
                        // Если мы получаем в колбеке данные о кнопках смены страниц, то попадаем сюда и меняем страницу, а дальше сохраняем шаг тем же.
                        await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите пользователя.👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    else if (context.Data["Callback"].ToString().StartsWith("showUser|"))
                    {
                        UserSchedule selectedUserSchedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId, ct);
                        context.Data.Add("scheduleId", selectedUserSchedule.Id);
                        context.Data.Add("userId", selectedUserSchedule.UserId);
                        await botClient.EditMessageText(message.Chat, message.MessageId, "Что вы хотите сделать с графиком данного пользователя❓",
                            replyMarkup: new InlineKeyboardMarkup()
                            .AddNewRow(new InlineKeyboardButton[]
                            {
                            new InlineKeyboardButton("Изменить время смены✏️","editShift"),
                            })
                            .AddNewRow(new InlineKeyboardButton[]
                            {
                            new InlineKeyboardButton("Отменить график🗑️", "cancelSchedule"),
                            new InlineKeyboardButton("Отменить смену🗑️", "cancelShift")
                            })
                            .AddNewRow(new InlineKeyboardButton[]
                            {
                            new InlineKeyboardButton("Отменить редактирование↩️","cancelEdit")
                            }), cancellationToken: ct);
                        context.CurrentStep = "scheduleActions";
                    }
                    return ScenarioResult.Transition;
                case "scheduleActions":
                    if (context.Data["Callback"].ToString() == "cancelEdit")
                    {
                        await botClient.SendMessage(message.Chat, "Редактирование отменено.", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                        if (context.Data.ContainsKey("TakeRequest"))
                        {
                            await botClient.SendMessage(message.Chat, "Введите причину отказа для пользователя", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                            context.CurrentStep = "EnterMessage";
                            return ScenarioResult.Transition;
                        }
                        break;
                    }
                    else if (context.Data["Callback"].ToString() == "cancelSchedule")
                    {
                        await botClient.SendMessage(message.Chat, "Вы уверены, что хотите отменить выбранный график❓", replyMarkup: new InlineKeyboardMarkup().AddNewRow(new InlineKeyboardButton[] { new InlineKeyboardButton("Да✅", "yes"), new InlineKeyboardButton("Нет❌", "no") }), cancellationToken: ct);
                        context.CurrentStep = "deleteUserSchedule";
                        return ScenarioResult.Transition;
                    }
                    else if (context.Data["Callback"].ToString() == "cancelShift")
                    {
                        callbackData.Clear();
                        user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
                        schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
                        foreach (Shift shift in schedule.Shifts)
                        {
                            if (shift.ShiftType == ShiftType.off)
                                continue;
                            callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShiftCancel|{shift.Id}").ToString()));
                        }
                        await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите смену📋 для удаления.🗑️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showShiftCancelPageNext||0")), cancellationToken: ct);
                        context.CurrentStep = "selectShift";
                    }
                    else if(context.Data["Callback"].ToString() == "editShift")
                    {
                        callbackData.Clear();
                        user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
                        schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
                        foreach (Shift shift in schedule.Shifts)
                        {
                            if (shift.ShiftType == ShiftType.off)
                                continue;
                            callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShiftEdit|{shift.Id}").ToString()));
                        }
                        await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите смену📋 для редактирования.✏️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showShiftEditPageNext||0")), cancellationToken: ct);
                        context.CurrentStep = "selectShift";
                    }
                    return ScenarioResult.Transition;
                case "selectShift":
                    callbackData.Clear();
                    user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
                    schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
                    if (context.Data["Callback"].ToString().StartsWith("showShiftCancelPageNext"))
                    {
                        // Если мы получаем в колбеке данные о кнопках смены страниц, то попадаем в эти условия и меняем страницу, а дальше сохраняем шаг тем же.
                        foreach (Shift shift in schedule.Shifts)
                        {
                            if (shift.ShiftType == ShiftType.off)
                                continue;
                            callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShiftCancel|{shift.Id}").ToString()));
                        }
                        await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите смену📋 для удаления.🗑️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
                    }
                    else if (context.Data["Callback"].ToString().StartsWith("showShiftEditPageNext"))
                    {
                        foreach (Shift shift in schedule.Shifts)
                        {
                            callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShiftEdit|{shift.Id}").ToString()));
                        }
                        await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите смену📋 для редактирования.✏️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
                    }
                    else if (context.Data["Callback"].ToString().StartsWith("showShiftCancel"))
                    {
                        await botClient.SendMessage(message.Chat, "Вы уверены, что хотите отменить выбранную смену?", replyMarkup: new InlineKeyboardMarkup().AddNewRow(new InlineKeyboardButton[] { new InlineKeyboardButton("Да✅", "yes"), new InlineKeyboardButton("Нет❌", "no") }), cancellationToken: ct);
                        context.Data.Add("shiftId", ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId);
                        context.CurrentStep = "deleteShift";
                    }
                    else if(context.Data["Callback"].ToString().StartsWith("showShiftEdit"))
                    {
                        Shift shift = await _scheduleRequestService.GetShiftByIdAsync(ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId, ct);
                        await botClient.SendMessage(message.Chat, $"{shift.ShiftType.GetDisplayName()} {shift.ShiftDate.ToShortDateString()}\r\nВремя работы с {shift.StartTime.Value.ToString("hh\\:mm")} по {shift.EndTime.Value.ToString("hh\\:mm")}\r\nВведите новое время начала в формате \"05:25\"", cancellationToken: ct);
                        context.Data.Add("shiftId", ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId);
                        context.CurrentStep = "editStartTimeShift";
                    }
                    return ScenarioResult.Transition;
                case "editStartTimeShift":
                    Validator.TextIsValidate(1, message.Text);
                    await botClient.SendMessage(message.Chat, $"Введите новое время окончания в формате \"05:25\"", cancellationToken: ct);
                    context.Data.Add("newStartTime",message.Text);
                    context.CurrentStep = "editEndTimeShift";
                    return ScenarioResult.Transition;
                case "editEndTimeShift":
                    Validator.TextIsValidate(1, message.Text);
                    int shiftId = Int32.Parse(context.Data["shiftId"].ToString());
                    TimeSpan start = TimeSpan.Parse(context.Data["newStartTime"].ToString());
                    TimeSpan end = TimeSpan.Parse(message.Text);
                    Shift editedShiftTime = await _scheduleRequestService.EditShiftScheduleAsync(shiftId, start, end, ct);
                    await botClient.SendMessage(message.Chat, $"Время смены пользователя успешно изменено!", cancellationToken: ct);
                    await botClient.SendMessage(editedShiftTime.UserSchedule.User.TelegramId,$"{(context.Data.ContainsKey("TakeRequest") ? requestResult : "")}Время смены на \"{editedShiftTime.ShiftDate.ToShortDateString()}\" изменено на \"{editedShiftTime.StartTime.Value.ToString("hh\\:mm")}-{editedShiftTime.EndTime.Value.ToString("hh\\:mm")}\"",cancellationToken: ct);
                    if (context.Data.ContainsKey("TakeRequest"))
                    {
                        await _scheduleRequestService.ApproveRequestAsync(Int32.Parse(context.Data["TakeRequest"].ToString()), (await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct)).Id, $"Время смены #{editedShiftTime.Id} изменено на \"{editedShiftTime.StartTime.Value.ToString("hh\\:mm")}-{editedShiftTime.EndTime.Value.ToString("hh\\:mm")}\"", ct);
                    }
                    break;
                case "deleteShift":
                    if (context.Data["Callback"].ToString() == "no")
                    {
                        await botClient.SendMessage(message.Chat, "Удаление отменено↩️", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                        if (context.Data.ContainsKey("TakeRequest"))
                        {
                            await botClient.SendMessage(message.Chat, "Введите причину отказа для пользователя", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                            context.CurrentStep = "EnterMessage";
                            return ScenarioResult.Transition;
                        }
                        break;
                    }
                    else if (context.Data["Callback"].ToString() == "yes")
                    {
                        Shift deletingShift = await _scheduleRequestService.GetShiftByIdAsync(Int32.Parse(context.Data["shiftId"].ToString()), ct);
                        await _scheduleRequestService.DeleteShiftByShiftIdAsync(deletingShift.Id, ct);
                        await botClient.SendMessage(message.Chat, "Выбранная смена удалёна✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                        await botClient.SendMessage(deletingShift.UserSchedule.User.TelegramId, $"{(context.Data.ContainsKey("TakeRequest") ? requestResult : "")}Ваша смена на \"{deletingShift.ShiftDate.ToShortDateString()}\" отменена!", cancellationToken: ct);
                        if (context.Data.ContainsKey("TakeRequest"))
                        {
                            await _scheduleRequestService.ApproveRequestAsync(Int32.Parse(context.Data["TakeRequest"].ToString()), (await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct)).Id, $"Смена #{deletingShift.Id} на {deletingShift.ShiftDate.ToShortDateString()} была отменена", ct);
                        }
                        break;
                    }
                    return ScenarioResult.Transition;
                case "deleteUserSchedule":
                    if (context.Data["Callback"].ToString() == "no")
                    {
                        await botClient.SendMessage(message.Chat, "Удаление отменено↩️", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                        if (context.Data.ContainsKey("TakeRequest"))
                        {
                            await botClient.SendMessage(message.Chat, "Введите причину отказа для пользователя", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                            context.CurrentStep = "EnterMessage";
                            return ScenarioResult.Transition;
                        }
                        break;
                    }
                    else if (context.Data["Callback"].ToString() == "yes")
                    {
                        UserSchedule deletedUserSchedule = await _scheduleRequestService.DeleteScheduleByScheduleIdAsync(Int32.Parse(context.Data["scheduleId"].ToString()), ct);
                        await botClient.SendMessage(message.Chat, "Выбранный график удалён✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                        await botClient.SendMessage(deletedUserSchedule.User.TelegramId, $"{(context.Data.ContainsKey("TakeRequest") ? requestResult : "")}Ваш график был удалён",cancellationToken: ct);
                        if (context.Data.ContainsKey("TakeRequest"))
                        {
                            await _scheduleRequestService.ApproveRequestAsync(Int32.Parse(context.Data["TakeRequest"].ToString()), (await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct)).Id, $"График #{deletedUserSchedule.Id} был отменен", ct);
                        }
                        break;
                    }
                    return ScenarioResult.Transition;
                case "EnterMessage":
                    int requestId = Int32.Parse(context.Data["TakeRequest"].ToString());
                    user = await _userService.GetUserByTelegramIdAsync(message.From.Id, ct);
                    Request request = await _scheduleRequestService.RejectRequestAsync(requestId, user.Id, message.Text, ct);
                    await botClient.SendMessage(message.From.Id, "Заявка отклонена", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(user.Role), cancellationToken: ct);
                    await botClient.SendMessage(request.Creator.TelegramId, $"Ваша заявка отклонена. Причина:\r\n---{message.Text}---", cancellationToken: ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}