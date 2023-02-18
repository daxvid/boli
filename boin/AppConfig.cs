using System;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace boin
{
    public class AppConfig
    {
        public string Home { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string GoogleKey { get; set; }

        // Telegram
        public string BotToken { get; set; }
        public List<long> ChatIds;

        // Chrome
        public bool Headless { get; set; }

        // 订单时间小时
        public int OrderHour { get; set; }

        // 审核配置文件
        public string ReviewFile { get; set; }

        // 提现最多读取页数
        public int WithdrawMaxPage { get; set; }

        // 充值最多读取页数
        public int RechargeMaxPage { get; set; }

        // 游戏记录最多读取页数
        public int GameLogMaxPage { get; set; }

        public AppConfig()
        {
        }

        public static AppConfig FromYamlFile(string path)
        {
            string yml = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder().Build();
            var cnf = deserializer.Deserialize<AppConfig>(yml);
            return cnf;
        }

        public static void Test()
        {
            AppConfig cnf = new AppConfig();
            cnf.Home = "https://www.google.com";
            cnf.UserName = "abc";
            cnf.Password = "124334";
            cnf.GoogleKey = "ASBDGFDF";
            cnf.BotToken = "tel token";
            cnf.ChatIds = new List<long> { 34355, 5645634 };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(cnf);
            System.Console.WriteLine(yaml);
        }
    }
}
