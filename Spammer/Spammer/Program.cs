using Spammer.BL;
using TL;

string? fileName = null;
Console.Write("Input name of file with source data(with .txt): ");
fileName = Console.ReadLine();

int timeout = 5 * 60 * 1000;

Console.WriteLine("Main menu(press number of point for the choose)");
Console.WriteLine("1.Start - v.1 ");
Console.WriteLine("2.Start - v.2");
Console.WriteLine($"3.Change timeout(current timeout = {timeout / 1000 / 60} minute, you can't change timeout if spam running)");
Console.WriteLine("(4).If you want stop spamming press - Enter(don't work, JUST FUCKING CLOSE OR RESTART PROGRAM FUCKING BASTARD!");
Console.Write("Choose item: ");
string str = Console.ReadLine();

ReadLinks parser = new ReadLinks("sourceLinks.txt");
LoadLinks loader = new LoadLinks("loadedLinks.txt");
string line = String.Empty;

if (int.TryParse(str, out int result))
{
    if (result == 1)
    {
        SpamAutomation spammer = new SpamAutomation();
        await spammer.authorizationAsync();
        //do
        //{
            await GetProcessingLinkAsync(spammer);
            Thread.Sleep(timeout);
        //} while (Console.ReadKey().Key != ConsoleKey.Enter);
        spammer.Dispose();

    }
    else if (result == 2)
    {
        AllChatsSpam allChatsSpam = new AllChatsSpam();
        
        await allChatsSpam.authorizationAsync();
        await allChatsSpam.GetAllChats();
    }
    else if (result == 3)
    {
        SpamAutomation spammer = new SpamAutomation();
        Console.Write("Input new value(minute) for timeout: ");
        string timeoutStr = Console.ReadLine();
        if (int.TryParse(timeoutStr, out int timeoutStrResult))
        {
            timeout = timeoutStrResult * 60 * 1000;
            Console.WriteLine($"Current timeout = {timeout / 60 / 1000} minute");

            await spammer.authorizationAsync();
            //do
            //{
                await GetProcessingLinkAsync(spammer);
                Thread.Sleep(timeout);
            //} while (Console.ReadKey().Key != ConsoleKey.Enter); // TODO: Пофіксити вихід зі спаму
            
        }
        else
            Console.WriteLine("You input uncorrect value for timeout, restart program and try again.");
        spammer.Dispose();
    }
}

async Task GetProcessingLinkAsync(SpamAutomation spammer)
{
    FileInfo fI = new FileInfo(parser?.FilePath);
    if (fI.Length == 0)
        Console.WriteLine("File is empty, fix it!");

    StreamReader f = new StreamReader(parser?.FilePath);
    while (true)
    {
        line = parser.ReadFile(f);
        if (line is not null)
        {
            line = line.Trim('@');
            await spammer.SpamMessageAsync(line, loader);
            if (SpamAutomation.flag == 1)
            {
                Console.WriteLine(line);
            }
        }
        
        if (!parser.IsReadStr)
            break;
    }
    string strErrors = $"Count of error: {SpamAutomation.errorsCount}";
    Console.WriteLine(strErrors);
    await loader.LoadUnprocessingLinkAsync(strErrors);

    loader.Dispose();

    if (f is not null)
        f?.Dispose();
}

async Task GetProcessedUserChats(AllChatsSpam allChatsSpam)
{
    await GetProcessedUserChats(allChatsSpam);
}





