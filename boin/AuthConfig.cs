namespace Boin;

using YamlDotNet.Serialization;

public class FeiTianConfig
{
    // 飞天充值查询地址
    public string Host { get; set; } = string.Empty;
    public string Merchant { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class AuthConfig
{
    
    public string Home { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string GoogleKey { get; set; } = string.Empty;

    // Telegram
    public string BotToken { get; set; } = string.Empty;

    // 所有接收通知的聊天ID
    public List<long> ChatIds = new List<long>();
    
    // 四方充值查询地址
    public string SiFangHost { get; set; } = string.Empty;
    
    // 飞天充值查询地址
    public FeiTianConfig FeiTian { get; set; } = new FeiTianConfig();
    
    // redis连接
    public string Redis { get; set; } = string.Empty;
    
    // # my=meiying, yr=yiren 
    public string Platform { get; set; } = string.Empty;
    
    public static AuthConfig FromYamlFile(string path)
    {
        var yml = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder().Build();
        var cnf = deserializer.Deserialize<AuthConfig>(yml);
        return cnf;
    }
}