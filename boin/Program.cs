namespace boin;
using boin.Bot;
using boin.Util;

class Program
{
    static void Main(string[] args)
    {
        AppConfig cnf = AppConfig.FromYamlFile("app.yaml");
        AuthConfig authCnf = AuthConfig.FromYamlFile("auth.yaml");
        SiFangPay.Host = authCnf.SiFangHost;
        FeiTianPay.Cnf = authCnf.FeiTian;

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
                        Log.SaveException(err, client.driver);
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

