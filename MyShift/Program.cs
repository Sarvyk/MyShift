using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MyShift.Core.Data;
using MyShift.Core.Helpers;
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
                UserService userService = new UserService(new UserRepository(db));
                ScheduleRequestService scheduleRequestService = new ScheduleRequestService(new RequestRepository(db), new ScheduleRepository(db));
                var scenarios = new List<IScenario>()
                {
                    new Add_Request(userService, scheduleRequestService),
                    new Delete_Request(scheduleRequestService),
                    new Add_Template(userService, scheduleRequestService),
                    new Add_Schedule(userService, scheduleRequestService),
                    new Edit_Schedule(userService, scheduleRequestService),
                    new EditRole(userService, scheduleRequestService)
                };
                var handle = new UpdateHandler(userService, scheduleRequestService, scenarios, new InMemoryScenarioContextRepository());
                botClient.StartReceiving(handle);
                await Task.Delay(-1);
            }
        }
    }
}