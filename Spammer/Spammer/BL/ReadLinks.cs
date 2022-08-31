
namespace Spammer.BL
{
    internal class ReadLinks
    {
        /// <summary>
        /// Шлях до файла.
        /// </summary>
        public string? FilePath { get; } 

        /// <summary>
        /// Стан зчитування.
        /// </summary>
        public bool IsReadStr { get; private set; }

        public ReadLinks(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("File name can't be a null or empty!");

            FilePath = fileName;
        }

        /// <summary>
        /// Метод для зчитування.
        /// </summary>
        /// <returns>Один зчитаний рядок</returns>
        public string ReadFile(StreamReader f)
        {
            IsReadStr = false;
            if (!f.EndOfStream)
            {
                string link = String.Empty;
                link = f.ReadLine();

                IsReadStr = true;
                return link;
            }
            return null;
        }
    }
}
