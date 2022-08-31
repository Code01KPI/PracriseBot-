
namespace Spammer.BL
{
    internal class LoadLinks : IDisposable
    {
        /// <summary>
        /// Потік запису.
        /// </summary>
        private StreamWriter sW { get; set; }


        public LoadLinks(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("File name can't be a null or empty!");

            sW = new StreamWriter(fileName, true);
        }

        public async Task LoadUnprocessingLinkAsync(string line)
        {
            await sW.WriteLineAsync(line);
            await sW.FlushAsync();
        }

        /// <summary>
        /// Звільнення ресурсів sW.
        /// </summary>
        public void Dispose()
        {
            sW?.Dispose();
            Console.WriteLine("sW disposed");
        }
    }
}
