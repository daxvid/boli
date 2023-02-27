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
        // 所有接收通知的聊天ID
        public List<long> ChatIds;

        // 订单时间小时
        public int OrderHour { get; set; }

        // 提现最多读取页数
        public int WithdrawMaxPage { get; set; }

        // 提现最多天数
        public int WithdrawMaxDay { get; set; }

        // 充值最多读取页数
        public int RechargeMaxPage { get; set; }

        // 充值最多天数
        public int RechargeMaxDay { get; set; }

        // 游戏记录最多读取页数
        public int GameLogMaxPage { get; set; }

        // 游戏记录最多小时
        public int GameLogMaxHour { get; set; }
        
        // 最大锁定单数
        public int OrderMaxLock { get; set; }

        // 审核配置文件
        public string ReviewFile { get; set; }

        // 充值查询地址
        public string RechargeHost { get; set; }

        // Chrome
        public bool Headless { get; set; }

        // redis连接
        public string Redis { get; set; }


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
