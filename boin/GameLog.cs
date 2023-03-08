namespace boin;

using OpenQA.Selenium;
using boin.Util;

// 游戏日志
public class GameLog
{
    // 编号ID
    public string LogId { get; set; } = string.Empty;

    // 游戏ID	
    public string GameId { get; set; } = string.Empty;

    // 游戏平台	
    public string GamePlatform { get; set; } = string.Empty;

    // 单号
    public string No { get; set; } = string.Empty;

    // 游戏名
    public string GameName { get; set; } = string.Empty;

    // 操作时间
    public string OpTime { get; set; } = string.Empty;

    // 下注总金额
    public Decimal TotalBet { get; set; }

    // 中奖金额
    public Decimal TotalWin { get; set; }

    // 有效下注
    public Decimal ValidBet { get; set; }

    // 小费
    public Decimal Tip { get; set; }

    // 取消下注
    public string CancelBet { get; set; } = string.Empty;

    public GameLog()
    {
    }

    // 繁体转为简体
    public void Init()
    {
        var gp = StringUtility.TW2ZH(this.GamePlatform);
        if (gp != this.GamePlatform)
        {
            this.GamePlatform = gp;
        }

        var gn = StringUtility.TW2ZH(this.GameName);
        if (gn != this.GameName)
        {
            this.GameName = gn;
        }
    }

    public bool IsMatch(string platform, string game)
    {
        if (string.IsNullOrEmpty(platform) || platform == "all" || platform == this.GamePlatform)
        {
            return this.GameName.Contains(game);
        }

        return false;
    }

    public static string[] Heads = new string[]
    {
        string.Empty, "编号ID", "游戏ID", "游戏平台", "单号",
        "游戏名", "操作时间", "下注总金额", "中奖金额", "有效下注", "小费", "取消下注", "操作"
    };

    public static GameLog Create(IWebElement element)
    {
        using var span = new Span();
        var ts = element.FindElements(By.XPath(".//td"));
        if (ts.Count != Heads.Length)
        {
            throw new ArgumentException("GameLog Create");
        }

        GameLog log = new GameLog()
        {
            LogId = Helper.ReadString(ts[1]), // 编号ID
            GameId = Helper.ReadString(ts[2]), //游戏ID
            GamePlatform = Helper.ReadString(ts[3]), //游戏平台
            No = Helper.ReadString(ts[4]), //单号
            GameName = Helper.ReadString(ts[5]), //游戏名
            OpTime = Helper.ReadString(ts[6]), //操作时间
            TotalBet = Helper.ReadDecimalOrDefault(ts[7]), //下注总金额
            TotalWin = Helper.ReadDecimalOrDefault(ts[8]), //中奖金额
            ValidBet = Helper.ReadDecimalOrDefault(ts[9]), //有效下注
            Tip = Helper.ReadDecimalOrDefault(ts[10]), //小费
            CancelBet = Helper.ReadString(ts[11]), //取消下注
        };

        log.Init();

        span.Msg = "游志:" + log.LogId;
        return log;
    }
}

public class GameInfo
{
    // 下注金额
    public decimal TotalBet { get; set; }

    // 中奖金额
    public decimal TotalWin { get; set; }

    // 有效下注金额
    public decimal TotalValidBet { get; set; }

    public List<GameLog> GameLogs { get; set; } = new List<GameLog>() { };

    public bool PlayGame(string platform, string game)
    {
        foreach (var log in GameLogs)
        {
            if (log.IsMatch(platform, game))
            {
                return true;
            }
        }

        return false;
    }
}
