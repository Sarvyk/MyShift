using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
using System.Runtime.CompilerServices;
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
            switch(context.CurrentStep)
            {
                case null:
                    return await ShowUsers(botClient, context, message, ct);
                case "NextPage":
                    return await NextPageWithSelectUser(botClient, context, message, ct);
                case "scheduleActions":
                    return await scheduleActions(botClient, context, message, ct);
                case "selectShift":
                    return await ShiftActions(botClient, context, message, ct);
                case "editStartTimeShift":
                    return await EditStartTimeShift(botClient, context, message, ct);
                case "editEndTimeShift":
                    return await EditEndTimeShift(botClient, context, message, ct);
                case "deleteShift":
                    return await DeleteShift(botClient, context, message, ct);
                case "deleteUserSchedule":
                    return await DeleteUserSchedule(botClient, context, message, ct);
                case "EnterMessage":
                    return await EnterMessage(botClient, context, message, ct);
            }
            return ScenarioResult.Completed;
        }
        /// <summary>
        /// Отправляет ответное сообщение на заявку
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> EnterMessage(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
            int requestId = Int32.Parse(context.Data["TakeRequest"].ToString());
            ToDoUser currentUser = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
            Request request = await _scheduleRequestService.RejectRequestAsync(requestId, currentUser.Id, message.Text, ct);
            await botClient.SendMessage(message.Chat.Id, "Заявка отклонена", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
            await botClient.SendMessage(request.Creator.TelegramId, $"Ваша заявка отклонена. Причина:\r\n---{message.Text}---", cancellationToken: ct);
            return ScenarioResult.Completed;
        }
        /// <summary>
        /// Удаление графика пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> DeleteUserSchedule(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
            ToDoUser currentUser = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
            if (context.Data["Callback"].ToString() == "no")
            {
                await botClient.SendMessage(message.Chat, "Удаление отменено↩️", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                if (context.Data.ContainsKey("TakeRequest"))
                {
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Введите причину отказа для пользователя", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                    context.CurrentStep = "EnterMessage";
                    return ScenarioResult.Transition;
                }
                return ScenarioResult.Completed;
            }
            else if (context.Data["Callback"].ToString() == "yes")
            {
                UserSchedule deletedUserSchedule = await _scheduleRequestService.DeleteScheduleByScheduleIdAsync(Int32.Parse(context.Data["scheduleId"].ToString()), ct);
                await botClient.SendMessage(message.Chat, "Выбранный график удалён✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                await botClient.SendMessage(deletedUserSchedule.User.TelegramId, $"{StartMessageRequest(context.Data.ContainsKey("TakeRequest"))}Ваш график был удалён", cancellationToken: ct);
                if (context.Data.ContainsKey("TakeRequest"))
                {
                    await _scheduleRequestService.ApproveRequestAsync(Int32.Parse(context.Data["TakeRequest"].ToString()), (await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct)).Id, $"График #{deletedUserSchedule.Id} был отменен", ct);
                }
                return ScenarioResult.Completed;
            }
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Удаление смены пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> DeleteShift(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            ToDoUser currentUser = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
            if (context.Data["Callback"].ToString() == "no")
            {
                await botClient.SendMessage(message.Chat, "Удаление отменено↩️", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                if (context.Data.ContainsKey("TakeRequest"))
                {
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Введите причину отказа для пользователя", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                    context.CurrentStep = "EnterMessage";
                    return ScenarioResult.Transition;
                }
                return ScenarioResult.Completed;
            }
            else if (context.Data["Callback"].ToString() == "yes")
            {
                Shift deletingShift = await _scheduleRequestService.GetShiftByIdAsync(Int32.Parse(context.Data["shiftId"].ToString()), ct);
                await _scheduleRequestService.DeleteShiftByShiftIdAsync(deletingShift.Id, ct);
                await botClient.SendMessage(message.Chat, "Выбранная смена удалёна✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                await botClient.SendMessage(deletingShift.UserSchedule.User.TelegramId, $"{StartMessageRequest(context.Data.ContainsKey("TakeRequest"))}Ваша смена на \"{deletingShift.ShiftDate.ToShortDateString()}\" отменена!", cancellationToken: ct);
                if (context.Data.ContainsKey("TakeRequest"))
                {
                    await _scheduleRequestService.ApproveRequestAsync(Int32.Parse(context.Data["TakeRequest"].ToString()), (await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct)).Id, $"Смена #{deletingShift.Id} на {deletingShift.ShiftDate.ToShortDateString()} была отменена", ct);
                }
                return ScenarioResult.Completed;
            }
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Редактирование конечного времени смены пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> EditEndTimeShift(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.TextIsValidate(1, message.Text);
            int shiftId = Int32.Parse(context.Data["shiftId"].ToString());
            TimeSpan start = TimeSpan.Parse(context.Data["newStartTime"].ToString());
            TimeSpan end = TimeSpan.Parse(message.Text);
            Shift editedShiftTime = await _scheduleRequestService.EditShiftScheduleAsync(shiftId, start, end, ct);
            await botClient.SendMessage(message.Chat, $"Время смены пользователя успешно изменено!", cancellationToken: ct);
            await botClient.SendMessage(editedShiftTime.UserSchedule.User.TelegramId, $"{StartMessageRequest(context.Data.ContainsKey("TakeRequest"))}Время смены на \"{editedShiftTime.ShiftDate.ToShortDateString()}\" изменено на \"{editedShiftTime.StartTime.Value.ToString("hh\\:mm")}-{editedShiftTime.EndTime.Value.ToString("hh\\:mm")}\"", cancellationToken: ct);
            if (context.Data.ContainsKey("TakeRequest"))
            {
                await _scheduleRequestService.ApproveRequestAsync(Int32.Parse(context.Data["TakeRequest"].ToString()), (await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct)).Id, $"Время смены #{editedShiftTime.Id} изменено на \"{editedShiftTime.StartTime.Value.ToString("hh\\:mm")}-{editedShiftTime.EndTime.Value.ToString("hh\\:mm")}\"", ct);
            }
            return ScenarioResult.Completed;
        }
        /// <summary>
        /// Редактирование начального времени смены пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> EditStartTimeShift(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
            Validator.TextIsValidate(1, message.Text);
            context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, $"Введите новое время окончания в формате \"05:25\"", cancellationToken: ct);
            context.Data.Add("newStartTime", message.Text);
            context.CurrentStep = "editEndTimeShift";
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Различные действия со сменой
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> ShiftActions(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
            string callback = context.Data["Callback"].ToString();
            if (callback.StartsWith("showShiftCancelPageNext"))
            {
                return await ShowShiftCancelPageNext(botClient, context, message, ct);
            }
            else if (callback.StartsWith("showShiftEditPageNext"))
            {
                return await ShowShiftEditPageNext(botClient, context, message, ct);
            }
            else if (callback.StartsWith("showShiftCancel"))
            {
                return await ShowShiftCancel(botClient, context, message, ct);
            }
            else if (callback.StartsWith("showShiftEdit"))
            {
                return await ShowShiftEdit(botClient, context, message, ct);
            }
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Создаёт страницы с пагинацией смен для выбора под удаление
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> ShowShiftCancelPageNext(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            ToDoUser user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
            UserSchedule schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
            var callbackData = new List<KeyValuePair<string, string>>();
            if (context.Data["Callback"].ToString().StartsWith("showShiftCancelPageNext"))
            {
                // Если мы получаем в колбеке данные о кнопках смены страниц, то попадаем в эти условия и меняем страницу, а дальше сохраняем шаг тем же.
                foreach (Shift shift in schedule.Shifts)
                {
                    if (shift.ShiftType == ShiftType.off)
                        continue;
                    callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShiftCancel|{shift.Id}").ToString()));
                }
                context.Data["currentMessage"] = await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите смену📋 для удаления.🗑️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
            }
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Ввод нового начального времени
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> ShowShiftEdit(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Shift shift = await _scheduleRequestService.GetShiftByIdAsync(ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId, ct);
            context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, $"{shift.ShiftType.GetDisplayName()} {shift.ShiftDate.ToShortDateString()}\r\nВремя работы с {shift.StartTime.Value.ToString("hh\\:mm")} по {shift.EndTime.Value.ToString("hh\\:mm")}\r\nВведите новое время начала в формате \"05:25\"", cancellationToken: ct);
            context.Data.Add("shiftId", ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId);
            context.CurrentStep = "editStartTimeShift";
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Диалог с вопросом на удаление выбранной смены
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> ShowShiftCancel(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Вы уверены, что хотите отменить выбранную смену?", replyMarkup: new InlineKeyboardMarkup().AddNewRow(new InlineKeyboardButton[] { new InlineKeyboardButton("Да✅", "yes"), new InlineKeyboardButton("Нет❌", "no") }), cancellationToken: ct);
            context.Data.Add("shiftId", ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId);
            context.CurrentStep = "deleteShift";
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Создаёт страницы с пагинацией смен под редактирование
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> ShowShiftEditPageNext(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
            ToDoUser user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
            UserSchedule schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
            var callbackData = new List<KeyValuePair<string, string>>();
            foreach (Shift shift in schedule.Shifts)
            {
                callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShiftEdit|{shift.Id}").ToString()));
            }
            context.Data["currentMessage"] = await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите смену📋 для редактирования.✏️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Создаёт первую страницу с пользователями
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> ShowUsers(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            IReadOnlyList<UserSchedule> schedules = await _scheduleRequestService.GetActiveSchedulesAsync(ct);
            if (schedules.Count == 0)
            {
                await botClient.SendMessage(message.Chat, "Пользователи с действующими графиками не обнаружены🔍❌", cancellationToken: ct);
                return ScenarioResult.Completed;
            }
            var callbackData = new List<KeyValuePair<string, string>>();
            foreach (UserSchedule userSchedule in schedules)
            {
                callbackData.Add(new KeyValuePair<string, string>($"{userSchedule.User.Id}){userSchedule.User.FirstName} {userSchedule.User.LastName}", ToDoItemCallbackDto.FromString($"showUser|{userSchedule.User.Id}").ToString()));
            }
            await botClient.SendMessage(message.Chat, "Процесс редактировани графика", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
            context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Выберите пользователя.📋", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showUserPageNext||0")), cancellationToken: ct);
            context.CurrentStep = "NextPage";
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Создаёт страницы с пользователями с возможностью переключения
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> NextPageWithSelectUser(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            if (!context.Data.ContainsKey("currentMessage"))
                context.Data.Add("currentMessage", null);
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
            string callback = context.Data["Callback"].ToString();
            if (callback.StartsWith("showUserPageNext|"))
            {
                var schedules = await _scheduleRequestService.GetActiveSchedulesAsync(ct);
                var callbackData = new List<KeyValuePair<string, string>>();
                foreach (UserSchedule userSchedule in schedules)
                {
                    callbackData.Add(new KeyValuePair<string, string>($"{userSchedule.User.Id}){userSchedule.User.FirstName} {userSchedule.User.LastName}", ToDoItemCallbackDto.FromString($"showUser|{userSchedule.User.Id}").ToString()));
                }
                // Если мы получаем в колбеке данные о кнопках смены страниц, то попадаем сюда и меняем страницу, а дальше сохраняем шаг тем же.
                await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите пользователя.👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(callback)), cancellationToken: ct);
            }
            else if (callback.StartsWith("showUser|"))
            {
                UserSchedule selectedUserSchedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(ToDoItemCallbackDto.FromString(callback).ToDoItemId, ct);
                context.Data.Add("scheduleId", selectedUserSchedule.Id);
                context.Data.Add("userId", selectedUserSchedule.UserId);
                context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Что вы хотите сделать с графиком данного пользователя❓",
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
        }
        /// <summary>
        /// Метод с действиями для графика
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> scheduleActions(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
            string callback = context.Data["Callback"].ToString();
            ToDoUser currentUser = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
            if (callback == "cancelEdit")
            {
                return await CancelEdit(botClient, context, message, currentUser, ct);
            }
            else if (callback == "cancelSchedule")
            {
                return await CancelSchedule(botClient, context, message, currentUser, ct);
            }
            else if (callback == "cancelShift")
            {
                return await CancelShift(botClient, context, message, currentUser, ct);
            }
            else if (callback == "editShift")
            {
                return await EditShift(botClient, context, message, currentUser, ct);
            }
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Отмена редактирования графика
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="currentUser"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> CancelEdit(ITelegramBotClient botClient, ScenarioContext context, Message message, ToDoUser currentUser, CancellationToken ct)
        {
            await botClient.SendMessage(message.Chat, "Редактирование отменено.", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
            if (context.Data.ContainsKey("TakeRequest"))
            {
                context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Введите причину отказа для пользователя", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                context.CurrentStep = "EnterMessage";
                return ScenarioResult.Transition;
            }
            return ScenarioResult.Completed;
        }
        /// <summary>
        /// Отмена активного графика
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="currentUser"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> CancelSchedule(ITelegramBotClient botClient, ScenarioContext context, Message message, ToDoUser currentUser, CancellationToken ct)
        {
            context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Вы уверены, что хотите отменить выбранный график❓", replyMarkup: new InlineKeyboardMarkup().AddNewRow(new InlineKeyboardButton[] { new InlineKeyboardButton("Да✅", "yes"), new InlineKeyboardButton("Нет❌", "no") }), cancellationToken: ct);
            context.CurrentStep = "deleteUserSchedule";
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Создание страниц со сменами для выбора под удаление
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="currentUser"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> CancelShift(ITelegramBotClient botClient, ScenarioContext context, Message message, ToDoUser currentUser, CancellationToken ct)
        {
            ToDoUser user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
            UserSchedule schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
            var callbackData = new List<KeyValuePair<string, string>>();
            foreach (Shift shift in schedule.Shifts)
            {
                if (shift.ShiftType == ShiftType.off)
                    continue;
                callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShiftCancel|{shift.Id}").ToString()));
            }
            context.Data["currentMessage"] = await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите смену📋 для удаления.🗑️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showShiftCancelPageNext||0")), cancellationToken: ct);
            context.CurrentStep = "selectShift";
            return ScenarioResult.Transition;
        }
        /// <summary>
        /// Создание страниц со сменами под редактирование
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="currentUser"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> EditShift(ITelegramBotClient botClient, ScenarioContext context, Message message, ToDoUser currentUser, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
            var callbackData = new List<KeyValuePair<string, string>>();
            ToDoUser user = await _userService.GetUserAsync(Int32.Parse(context.Data["userId"].ToString()), ct);
            UserSchedule schedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct);
            foreach (Shift shift in schedule.Shifts)
            {
                if (shift.ShiftType == ShiftType.off)
                    continue;
                callbackData.Add(new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShiftEdit|{shift.Id}").ToString()));
            }
            context.Data["currentMessage"] = await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите смену📋 для редактирования.✏️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showShiftEditPageNext||0")), cancellationToken: ct);
            context.CurrentStep = "selectShift";
            return ScenarioResult.Transition;
        }
        private string StartMessageRequest(bool takeRequest) => (takeRequest ? "Ваша заявка выполнена. " : "");
    }
}