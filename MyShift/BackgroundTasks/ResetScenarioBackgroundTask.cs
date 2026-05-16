using MyShift.Core.Helpers;
using MyShift.Core.Scenarios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MyShift.BackgroundTasks
{
    internal class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan _resetScenarioTimeout;
        private readonly IScenarioContextRepository _scenarioRepository;
        private readonly ITelegramBotClient _bot;
        public ResetScenarioBackgroundTask(TimeSpan resetScenarioTimeout, IScenarioContextRepository scenarioRepository, ITelegramBotClient bot) : base(resetScenarioTimeout, nameof(ResetScenarioBackgroundTask))
        {
            _resetScenarioTimeout = resetScenarioTimeout;
            _scenarioRepository = scenarioRepository;
            _bot = bot;
        }
        protected override async Task Execute(CancellationToken ct)
        {
            try
            {
                var contexts = await _scenarioRepository.GetContexts(ct);
                foreach (var context in contexts)
                {
                    if (DateTime.UtcNow - context.Value.CreatedAt > _resetScenarioTimeout)
                    {
                        await _scenarioRepository.ResetContext(context.Key, ct);
                        await _bot.SendMessage(context.Key, $"Ваш сценарий был сброшен из-за неактивности в течении {_resetScenarioTimeout}"/*, replyMarkup: MarkupManager.SetStandartKeyboardButtonList()*/, cancellationToken: ct
                        );
                    }
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                Console.WriteLine("Фоновые операции прекращены");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in {nameof(ResetScenarioBackgroundTask)}: {ex}");
            }
        }
    }
}