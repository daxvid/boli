namespace boin;

using YamlDotNet.Serialization;

public class AuthConfig
{
    
    public string Home { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string GoogleKey { get; set; }

    // Telegram
    public string BotToken { get; set; }

    // 所有接收通知的聊天ID
    public List<long> ChatIds;
    
    // 充值查询地址
    public string RechargeHost { get; set; }
    
    // redis连接
    public string Redis { get; set; }
    
    public static AuthConfig FromYamlFile(string path)
    {
        string yml = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder().Build();
        var cnf = deserializer.Deserialize<AuthConfig>(yml);
        return cnf;
    }
}