using DotNetEnv;
using MyShift.Data;
using MyShift.Enums;
using MyShift.Helpers;
using MyShift.Repositories;
using MyShift.Services;
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
            var handle = new UpdateHandler();
            botClient.StartReceiving(handle);
            await Task.Delay(-1);
        }
    }
}