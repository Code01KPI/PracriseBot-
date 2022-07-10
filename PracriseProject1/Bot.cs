using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net;
using System.Text;
//using Newtonsoft.Json;

namespace PracriseProject1
{
    public class Bot
    {
        /// <summary>
        /// Токен.
        /// </summary>
        private string token { get; }

        /// <summary>
        /// Токен api google maps.
        /// </summary>
        private string mapsApiToken { get; }

        /// <summary>
        /// Токен відміни.
        /// </summary>
        private CancellationTokenSource cts = new CancellationTokenSource();

        private TelegramBotClient client;

        /// <summary>
        /// Назва н. п. яка отримана від користувача.
        /// </summary>
        public string settlementName { get; private set; }

        private Coordinates coordinates { get; set; }

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
            new KeyboardButton[] { "Weather info", "Exit"}
        })
        {
            ResizeKeyboard = true
        };

        public Bot()
        {
            using (StreamReader sr = new StreamReader("tokens.txt"))
            {
                string[] buffer = sr.ReadToEnd().Split('\n');
                token = buffer[0].Trim();
                mapsApiToken = buffer[1].Trim();
            }

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
            else if (message.Text == "/keyboard")
            {
                await botClient.SendTextMessageAsync(chatId: chatId,
                                                     text: "Choose command please:",
                                                     replyMarkup: keyboard,
                                                     cancellationToken: cts.Token);
                Console.WriteLine($"Bot sent keyboard for user in chat: {chatId}");
                return;
            }
            
            if(message.Text == "Exit")// TODO: доробити, переробити
            {
                Console.WriteLine($"Bot has finished its work");
                await botClient.SendTextMessageAsync(chatId: chatId,
                                     text: "Choose command: /keyboard", //TODO: Доробити обробку команди inline
                                     cancellationToken: cts.Token);
                Console.WriteLine($"Bot sent a command list in chat: {chatId}");
                return;
            }
            if (message.Text == "Weather info")
            {
                await botClient.SendTextMessageAsync(chatId: chatId,
                                                     text: "Please enter the name of your settlement:",
                                                     cancellationToken: cts.Token);

                return;
            }
            else
            {
                settlementName = message?.Text;

                try
                {
                    await HandleSettlementCoordinateAsync(settlementName);
                    await botClient.SendTextMessageAsync(chatId: chatId,
                                         text: $"Lat: {coordinates.Lat}; Lng: {coordinates.Lng}",
                                         cancellationToken: cts.Token);
                }
                catch (WebException ex)//TODO: замінити ексепшин.
                {
                    Console.WriteLine(ex.Message);
                    await botClient.SendTextMessageAsync(chatId: chatId,
                                                         text: "Unfortunately, you entered the incorrect name of the settlement(",
                                                         cancellationToken: cts.Token);
                    await botClient.SendStickerAsync(chatId: chatId,
                                                     sticker: "https://cdn.tlgrm.app/stickers/ccd/a8d/ccda8d5d-d492-4393-8bb7-e33f77c24907/192/9.webp",
                                                     cancellationToken: cts.Token);
                    await botClient.SendTextMessageAsync(chatId: chatId,
                                     text: "Please try again:)",
                                     cancellationToken: cts.Token);
                }

                return;
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

        /// <summary>
        /// Визначення широти і довготи н. п..
        /// </summary>
        /// <param name="settlement"></param>
        private async Task<Task> HandleSettlementCoordinateAsync(string settlement)//TODO: Перевірку вхідних даних
        {

            string adress = $"https://maps.googleapis.com/maps/api/geocode/json?address={settlement},&key={mapsApiToken}";
            string source = String.Empty;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(adress);
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    source = await reader.ReadToEndAsync();
                }
            }
            Console.WriteLine(source);
            coordinates = new Coordinates(source);

            return Task<Task>.CompletedTask;
        }
    }
}




