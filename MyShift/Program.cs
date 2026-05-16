using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MyShift.BackgroundTasks;
using MyShift.Core.Data;
using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Scenarios;
using MyShift.Core.Scenarios.Interfaces;
using MyShift.Core.Services;
using MyShift.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift
{
    internal class Program
    {
        private static string _token = "";
        static async Task Main(string[] args)
        {
            Env.Load();
            _token = Env.GetString("API_TOKEN");
            using (var db = new SqLiteDbContext())
            {
                db.Database.Migrate();
                var botClient = new TelegramBotClient(_token);
                var userRepository = new UserRepository(db);
                UserService userService = new UserService(userRepository);
                var requestRepository = new RequestRepository(db);
                var scheduleRepository = new ScheduleRepository(db);
                var notificationRepository = new NotificationRepository(db);
                ScheduleRequestService scheduleRequestService = new ScheduleRequestService(requestRepository, scheduleRepository);
                NotificationService notificationService = new NotificationService(notificationRepository);
                var scenarioContextRepository = new InMemoryScenarioContextRepository();
                var scenarios = new List<IScenario>()
                {
                    new Add_Request(userService, scheduleRequestService),
                    new Delete_Request(scheduleRequestService),
                    new Add_Template(userService, scheduleRequestService),
                    new Add_Schedule(userService, scheduleRequestService),
                    new Edit_Schedule(userService, scheduleRequestService),
                    new EditRole(userService, scheduleRequestService)
                };
                var cts = new CancellationTokenSource();
                var backgroundRunner = new BackgroundTaskRunner();
                backgroundRunner.AddTask(new ResetScenarioBackgroundTask(TimeSpan.FromMinutes(5), scenarioContextRepository, botClient));
                backgroundRunner.AddTask(new TodayBackgroundTask(TimeSpan.FromMinutes(1), notificationService, requestRepository));
                backgroundRunner.AddTask(new RequestsDeliveryBackgroundTask(TimeSpan.FromMinutes(1),botClient, userRepository, notificationService));
                backgroundRunner.StartTasks(cts.Token);
                var handle = new UpdateHandler(userService, scheduleRequestService, scenarios, scenarioContextRepository);
                botClient.StartReceiving(handle, cancellationToken:cts.Token);
                var botInfo = await botClient.GetMe();
                Console.WriteLine($"-------------Бот \"{botInfo.FirstName}\" работает.-------------");
                await KeyCheck(botInfo, backgroundRunner, cts);
                await Task.Delay(-1);
            }
        }
        private static async Task KeyCheck(User bot, BackgroundTaskRunner backgroundRunner, CancellationTokenSource cts)
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.A)
                {
                    await backgroundRunner.StopTasks(cts.Token);
                    Console.WriteLine("Асинхронные операции отменены.");
                    cts.Cancel();
                    break;
                }
                else
                {
                    Console.WriteLine($@"------------Информация о боте------------
Никнейм:{bot.Username}
{bot.FirstName}
{bot.LastName}");
                }
            }
        }
    }
}