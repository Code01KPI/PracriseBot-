using Newtonsoft.Json;

namespace PracriseProject1
{
    /// <summary>
    /// Клас обробки і зберігання координат.
    /// </summary>
    public class Coordinates
    {
        /// <summary>
        /// Широта.
        /// </summary>
        public string? Lat { get; }

        /// <summary>
        /// Довгота.
        /// </summary>
        public string? Lng { get; }

        /// <summary>
        /// Повна, форматована адреса.
        /// </summary>
        public string? fullAdress { get; }

        /// <summary>
        /// Флажок на випадок якщо відповідь від апі не відповідає формі(некоректно введенна назва н. п.).
        /// </summary>
        public bool coordinatesFlag = false;

        public Coordinates(string jsonSource)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<ResponceSource>(jsonSource);
                if (obj?.results.Count == 0)
                    coordinatesFlag = true;
                else
                {
                    fullAdress = obj?.results[0]?.formatted_address;
                    Lat = obj?.results[0]?.geometry?.location?.lat;
                    Lng = obj?.results[0]?.geometry?.location?.lng;
                }
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

/// <summary>
/// Клас для зберігання двох основних полів з json.
/// </summary>
class ResponceSource
{
    public List<ResultsComponent> results = new List<ResultsComponent>();

    public string? status;
}

/// <summary>
/// Клас який описує повну адресу і координати.
/// </summary>
/// <param name="formatted_address"></param>
/// <param name="geometry"></param>
record class ResultsComponent(string formatted_address, Geometry geometry);

/// <summary>
/// Клас для опису повних координат.
/// </summary>
/// <param name="location"></param>
record class Geometry(Location location);

/// <summary>
/// Широта і довгота з JSON geometry.
/// </summary>
/// <param name="lat"></param>
/// <param name="lng"></param>
record class Location(string lat, string lng);




