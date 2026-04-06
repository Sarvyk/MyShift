using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Core.Dialogs
{
    internal class DialogCreateSchedule : Dialog<CreateScheduleStage>
    {
        private IReadOnlyList<ScheduleTemplate> _templates;
        private IReadOnlyList<ToDoUser> _users;
        private readonly IScheduleBuilder _builder;
        public DialogCreateSchedule(ITelegramBotClient botClient, Update update, IReadOnlyList<ScheduleTemplate> templates, IReadOnlyList<ToDoUser> users, IScheduleBuilder scheduleBuilder, IScheduleRequestService schReqService) : base(botClient, update, schReqService)
        {
            _builder = scheduleBuilder;
            _templates = templates;
            _users = users;
        }
        public override async Task<bool> NextStep(string? message, CancellationToken ct)
        {
            Validate(message);
            switch(_stage)
            {
                case CreateScheduleStage.AssigneeSelection:
                    ToDoUser user = _users[Int32.Parse(message) - 1];
                    _builder.AddUserSchedule(user);
                    _stage++;
                    await _botClient.SendMessage(_update.Message.Chat, $"Выберите шаблон.", cancellationToken: ct);
                    _templates = await _scheduleRequestService.GetAllTemplates();
                    StringBuilder sb = new StringBuilder();
                    int i = 1;
                    foreach (ScheduleTemplate temp in _templates)
                    {
                        sb.AppendLine($"{i++}) {temp.Name}; начало работы в {temp.StartTime}; окончание работы в {temp.EndTime}; дни недели:{EnumBitConverter.GetFromBitToShortNames(temp.DaysOfWeekBits)}");
                    }
                    await _botClient.SendMessage(_update.Message.Chat, sb.ToString(), cancellationToken: ct);
                    return false;
                case CreateScheduleStage.TemplateSelection:
                    Schedule schedule = _builder.GetSchedule();
                    ScheduleTemplate template = _templates[Int32.Parse(message) - 1];
                    await _scheduleRequestService.InsertScheduleAsync(schedule, template, ct);
                    await _botClient.SendMessage(_update.Message.Chat, $"График успешно составлен!", cancellationToken: ct);
                    break;
            }
            return true;
        }

        protected override void Validate(string? str)
        {
            base.Validate(str);
            switch (_stage)
            {
                case CreateScheduleStage.AssigneeSelection:
                    if (!Int32.TryParse(str, out int result2))
                        throw new FormatException("Ответ должен содержать только цифры!");
                    if (Int32.Parse(str)-1 >= _users.Count || Int32.Parse(str)-1 < 0)
                        throw new IndexOutOfRangeException("Такого варианта нет в списке");
                    break;
                case CreateScheduleStage.TemplateSelection:
                    if (!Int32.TryParse(str, out int result))
                        throw new FormatException("Ответ должен содержать только цифры!");
                    if (Int32.Parse(str) - 1 >= _templates.Count || Int32.Parse(str) - 1 < 0)
                        throw new IndexOutOfRangeException("Такого варианта нет в списке");
                    break;
            }
        }
    }
    internal enum CreateScheduleStage
    {
        AssigneeSelection,
        TemplateSelection
    }
}