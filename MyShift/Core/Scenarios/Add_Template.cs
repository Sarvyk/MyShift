using MyShift.Core.Interfaces;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Core.Scenarios
{
    internal class Add_Template : IScenario
    {
        private readonly IScheduleRequestService _scheduleRequestService;
        public Add_Template(IScheduleRequestService scheduleRequestService)
        {
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Add_Template;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    await botClient.SendMessage(message.Chat, "Введите название шаблона", cancellationToken: ct);
                    return ScenarioResult.Transition;
                case "SelectTemplateName":
                    return ScenarioResult.Transition;
                case "SelectTemplateType":
                    return ScenarioResult.Transition;
                case "SelectTemplateDaysOfWeeakBits":
                    return ScenarioResult.Transition;
            }
            return ScenarioResult.Completed;
        }
    }
}
