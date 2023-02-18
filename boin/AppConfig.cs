using System;
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

        public string BotToken { get; set; }
        public List<long> ChatIds;

        public AppConfig()
        {
        }

        public static AppConfig FromYamlFile(string path)
        {
            string yml = System.IO.File.ReadAllText(path);
            var deserializer = new DeserializerBuilder().Build();
            var p = deserializer.Deserialize<AppConfig>(yml);
            return p;
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
