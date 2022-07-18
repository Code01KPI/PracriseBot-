using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net;

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
        /// Токен api OpenWeather.
        /// </summary>
        private string weatherApiToken { get; }

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

        private WeatherData weatherData { get; set; }

        /// <summary>
        /// Флажок для уточнення назви н. п..
        /// </summary>
        private static bool settlementFlag = false;

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
            new KeyboardButton[] { "Weather info" }
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
                weatherApiToken = buffer[2].Trim();
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
            Console.WriteLine($"Bot {me.FirstName} started work");

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
                                                     text: "This bot can give you the weather in your city for the day. Just follow the instructions!", 
                                                     replyMarkup: keyboard,
                                                     cancellationToken: cts.Token);
                Console.WriteLine($"Bot sent a command list in chat: {chatId}");
                return;
            }

            if (message.Text == "Weather info")
            {
                Console.WriteLine("User requested weather data.");
                await botClient.SendTextMessageAsync(chatId: chatId,
                                                     text: "Please enter the name of your settlement:",
                                                     cancellationToken: cts.Token);
                settlementFlag = true;
                return;
            }
            else if (settlementFlag == true && message.Text is not null /*&& string.IsNullOrWhiteSpace(message.Text)*/) // TODO: Пофіксити
            {
                settlementName = message.Text;

                try
                {
                    await HandleSettlementCoordinateAsync(settlementName);
                    if (settlementFlag)
                        await botClient.SendTextMessageAsync(chatId: chatId,
                                                             text: $"<b>Do you mean this settlement? -  {coordinates.fullAdress}</b>\nIf not, please clarify your request (add the name of the region/country, etc.)",
                                                             parseMode: ParseMode.Html,
                                                             replyToMessageId: message.MessageId,
                                                             replyMarkup: new InlineKeyboardMarkup(
                                                             new[]
                                                             {
                                                                 InlineKeyboardButton.WithCallbackData("Yes", "1"),
                                                             }),
                                                             cancellationToken: cts.Token); 
                    if (coordinates.coordinatesFlag)
                        throw new Exception("There aren't settlement with thats title");

                }
                catch (Exception ex)
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
            else
            {
                Console.WriteLine($"User in char {chatId} sent invalid command");
                await botClient.SendTextMessageAsync(chatId: chatId,
                                                     text: "Unfortunately, you entered the invalid command(",
                                                     cancellationToken: cts.Token);
                return;
            }
        }

        /// <summary>
        /// Обробник клавіатури inline.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            try
            {
                if (callbackQuery?.Data == "1")
                {
                    await HandleWeatherDataAsync(botClient, callbackQuery?.Message?.Chat.Id);
                    await botClient.SendTextMessageAsync(chatId: callbackQuery?.Message?.Chat.Id,
                                     text: "<b>Select a time period:</b>",
                                     parseMode: ParseMode.Html,
                                     replyMarkup: new InlineKeyboardMarkup(
                                     new[]
                                     {
                                            InlineKeyboardButton.WithCallbackData("24 hours", "24"),
                                            InlineKeyboardButton.WithCallbackData("2 days", "48"),
                                            InlineKeyboardButton.WithCallbackData("5 days", "120")
                                     }),
                                     cancellationToken: cts.Token);
                    settlementFlag = false;
                }
                else if (callbackQuery?.Data == "24")
                {
                    await PrintWeatherDataAsync(botClient, callbackQuery?.Message?.Chat.Id, 8);
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return;
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
        private async Task HandleSettlementCoordinateAsync(string settlement)
        {
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={settlement},&key={mapsApiToken}";
            string coordinatesDataJson = await ReadJSONResponse(url);
            coordinates = new Coordinates(coordinatesDataJson);

            return;
        }

        /// <summary>
        /// Отримання даних про погоду.
        /// </summary>
        /// <returns></returns>
        private async Task HandleWeatherDataAsync(ITelegramBotClient botClient, long? chatId)
        {
            string url = $"https://api.openweathermap.org/data/2.5/forecast?lat={coordinates.Lat}&lon={coordinates.Lng}&units=metric&lang=ua&appid={weatherApiToken}";
            string weatherDataJson = String.Empty;

            try
            {
                weatherDataJson = await ReadJSONResponse(url);
                try
                {
                    weatherData = new WeatherData(weatherDataJson);
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine(ex.Message);
                    await botClient.SendTextMessageAsync(chatId: chatId,
                                                         text: "Sorry, but our weather service is currently down, so we don't have any weather data",
                                                         cancellationToken: cts.Token);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return;
        }

        /// <summary>
        /// Зчитування JSON відповіді
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<string> ReadJSONResponse(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    string dataJSON = await reader.ReadToEndAsync();
                    return dataJSON;
                }
            }
        }

        /// <summary>
        /// Вивід інформації про погоду.
        /// </summary>
        /// <returns></returns>
        private async Task PrintWeatherDataAsync(ITelegramBotClient botClient, long? chatId, int timeCount)
        {
            if(weatherData == null)
            {
                Console.WriteLine("Error - weather data didn't parse");
                await botClient.SendTextMessageAsync(chatId: chatId,
                                     text: "Unfortunately, our service cannot receive a response from the weather API.",
                                     cancellationToken: cts.Token);
            }
            for(int i = 0; i < timeCount; i += 2)
            {
                await botClient.SendTextMessageAsync(chatId: chatId,
                                                     text: $"<b>Time: {weatherData?.data.list[i].dt_txt.Remove(16)}</b>" +
                                                     $"\nTemperature: {weatherData?.data.list[i].main.temp} C Feels like: {weatherData?.data.list[i].main.feels_like} C" +
                                                     $"\nPressure: {weatherData?.data.list[i].main.pressure}" +
                                                     $"\nHumidity: {weatherData?.data.list[i].main.humidity}" +
                                                     $"\nWeather: {weatherData?.data.list[i].weather[0].main}" +
                                                     $"\nClouds: {weatherData?.data.list[i].clouds.all} %" +
                                                     $"\nWind speed: {weatherData?.data.list[i].wind.speed} Wind gust: {weatherData?.data.list[i].wind.gust}",
                                                     parseMode: ParseMode.Html,
                                                     cancellationToken: cts.Token);
            }
            Console.WriteLine($"Received weather data in chat: {chatId}");
        }
    }
}


