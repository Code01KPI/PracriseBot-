using System.Text.RegularExpressions;

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
        public string Lat { get; private set; }

        /// <summary>
        /// Довгота.
        /// </summary>
        public string Lng { get; private set; }

        /// <summary>
        /// Регулярка для пошуку широти.
        /// </summary>
        private Regex latRegex = new Regex(@".lat. : (\d*.\d*)");

        /// <summary>
        /// Регулярка для пошуку довготи.
        /// </summary>
        private Regex lngRegex = new Regex(@".lng. : (\d*.\d*)");

        public Coordinates(string jsonSource)//TODO: Перевірка вхідних даних. 
        {
            string tmp1 = String.Empty;
            string tmp2 = String.Empty;

            MatchCollection matches1 = latRegex.Matches(jsonSource);
            if (matches1.Count >= 3)
            {
                tmp1 = matches1[2].Value;
                tmp1 = tmp1.Replace("\"lat\" : ", "");
            }
            Lat = tmp1;

            MatchCollection matches2 = lngRegex.Matches(jsonSource);
            if (matches2.Count >= 3)
            {
                tmp2 = matches2[2].Value;
                tmp2 = tmp2.Replace("\"lng\" : ", "");
            }
            Lng = tmp2;
        }
    }
}
