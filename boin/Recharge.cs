namespace boin;

using OpenQA.Selenium;
using boin.Util;

    // 充值
public class Recharge
{
    // 游戏ID	
    public string GameId { get; set; } = string.Empty;

    // 用户昵称
    public string Nickname { get; set; } = string.Empty;

    // 存款人	
    public string Payer { get; set; } = string.Empty;

    // 订单号
    public string OrderId { get; set; } = string.Empty;

    // 外部订单号
    public string OutsideOrderId { get; set; } = string.Empty;

    // 充值金额
    public Decimal RechargeAmount { get; set; }

    // 首充
    public string FirstRecharge { get; set; } = string.Empty;

    // 实际到账金额
    public Decimal ActualAmount { get; set; }

    // 充值类型
    public string RechargeType { get; set; } = string.Empty;

    // 充值接口(客单充值(不能删)/)
    public string RechargeChannel { get; set; } = string.Empty;

    // VIP期数
    public string VipPeriod { get; set; } = string.Empty;

    // 小费
    public Decimal Tip { get; set; }

    // 取消下注
    public string CancelBet { get; set; } = string.Empty;

    // 时间
    public DateTime Created { get; set; }

    // 说明
    public string Mark { get; set; } = string.Empty;

    public Recharge()
    {
    }

    public bool IsSyncName
    {
        get { return Interlocked.Read(ref nameLocker) == 2; }
    }

    private long nameLocker = 0;

    // 同步姓名
    public void SyncName()
    {
        if (Interlocked.CompareExchange(ref nameLocker, 1, 0) != 0)
        {
            return;
        }

        var chan = this.RechargeChannel;
        var cacheKey = this.OutsideOrderId + chan;
        bool wait = false;
        if (string.IsNullOrEmpty(this.Payer))
        {
            string? name = Cache.GetRecharge(cacheKey);
            if (name != null)
            {
                this.Payer = name;
            }
            else
            {
                if (chan.Contains("四方") && (chan.Contains("银联") || chan.Contains("卡卡")))
                {
                    wait = true;
                }
                else if (chan.Contains("飞天") && (chan.Contains("银联") || chan.Contains("云闪付")))
                {
                    wait = true;
                }
            }
        }

        if (!wait)
        {
            Interlocked.Increment(ref nameLocker);
            return;
        }

        ThreadPool.QueueUserWorkItem(state =>
        {
            var payer = GetRechargeName(chan, this.OutsideOrderId);
            this.Payer = payer ?? string.Empty;
            Interlocked.Increment(ref nameLocker);
            if (payer != null)
            {
                Cache.SaveRecharge(cacheKey, payer);
            }
        });
    }

    public static readonly string[] Heads = new string[]
    {
        "充值账户游戏ID", "用户昵称", "存款人", "订单号", "外部订单号",
        "充值金额", "首充", "实际到账金额", "充值类型", "充值接口", "VIP期数", "时间", "说明"
    };

    public static Recharge Create(IWebElement element)
    {
        using var span = new Span();
        var ts = element.FindElements(By.XPath(".//td"));
        if (ts.Count != Heads.Length)
        {
            throw new ArgumentException("Recharge Create");
        }

        Recharge log = new Recharge()
        {
            GameId = Helper.ReadString(ts[0]), // 充值账户游戏ID
            Nickname = Helper.ReadString(ts[1]), //  用户昵称
            Payer = Helper.ReadString(ts[2]), // 存款人
            OrderId = Helper.ReadString(ts[3]), // 订单号
            OutsideOrderId = Helper.ReadString(ts[4]), // 外部订单号

            RechargeAmount = Helper.ReadDecimal(ts[5]), // 充值金额
            FirstRecharge = Helper.ReadString(ts[6]), // 首充
            ActualAmount = Helper.ReadDecimal(ts[7]), // 实际到账金额
            RechargeType = Helper.ReadString(ts[8]), // 充值类型
            RechargeChannel = Helper.ReadString(ts[9]), // 充值接口
            VipPeriod = Helper.ReadString(ts[10]), // VIP期数
            Created = Helper.ReadDateTime(ts[11]), // 时间
            Mark = Helper.ReadString(ts[12]), // 说明
        };

        span.Msg = "充值:" + log.OrderId;
        return log;
    }

    static string? GetRechargeName(string chan, string orderId)
    {
        string? name = null;
        if (chan.Contains("四方"))
        {
            name = SiFangPay.GetPayer(orderId);
        }
        else if (chan.Contains("飞天"))
        {
            name = FeiTianPay.GetPayer(orderId);
        }

        return name;
    }
}


