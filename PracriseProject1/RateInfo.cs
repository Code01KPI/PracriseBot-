using System;
using System.Net;
using System.Text.RegularExpressions;

namespace PracriseProject1
{
    /// <summary>
    /// Парсинг і зберігання даних про курс валюти.
    /// </summary>
    public class RateInfo
    {
        /// <summary>
        /// Актуальна дата.
        /// </summary>
        private string date { get; } = string.Empty;

        /// <summary>
        /// Адреса звідки будемо парсити.
        /// </summary>
        private string url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";

        /// <summary>
        /// Спарсені дані.
        /// </summary>
        private string sourceLine = string.Empty;

        /// <summary>
        /// Регулярний вираз для парсинга курса долара.
        /// </summary>
        private Regex regex = new Regex(@".Долар США.,.rate.:(.*),.c");

        /// <summary>
        /// Значення курсу долара.
        /// </summary>
        public string dollarRate { get; private set; }

        public RateInfo()
        {
            /*HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader myStreamReader = new StreamReader(httpWebResponse.GetResponseStream());
            html = myStreamReader.ReadLine().Replace("\"", "$");*/

            WebClient wc = new WebClient();
            sourceLine = wc.DownloadString(url);

            Match match = regex.Match(sourceLine);
            dollarRate = match.Groups[1].Value.ToString();
        }
    }
}
