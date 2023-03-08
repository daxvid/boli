using System;
using System.Collections.ObjectModel;
using boin.Util;

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
        var nearWithdraw = user.Funding.NearSuccessWithdraw(order.OrderId, order.Way, order.CardNo);
        if (order.Way == "银行卡")
        {
            if (nearWithdraw != null && nearWithdraw.Payee != order.Payee)
            {
                rs.Add(new ReviewResult { Code = 200, Msg = "名字不同:" + nearWithdraw.Payee });
            }
            else
            {
                var maskName = Helper.MaskName(order.Payee);
                rs.Add(new ReviewResult { Code = 0, Msg = "@名字通过:" + maskName});
            }
        }
        else if (order.Way == "数字钱包")
        {
            if (nearWithdraw == null)
            {
                rs.Add(new ReviewResult { Code = 201, Msg = "首笔人工:" + order.Payee });
            }
            else if (nearWithdraw.CardNo != order.CardNo)
            {
                rs.Add(new ReviewResult { Code = 202, Msg = "钱包不同:" + nearWithdraw.CardNo });
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

