namespace boin;
using boin.Bot;
using boin.Util;

class Program
{
    static void Main(string[] args)
    {
        AppConfig cnf = AppConfig.FromYamlFile("app.yaml");
        Recharge.RechargeHost = cnf.RechargeHost;
        TelegramBot.Instance.Run(cnf);

        while (true)
        {
            using (BoinClient client = new BoinClient(cnf))
            {
                try
                {
                    client.Run();
                }
                catch (Exception err)
                {
                    try
                    {
                        client.TakeScreenshot(err);
                        Helper.SendMsg(err);
                    }
                    catch
                    {
                    }
                    Thread.Sleep(30 * 1000);
                }
            }
        }

    }
}

