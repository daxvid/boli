namespace boin;

class Program
{
    static void Main(string[] args)
    {
        BoinClient client = new BoinClient(Auth.Home, Auth.UserName, Auth.Password, Auth.GoogleKey);
        client.Run();
        var s = Console.ReadKey(true).KeyChar;
        while (Char.ToLower(s) != 'q')
        {
            s = Console.ReadKey(true).KeyChar;
            Console.Write(s);
        }
        client.Quit();
    }

}

