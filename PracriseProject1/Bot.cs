using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PracriseProject1
{
    internal class Bot
    {
        /// <summary>
        /// Токен.
        /// </summary>
        private string token { get; set; } = "5567347333:AAEGUpFG-H7gpiFwZlUGfBF-IaKlyRDGcpc";

        /// <summary>
        /// Токен відміни.
        /// </summary>
        private CancellationTokenSource cts = new CancellationTokenSource();

        private TelegramBotClient client;

        /// <summary>
        /// Налаштування отримання оновлень.
        /// </summary>
        private ReceiverOptions receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        /// <summary>
        /// Клавіатура бота.
        /// </summary>
        private ReplyKeyboardMarkup keyboard = new(new []
        {
            new KeyboardButton[] { "Exchange rate", "Exit"}
        })
        {
            ResizeKeyboard = true
        };

        public Bot()
        {
            client = new TelegramBotClient(token);
            Console.WriteLine("Bot has been created");
        }

        /// <summary>
        /// Початок роботи бота.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            client.StartReceiving(updateHandler: HandleUpdatesAsync,
                                  pollingErrorHandler: HandlePollingErrorAsync,
                                  receiverOptions: receiverOptions,
                                  cancellationToken: cts.Token);

            var me = await client.GetMeAsync();
            Console.WriteLine($"Bot {me.Username} started work");

            Console.ReadLine();
            cts.Cancel();
        }

        

        /// <summary>
        /// Обробка оновлень.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="update"></param>
        /// <param name="cancelletionToken"></param>
        /// <returns></returns>
        private async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancelletionToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text is not null)
            {
                await HandleMessageAsync(botClient, update.Message);
                return;
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQueryAsync(botClient, update.CallbackQuery);
                return;
            }               
        }

        /// <summary>
        /// Обробка повідомлень.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(chatId: chatId,
                                                     text: "Choose command: /keyboard", //TODO: Доробити обробку команди inline
                                                     cancellationToken: cts.Token);
                Console.WriteLine($"Bot sent a command list in chat: {chatId}");
                return;
            }

            if (message.Text == "/keyboard")
            {
                await botClient.SendTextMessageAsync(chatId: chatId,
                                                     text: "Choose command please:",
                                                     replyMarkup: keyboard,
                                                     cancellationToken: cts.Token);
                Console.WriteLine($"Bot sent keyboard for user in chat: {chatId}");
                return;
            }

            if(message.Text == "Exit")
            {
                Console.WriteLine($"Bot has finished its work");
                cts.Cancel();
                return;
            }
            else if (message.Text == "Exchange rate")
            {
                RateInfo rateInfoGRN = new RateInfo();
                await botClient.SendTextMessageAsync(chatId: chatId,
                                                     text: $"Exchange rate of the hryvnia against the dollar: {rateInfoGRN.dollarRate}",
                                                     cancellationToken: cts.Token);
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {

        }

        /// <summary>
        /// Обробка помилок.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
