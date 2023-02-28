namespace boin;
using boin.Bot;
using boin.Util;

class Program
{
    static void Main(string[] args)
    {
        AppConfig cnf = AppConfig.FromYamlFile("app.yaml");
        AuthConfig authCnf = AuthConfig.FromYamlFile("auth.yaml");
        Recharge.RechargeHost = authCnf.RechargeHost;
        TelegramBot.Instance.Run(authCnf);

        while (true)
        {
            using (BoinClient client = new BoinClient(cnf, authCnf))
            {
                try
                {
                    client.Run();
                }
                catch (Exception err)
                {
                    try
                    {
                        Helper.TakeScreenshot(client.driver ,err);
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

