using System;
using System.Collections.ObjectModel;

namespace boin.Review;

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
        var nearName = user.Funding.NearBankName(order.OrderId, order.Way, order.CardNo);
        if (order.Way == "银行卡")
        {
            if (nearName == order.Payee || string.IsNullOrEmpty(nearName))
            {
                rs.Add(new ReviewResult { Code = 0, Msg = "@名字通过:" + order.Payee });
            }
            else
            {
                rs.Add(new ReviewResult { Code = 200, Msg = "名字不同:" + nearName });
            }
        }
        else if (order.Way == "数字钱包")
        {
            if (string.IsNullOrEmpty(nearName))
            {
                rs.Add(new ReviewResult { Code = 201, Msg = "首笔人工:" + order.Payee });
            }
            else if (nearName != order.Payee)
            {
                rs.Add(new ReviewResult { Code = 202, Msg = "名字不同:" + nearName });
            }
            else
            {
                // 最近10笔提款内 波币不能超过4笔
                var countBobi = user.Funding.NearBobiCount(order.OrderId, ReviewConfig.Cnf.NearWithdrawCount);
                if (countBobi > ReviewConfig.Cnf.BobiMaxCount)
                {
                    rs.Add(new ReviewResult { Code = -402, Msg = "请使用银行卡，否则不给予提现" });
                }
            }
        }

        var nearPass = user.Funding.NearPass(order.OrderId);
        if (!nearPass)
        {
            rs.Add(new ReviewResult { Code = 201, Msg = "上一单未成功" });
        }

        return new ReadOnlyCollection<ReviewResult>(rs);
    }
}

