namespace boin;
using boin.Bot;
using boin.Review;

class Program
{
    static void Main(string[] args)
    {
        AppConfig cnf = AppConfig.FromYamlFile("app.yaml");

        TelegramBot.Instance.Run(cnf);

        BoinClient client = new BoinClient(cnf);
        client.Run();

        // while(true){
        //     Thread.Sleep(1000);
        // }
        
        var s = Console.ReadKey(true).KeyChar;
        while (Char.ToLower(s) != 'q')
        {
            s = Console.ReadKey(true).KeyChar;
            Console.Write(s);
        }
        client.Quit();
    }
}

