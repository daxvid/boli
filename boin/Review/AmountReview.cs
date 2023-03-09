namespace Boin.Review;

using System.Collections.ObjectModel;

// （波币）拒绝原因-
// 今日波币已达上限，请使用银行卡
// 超过单笔波币金额，请使用银行卡
// 请使用银行卡，否则不给予提现
// 波币没有绑定姓名，请绑定姓名 谢谢
// 波币资讯有误请填写正确地址谢谢

// 金额审核
public class AmountReview : IReviewUser
{
    private readonly ReviewConfig config;

    public AmountReview(ReviewConfig config)
    {
        this.config = config;
    }

    public ReadOnlyCollection<ReviewResult> Review(User user)
    {
        List<ReviewResult> rs = new List<ReviewResult>();
        var order = user.Order;
        bool isNew = user.IsNewUser();
        var ac = config.GetAmountConfig(order.Way, isNew);

        // 检查单笔额度
        rs.Add(CheckOnceMax(order.Way, ac.OnceMax, order.Amount));
        // 检查当日总额度
        var dayWithdraw = user.Funding.TotalDayWithdraw(order.Way, order.OrderId);
        rs.Add(CheckDayMax(order.Way, ac.DayMax, dayWithdraw, order.Amount));

        // 检查当日总充值
        var dayRecharge = user.Funding.ToDay.RechargeAmount;
        rs.Add(CheckDayRecharge(order.Way, ac.DayRecharge, dayRecharge));

        // 检查如果玩了某种游戏，当日提现总额限制
        var r4 = CheckDayGames(user, ac.DayMaxGames, dayWithdraw, order.Amount);
        if (r4 != null)
        {
            rs.Add(r4);
        }

        return new ReadOnlyCollection<ReviewResult>(rs);
    }

    // 检查每笔提现金额
    private static ReviewResult CheckOnceMax(string way, decimal max, decimal amount)
    {
        return way switch
        {
            // 检查老用户单笔银行卡限制
            "银行卡" => amount > max
                ? new ReviewResult { Code = 201, Msg = "卡单笔超限" + max + ":" + amount }
                : new ReviewResult { Code = 0, Msg = "@单笔通过:" + amount },

            // 检查老用户单笔波币限制
            "数字钱包" => amount > max
                ? new ReviewResult { Code = -202, Msg = "超过单笔波币金额，请使用银行卡" }
                : new ReviewResult { Code = 0, Msg = "@单笔通过:" + amount },

            // 未知的通道
            _ => new ReviewResult { Code = 203, Msg = "未知的通道:" + way }
        };
    }

    // 检查当日提现金额
    private static ReviewResult CheckDayMax(string way, decimal max, decimal dayAmount, decimal amount)
    {
        var checkAmount = dayAmount + amount;
        return way switch
        {
            // 检查老用户当日银行卡限制
            "银行卡" => checkAmount > max
                ? new ReviewResult { Code = 201, Msg = "卡当日提款超限" + max + ":" + checkAmount }
                : new ReviewResult { Code = 0, Msg = "@当日通过:" + checkAmount },

            // 检查老用户当日波币限制
            "数字钱包" => checkAmount > max
                ? new ReviewResult { Code = -202, Msg = "今日波币已达上限，请使用银行卡" }
                : new ReviewResult { Code = 0, Msg = "@当日通过:" + checkAmount },

            // 未知的通道
            _ => new ReviewResult { Code = 203, Msg = "未知的通道:" + way }
        };
    }

    // 检查当日充值金额
    private static ReviewResult CheckDayRecharge(string way, decimal max, decimal amount)
    {
        return way switch
        {
            // 检查当日银行卡充值限制
            "银行卡" => amount > max
                ? new ReviewResult { Code = 401, Msg = "日充超限需人工" + max + ":" + amount }
                : new ReviewResult { Code = 0, Msg = "@日充通过:" + amount },

            // 检查当日波币充值限制
            "数字钱包" => amount > max
                ? new ReviewResult { Code = 402, Msg = "日充超限需人工" + max + ":" + amount }
                : new ReviewResult { Code = 0, Msg = "@日充通过:" + amount },

            // 未知的通道
            _ => new ReviewResult { Code = 203, Msg = "未知的通道:" + way }
        };
    }

    // 检查如果玩了某种游戏，当日提现总额限制
    private static ReviewResult? CheckDayGames(User user, Dictionary<string, decimal> dayMaxGames, decimal dayAmount,
        decimal amount)
    {
        var checkAmount = dayAmount + amount;
        foreach (var kv in dayMaxGames)
        {
            if (user.GameInfo.PlayGame(string.Empty, kv.Key))
            {
                // 游戏日提限制
                if (checkAmount > kv.Value)
                {
                    var gameName = kv.Key; //.Replace("all", "");
                    var msg = "@游戏[" + gameName + "]日提超限:" + kv.Value + "<" + checkAmount;
                    return new ReviewResult { Code = -401, Msg = msg };
                }
            }
        }

        return null;
    }
}


