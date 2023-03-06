using System;
using System.Collections.ObjectModel;

namespace boin.Review;

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

        // if (f.ExistsChan("飞天"))
        // {
        //     rs.Add(new ReviewResult { Code = 201, Msg = 飞天人工:" + order.Payee });
        //     return new ReadOnlyCollection<ReviewResult>(rs);;
        // }

        if (order.Way == "银行卡")
        {
            // 检查银行卡姓名充值是否一致
            var t2 = f.OtherRechargeAmount(order.Payee);
            if (t2 > 0)
            {
                var name = f.FirstOtherRechargeName(order.Payee);
                rs.Add(new ReviewResult { Code = 401, Msg = "其它名字充值:" + name });
            }
            else
            {
                var t1 = f.TotalRechargeAmount(order.Payee);
                if (t1 <= 0)
                {
                    rs.Add(new ReviewResult { Code = 402, Msg = "最近无充值:" + order.Payee });
                }
                else if (t1 < order.Amount)
                {
                    rs.Add(new ReviewResult { Code = 0, Msg = "@总充通过:" + t1 });
                }
                else
                {
                    rs.Add(new ReviewResult { Code = 0, Msg = "@总充通过:" + t1 });
                }
            }
        }
        else
        {
            var name = f.FirstOtherRechargeName(order.Payee);
            if (string.IsNullOrEmpty(name))
            {
                rs.Add(new ReviewResult { Code = 0, Msg = "@币充值通过" });
            }
            else
            {
                rs.Add(new ReviewResult { Code = 402, Msg = "其它名字充值:" + name });
            }
        }

        var rbc = RechargeByChannel(user);
        if (rbc != null)
        {
            rs.Add(rbc);
        }

        return new ReadOnlyCollection<ReviewResult>(rs);
    }


    // 针对充值接口显示『客单充值(不能删)』
    // 计算日期:上一笔提款日-最新一笔提款日
    // 金额>2000-人工审核
    // 金额<2000，笔数小于3笔的-可以机器人审核，笔数>三笔的-人工审核
    public ReviewResult RechargeByChannel(User user)
    {
        Order order = user.Order;
        DateTime startTime;
        if (!user.Funding.LastSuccessWithdrawTime(order.OrderId, out startTime))
        {
            startTime = DateTime.Now.AddDays(-21);
        }

        foreach (var kv in cnf.RechargeChannel)
        {
            var pair = user.Funding.TotalRechargeByChannel(kv.Key, startTime);
            if (pair.Item1 > kv.Value[0])
            {
                return new ReviewResult { Code = 400, Msg = kv.Key + kv.Value[0] + ":" + pair.Item1 };
            }
            else if (pair.Item2 > kv.Value[1])
            {
                return new ReviewResult { Code = 400, Msg = kv.Key + kv.Value[0] + ":" + pair.Item2 + "笔" };
            }
        }

        return null;
    }
}

