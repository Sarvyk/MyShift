using Telegram.Bot;

namespace MyShift
{
    internal class Program
    {
        private static string _token = "";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в бот планировщик. Ниже описаны возможные команды.");
            ITelegramBotClient botClient = new TelegramBotClient(_token);
            //var me = await botClient.GetMe();
            var Handle = new UpdateHandler();
            botClient.StartReceiving(Handle);
            await Task.Delay(-1);
        }
    }
}