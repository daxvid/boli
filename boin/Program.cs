namespace boin;
using boin.Review;

class Program
{
    static void Main(string[] args)
    {
        var testCard = "6222801251011210972";
        var c = BankCardReview.Chenk(testCard);
        var d = BankUtil.GetBankInfo(testCard);

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

