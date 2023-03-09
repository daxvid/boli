namespace Boin;

using Boin.Bot;
using Boin.Util;

class Program
{
    static void Main(string[] args)
    {
        var cnf = AppConfig.FromYamlFile("app.yaml");
        var authCnf = AuthConfig.FromYamlFile("auth.yaml");
        SiFangPay.Host = authCnf.SiFangHost;
        FeiTianPay.Cnf = authCnf.FeiTian;

        TelegramBot.Instance.Run(authCnf);

        while (true)
        {
            using var client = new BoinClient(cnf, authCnf);
            try
            {
                client.Run();
            }
            catch (Exception err)
            {
                client.SaveException(err);
            }
            Thread.Sleep(30 * 1000);
        }
    }
}
