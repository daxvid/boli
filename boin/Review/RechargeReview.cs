using System;
using System.Collections.ObjectModel;

namespace boin.Review
{
    // 充值记录
	public class RechargeReview : IReviewUser
    {
        ReviewConfig cnf;
        public RechargeReview(ReviewConfig cnf)
        {
            this.cnf = cnf;
        }

        public ReadOnlyCollection<ReviewResult> Review(User user)
        {
            Order order = user.Order;
            List<ReviewResult> rs = new List<ReviewResult>();
            var f = user.Funding;

            if (order.Way == "银行卡")
            {
                // 检查银行卡姓名充值是否一致
                var t2 = f.OtherRechargeAmount(order.Name);
                if (t2 > 0)
                {
                    var name = f.FirstOtherRechargeName(order.Name);
                    rs.Add(new ReviewResult { Code = -401, Msg = "@其它名字充值:" + name });
                }
                else
                {
                    var t1 = f.TotalRechargeAmount(order.Name);
                    if (t1 <= 0)
                    {
                        rs.Add(new ReviewResult { Code = 402, Msg = "@最近无充值:" + order.Name });
                    }
                    else if (t1 < order.Amount)
                    {
                        rs.Add(new ReviewResult { Code = 0, Msg = "@近期充值:" + t1});
                    }
                    else
                    {
                        rs.Add(new ReviewResult { Code = 0, Msg = "@充值通过:" + t1 });
                    }
                }
            }
            else
            {
                var name = f.FirstOtherRechargeName(string.Empty);
                if (string.IsNullOrEmpty(name))
                {
                    rs.Add(new ReviewResult { Code = 0, Msg = "@波币充值通过"});
                }
                else
                {
                    rs.Add(new ReviewResult { Code = -402, Msg = "@其它名字充值:" + name });
                }
            }
            return new ReadOnlyCollection<ReviewResult>(rs);
        }

    }
}

