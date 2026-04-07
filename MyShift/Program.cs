using DotNetEnv;
using MyShift.Core.Data;
using MyShift.Core.Interfaces;
using MyShift.Core.Scenarios;
using MyShift.Core.Scenarios.Interfaces;
using MyShift.Core.Services;
using MyShift.Repositories;
using Telegram.Bot;

namespace MyShift
{
    internal class Program
    {
        private static string _token = "";
        static async Task Main(string[] args)
        {
            Env.Load();
            _token = Env.GetString("API_TOKEN");
            var botClient = new TelegramBotClient(_token);
            SqLiteDbContext dbContext = new SqLiteDbContext();
            UserService userService = new UserService(new UserRepository(dbContext));
            ScheduleRequestService scheduleRequestService = new ScheduleRequestService(new RequestRepository(dbContext), new ScheduleRepository(dbContext));
            var scenarios = new List<IScenario>()
            {
                new Add_Request(userService, scheduleRequestService),
                new Delete_Request(scheduleRequestService),
                new Add_Template(scheduleRequestService)
            };
            var handle = new UpdateHandler(userService, scheduleRequestService, scenarios, new InMemoryScenarioContextRepository());
            botClient.StartReceiving(handle);
            await Task.Delay(-1);
        }
    }
}