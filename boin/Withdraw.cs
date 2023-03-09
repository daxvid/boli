namespace Boin;

using OpenQA.Selenium;
using Boin.Util;

public class Withdraw : WithdrawExpand
{
    // 订单号
    public string OrderId { get; set; } = string.Empty;

    // 发起时间
    public DateTime Created { get; set; }

    // 到账时间
    public string TimeToAccount { get; set; } = string.Empty;

    // 游戏ID
    public string GameId { get; set; } = string.Empty;

    // 昵称
    public string NickName { get; set; } = string.Empty;

    // 提现金额
    public decimal Amount { get; set; } = 0;

    // 通道(银行卡/数字钱包/未知=数字钱包)
    public string Way { get; set; } = string.Empty;

    // 审核状态(待审核/已通过/已拒绝)
    public string Review { get; set; } = string.Empty;

    // 转账状态(代付中/未发起/成功/失败)
    public string Transfer { get; set; } = string.Empty;

    // 操作类型
    public string Operating { get; set; } = string.Empty;

    public Withdraw()
    {
    }

    private static readonly string[] Heads = new string[]
    {
        string.Empty, "订单号", "发起时间", "到账时间", "游戏ID", "用户昵称", "提现金额",
        "通道", "状态", "转账", "操作类型"
    };

    public bool Pass()
    {
        if (Review == "已通过" && (Transfer == "成功" || Transfer == "代付中"))
        {
            return true;
        }

        return false;
    }


    public static Withdraw Create(IWebElement element, IWebElement rowEx)
    {
        using var span = new Span();
        var ts = element.FindElements(By.XPath(".//td"));
        if (ts.Count != Heads.Length)
        {
            throw new ArgumentException("Withdraw Create");
        }

        Withdraw order = new Withdraw
        {
            OrderId = Helper.ReadString(ts[1]), // 订单号
            Created = Helper.ReadDateTime(ts[2]), // 发起时间
            TimeToAccount = Helper.ReadString(ts[3]), // 到账时间
            GameId = Helper.ReadString(ts[4]), // 游戏ID
            NickName = Helper.ReadString(ts[5]), // 用户昵称
            Amount = Helper.ReadDecimal(ts[6]), // 提现金额
            Way = Helper.ReadString(ts[7]), // 通道
            Review = Helper.ReadString(ts[8]), // 状态
            Transfer = Helper.ReadString(ts[9]), // 转账
            Operating = Helper.ReadString(ts[10]), // 操作类型
        };
        order.ReadExpand(rowEx, false);
        if (order.Way == "未知")
        {
            order.Way = "数字钱包";
        }

        span.Msg = "提现:" + order.OrderId;
        return order;
    }
    
}

