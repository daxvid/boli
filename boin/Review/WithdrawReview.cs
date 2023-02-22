using System;
using System.Collections.ObjectModel;

namespace boin.Review
{
    // 审核之前的提现
    public class WithdrwReview : IReviewUser
    {
        ReviewConfig cnf;
        public WithdrwReview(ReviewConfig cnf)
        {
            this.cnf = cnf;
        }


        public ReadOnlyCollection<ReviewResult> Review(Order order)
        {
            return ReviewResult.Empty;
        }


        public ReadOnlyCollection<ReviewResult> Review(User user)
        {
            Order order = user.Order;
            List<ReviewResult> rs = new List<ReviewResult>();
            //最新两笔提款名字不一致-不可以通过
            var nearName = user.Funding.NearBankName();
            if (order.Way == "银行卡")
            {
                if (nearName == order.Payee || string.IsNullOrEmpty(nearName))
                {
                    rs.Add(new ReviewResult { Msg = "@名字通过:" + order.Payee });
                }
                else
                {
                    rs.Add(new ReviewResult { Code = 200, Msg = "@名字不一致:" + nearName });
                }
            }
            else if (order.Way == "数字钱包")
            {
                // 最近10笔提款内 波币不能超过4笔
                var countBobi = user.Funding.NearBobiCount(order.OrderID, ReviewConfig.Cnf.NearWithdrawCount);
                if (countBobi > ReviewConfig.Cnf.BobiMaxCount)
                {
                    rs.Add(new ReviewResult { Code = -402, Msg = "@币次数超限:" + countBobi.ToString() });
                }
            }

            var reject = user.Funding.NearReject(order.OrderID);
            if (reject)
            {
                rs.Add(new ReviewResult { Code = 201, Msg = "@上一单未成功"});
            }

            return new ReadOnlyCollection<ReviewResult>(rs);
        }
    }
}

