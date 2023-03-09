namespace Boin;

using System.Text;
using OpenQA.Selenium;
using Boin.Review;
using Boin.Util;

public enum OrderReviewEnum
{
        None = 0,     // 未审核
        Pass = 1,     // 审核通过
        Reject = 2,   // 拒绝
        Doubt = 3,    // 有疑问，需人工
}

// 提现订单
public class Order : WithdrawExpand
{
    // 订单号
    public string OrderId { get; set; } = string.Empty;

    // 创建时间
    public DateTime Created { get; set; }

    // 到账时间
    public string TimeToAccount { get; set; } = string.Empty;

    // 游戏ID
    public string GameId { get; set; } = string.Empty;

    // 昵称
    public string NickName { get; set; } = string.Empty;

    // 提现金额
    public decimal Amount { get; set; } = 0;

    // 通道
    public string Way { get; set; } = string.Empty;

    // 审核状态
    public string Review { get; set; } = string.Empty;

    // 转账状态
    public string Transfer { get; set; } = string.Empty;

    // 操作类型
    public string Operating { get; set; } = string.Empty;

    // 操作人
    public string Operator { get; set; } = string.Empty;

    // 提现备注
    public string Remark { get; set; } = string.Empty;

    // 订单状态
    public string Status { get; set; } = string.Empty;

    public OrderReviewEnum ReviewMsg { get; set; }

    public List<Review.ReviewResult> ReviewResult { get; set; } = new List<ReviewResult>() { };

    public GameBind? Bind { get; set; }

    // 是否已处理
    public bool Processed { get; set; }

    public Order()
    {
    }

    public bool CanPass
    {
        get
        {
            foreach (var r in ReviewResult)
            {
                if (r.Code != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public bool CanReject
    {
        get
        {
            foreach (var r in ReviewResult)
            {
                if (r.Code < 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public string RejectReason
    {
        get
        {
            foreach (var r in ReviewResult)
            {
                if (r.Code < 0)
                {
                    return r.Msg;
                }
            }

            return "成功";
        }
    }

    public string DoubtReason
    {
        get
        {
            var msg = "@";
            foreach (var r in ReviewResult)
            {
                if (r.Code > 0)
                {
                    if (msg.Length > 1)
                    {
                        msg += ";";
                    }

                    msg += r.Msg;
                }
            }

            return msg;
        }
    }

    public string ReviewNote()
    {
        StringBuilder sb = new StringBuilder(1024);
        sb.Append(OrderId).Append(":").AppendLine(ReviewMsg.ToString());
        //sb.Append("game:").AppendLine(this.GameId);
        foreach (var r in ReviewResult)
        {
            if (r.Code != 0)
            {
                sb.Append(r.Code).Append(":");
            }

            sb.AppendLine(r.Msg);
        }

        var m = sb.ToString();
        return m;
    }

    private static readonly string[] Heads = new string[]
    {
        string.Empty, "订单号", "发起时间", "到账时间", "游戏ID",
        "用户昵称", "提现金额", "通道", "状态", "转账", "操作类型", "操作人", "提现备注", "操作"
    };

    public static Order Create(IWebElement element, IWebElement rowEx)
    {
        using var span = new Span();
        var ts = element.FindElements(By.XPath(".//td"));
        if (ts.Count != Heads.Length)
        {
            throw new ArgumentException("Order Create");
        }

        var orderId = Helper.ReadString(ts[1]);
        var order = new Order()
        {
            OrderId = orderId, // 订单号
            Created = Helper.ReadShortTime(ts[2]), // 发起时间
            TimeToAccount = Helper.ReadString(ts[3]), // 到账时间
            GameId = Helper.ReadString(ts[4]), // 游戏ID"
            NickName = Helper.ReadString(ts[5]), // 用户昵称
            Amount = Helper.ReadDecimal(ts[6]), // 提现金额
            Way = Helper.ReadString(ts[7]), // 通道
            Review = Helper.ReadString(ts[8]), // 状态
            Transfer = Helper.ReadString(ts[9]), // 转账
            Operating = Helper.ReadString(ts[10]), // 操作类型
            Operator = Helper.ReadString(ts[11]), //  操作人
            Remark = Helper.ReadString(ts[12]), // 提现备注
            Status = Helper.ReadString(ts[13]), //  操作
        };

        order.ReadExpand(rowEx, order.Way == "银行卡");
        span.Msg = "订单:" + order.OrderId;
        return order;
    }
}
