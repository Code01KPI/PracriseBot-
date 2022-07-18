using Newtonsoft.Json;

namespace PracriseProject1
{
    /// <summary>
    /// Клас для обробки JSON відповіді від OpenWeather API.
    /// </summary>
    public class WeatherData
    {
        public Data data { get; }

        public WeatherData(string jsonSource)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<Data>(jsonSource);
                if (obj?.list.Count <= 0)
                    throw new ArgumentNullException("There are null responce", nameof(obj));
                data = obj;
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// Список погоди на 5 днів(інтервал 3 години).
    /// </summary>
    public class Data
    {
        public List<Info> list = new List<Info>();
    }

    /// <summary>
    /// Компоненти погоди.
    /// </summary>
    /// <param name="main"></param>
    /// <param name="weather"></param>
    /// <param name="clouds"></param>
    /// <param name="wind"></param>
    /// <param name="visibility"></param>
    /// <param name="dt_txt"></param>
    public record class Info(TempAndPressure main, List<Weather> weather, Clouds clouds, Wind wind, int visibility, string dt_txt);

    /// <summary>
    /// Температура і тиск.
    /// </summary>
    /// <param name="main"></param>
    /// <param name="weather"></param>
    /// <param name="clouds"></param>
    /// <param name="wind"></param>
    /// <param name="visibility"></param>
    /// <param name="dt_txt"></param>
    public record class TempAndPressure(double temp, double feels_like, int pressure, int sea_level, int grnd_level, int humidity);

    /// <summary>
    /// Опади і опис погоди.
    /// </summary>
    /// <param name="main"></param>
    /// <param name="description"></param>
    public record class Weather(string main, string description);

    /// <summary>
    /// Хмарність в %.
    /// </summary>
    /// <param name="all"></param>
    public record class Clouds(int all);

    /// <summary>
    /// Швидкість, пориви та напрямок вітру.
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="deg"></param>
    /// <param name="gust"></param>
    public record class Wind(double speed, double deg, double gust);
}
