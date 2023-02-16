using System;

namespace boin.Review
{
    // 银行卡审核
    public class BankCardReview : IReviewInterface
    {
        public BankCardReview()
        {
        }


        public List<ReviewResult> Review(User user, Order order)
        {
            List<ReviewResult> rs = new List<ReviewResult>();
            var f = user.Funding.Nearly2Months;

            if (order.Way == "银行卡")
            {
                if (f.FirstWithdraw(order.CardNo, order.Name))
                {
                    var bankInfo = BankUtil.GetBankInfo(order.CardNo);
                    // 银行卡状态。值：ok，no。
                    if (!bankInfo.stat.Equals("ok"))
                    {
                        rs.Add(new ReviewResult { Code = -100, Msg = "@银行卡不可使用" + order.CardNo });
                    }
                    // 有效性，是否正确有效。值：true为是，false为否。
                    else if (!bankInfo.validated)
                    {
                        rs.Add(new ReviewResult { Code = -101, Msg = "@银行卡号不正确:" + order.CardNo });
                    }
                }
            }
            else if (order.Way == "数字钱包")
            {
                rs.Add(new ReviewResult { Code = 102, Msg = "@数字钱包" });
            }
            else
            {
                rs.Add(new ReviewResult { Code = -103, Msg = "@未知的通道:" + order.Way });
            }

            return rs;
        }

    }
}

