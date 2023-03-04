namespace boin;

using YamlDotNet.Serialization;

public class FeiTianConfig
{
    // 飞天充值查询地址
    public string Host { get; set; }
    public string Merchant { get; set; }
    public string Token { get; set; }
}

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
    
    // 四方充值查询地址
    public string SiFangHost { get; set; }
    
    // 飞天充值查询地址
    public FeiTianConfig FeiTian { get; set; }
    
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