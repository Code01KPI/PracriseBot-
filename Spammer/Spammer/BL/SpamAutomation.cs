using TL;

namespace Spammer.BL
{
    internal class SpamAutomation : IDisposable
    {
        /// <summary>
        /// Кількість чатів в які не було надіслано повідомлення через помилку.
        /// </summary>
        public static int errorsCount = 0;

        /// <summary>
        /// Флажок на випадок перевірки в супергруппі.
        /// </summary>
        public static int flag = 0;

        /// <summary>
        /// Клієнт ліби.
        /// </summary>
        private WTelegram.Client? client = null;

        /// <summary>
        /// Повідомлення, яке мало бути надіслано в чат.
        /// </summary>
        private Message? message = null;

        /// <summary>
        /// Об'єкт опису приєднання до супергрупи/чату.
        /// </summary>
        private UpdatesBase? joinUpdate = null;

        public SpamAutomation() => client = new WTelegram.Client();

        public async Task authorizationAsync()
        {
            if (client == null)
                throw new Exception("There are don't create WTelegram client!");

            var my = await client.LoginUserIfNeeded();
            Console.WriteLine($"We are logged-in as {my.username ?? my.first_name + " " + my.last_name} (id {my.id})");
        }

        /// <summary>
        /// Звільнення ресурсів.
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("client disposed");
            client?.Dispose();
        }

        public async Task SpamMessageAsync(string link, LoadLinks loader)
        {
            flag = 0;
            if (client == null)
                throw new Exception("There are don't create WTelegram client or you don't do the authorization!");

            try
            {
                var chatPeer = await client.Contacts_ResolveUsername(link); // receive the peer of chat/channel(supergroup)
                if (chatPeer.Chat is Channel channel)
                {
                    await Task.Delay(1000);

                    joinUpdate = await client.Channels_JoinChannel(channel); // TODO: Do processing exception of join to channel
                    if (joinUpdate is null) // -> load unprocessing link to txt file
                    {
                        await loader.LoadUnprocessingLinkAsync($"Couldn't joined to channel - @{link} - {joinUpdate?.Date}");
                        throw new Exception($"Couldn't joined to channel - @{link} - {joinUpdate?.Date}");
                    }

                    await Task.Delay(1000);
                    joinUpdate = null;

                    //await Task.Delay(2000);
                    //message = await client.SendMessageAsync(chatPeer, "привет");


                    /*if (message.ID == 0) // -> load unprocessing link to txt file
                    {
                        await loader.LoadUnprocessingLinkAsync(link);
                        throw new Exception("Couldn't sent message!");
                    }*/
                    //await Task.Delay(10000);
                    //Thread.Sleep(5000);
                }
                else
                {
                    if (chatPeer.Chat is Chat chat)
                        Console.WriteLine($"@{link} - chat");
                }
            }
            catch (RpcException ex)
            {
                ++errorsCount;
                Console.WriteLine(ex.Message);
                await loader.LoadUnprocessingLinkAsync($"Couldn't joined to channel - @{link} - RcpException");
                await DelayMethodAsync(ex);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Метод для очікування для уникнення flood exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task DelayMethodAsync(RpcException ex)
        {
            if (ex.Code == 420)
            {
                await Task.Delay(ex.X * 1000 + 1000);
            }
        }

        /// <summary>
        /// Фіналізатор для звільнення ресурсів.
        /// </summary>
        /*~SpamAutomation()
        {
            if (client is not null)
                client.Dispose();
        }*/
    }
}
