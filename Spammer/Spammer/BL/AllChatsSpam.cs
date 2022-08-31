using TL;

namespace Spammer.BL
{
    internal class AllChatsSpam : IDisposable
    {
        /// <summary>
        /// Клієнт ліби.
        /// </summary>
        private WTelegram.Client? client = null;

        /// <summary>
        /// Число необроблених посилень з файлу через помилку .
        /// </summary>
        public static int unprocessingLinksCount = 0;

        /// <summary>
        /// Все чаты в которых состоит пользователь.
        /// </summary>
        private IEnumerable<ChatBase> targetChats = new List<ChatBase>();

        public AllChatsSpam() => client = new WTelegram.Client();

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

        /// <summary>
        /// Отримання всіх чатів користувача та отримання даних чатів з файлу.
        /// </summary>
        /// <returns></returns>
        public async Task GetAllChats() // TODO: доробити передачу назви source file
        {

            Messages_Chats allChats = await client.Messages_GetAllChats();
            List<Contacts_ResolvedPeer> chatsFromSourceFile = new List<Contacts_ResolvedPeer>();

            using (StreamReader sR = new StreamReader("sourceLinks.txt"))
            {
                while (!sR.EndOfStream)
                {
                    string line = null;
                    line = sR.ReadLine();
                    line = line.Trim('@');
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            chatsFromSourceFile.Add(await client.Contacts_ResolveUsername(line));
                        }
                        catch (RpcException ex)
                        {
                            Console.WriteLine($"{ex.Message} {ex.Code}");
                            ++unprocessingLinksCount; // TODO: добавити завантаженні необроблених посилань в файл.

                            if (ex.Code == 420)
                            {
                                await DelayMethodAsync(ex);
                            }
                        }
                        await Task.Delay(500);
                    }
                }
            }

            targetChats = from a in allChats.chats
                          from f in chatsFromSourceFile
                          where a.Key == f.Chat.ID
                          select allChats.chats[a.Key];

            Console.WriteLine($"targetChat count: {targetChats.Count()}");

            foreach (var chat in targetChats)
            {
                switch (chat)
                {
                    case Chat smallgroup when (smallgroup.flags & Chat.Flags.deactivated) == 0:
                        Console.WriteLine($"{chat.ID}:  Small group: {smallgroup.title} with {smallgroup.participants_count} members");
                        break;
                    case Channel channel when (channel.flags & Channel.Flags.broadcast) != 0:
                        Console.WriteLine($"{chat.ID}: Channel {channel.username}: {channel.title}");
                        //Console.WriteLine($"              → access_hash = {channel.access_hash:X}");
                        break;
                    case Channel group: // no broadcast flag => it's a big group, also called supergroup or megagroup
                        Console.WriteLine($"{chat.ID}: Group {group.username}: {group.title}");
                        //Console.WriteLine($"              → access_hash = {group.access_hash:X}");
                        break;
                }
                InputPeer peer = allChats.chats[chat.ID];
                try
                {
                    var message = client.SendMessageAsync(peer, "Привет");
                }
                catch (RpcException ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Code == 420)
                    {
                        await DelayMethodAsync(ex);
                    }
                }
                await Task.Delay(500);
            }
        }

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
    }
}

