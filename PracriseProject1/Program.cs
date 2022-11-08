using PracriseProject1;

try
{
    Bot bot = new Bot();
    await bot.StartAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


