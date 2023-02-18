using System;
using System.Collections.ObjectModel;

namespace boin.Review
{
    // 银行卡审核
    public class BankCardReview : IReviewInterface
    {
        ReviewConfig cnf;
        public BankCardReview(ReviewConfig cnf)
        {
            this.cnf = cnf;
        }

        public ReadOnlyCollection<ReviewResult> Review(Order order)
        {
            List<ReviewResult> rs = new List<ReviewResult>();
            if (order.Way == "银行卡")
            {
                var bankInfo = BankUtil.GetBankInfo(order.CardNo);
                // 银行卡状态。值：ok，no。
                if (!bankInfo.stat.Equals("ok"))
                {
                    rs.Add(new ReviewResult { Code = -101, Msg = "@卡不可使用" + order.CardNo });
                }
                // 有效性，是否正确有效。值：true为是，false为否。
                else if (!bankInfo.validated)
                {
                    rs.Add(new ReviewResult { Code = -102, Msg = "@卡号不正确:" + order.CardNo });
                }
                if (bankInfo.key != order.CardNo)
                {
                    rs.Add(new ReviewResult { Code = 101, Msg = "@卡待定:" + order.CardNo });
                }
                else
                {
                    string bankname = BankUtil.GetNameOfBank(bankInfo.bank);
                    rs.Add(new ReviewResult { Code = 102, Msg = "@卡正确:" + bankname });
                }
            }
            else if (order.Way == "数字钱包")
            {
                // TODO: 检查地址格式
            }
            else
            {
                rs.Add(new ReviewResult { Code = -103, Msg = "@未知的通道:" + order.Way });
            }
            return new ReadOnlyCollection<ReviewResult>(rs);
        }


        public ReadOnlyCollection<ReviewResult> Review(User user)
        {
            return ReviewResult.Empty;
        }
    }
}


