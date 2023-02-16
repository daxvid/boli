using System;

namespace boin.Review
{
    // 充值记录
	public class RechargeReview : IReviewInterface
    {
		public RechargeReview()
		{
		}

        public List<ReviewResult> Review(User user, Order order)
        {
            List<ReviewResult> rs = new List<ReviewResult>();
            var f = user.Funding.Nearly2Months;

            if (order.Way == "银行卡")
            {
                // 检查银行卡姓名充值是否一致
                var tr = f.TotalRechargeAmount(order.Name);
                if (tr <= 0)
                {
                    rs.Add(new ReviewResult { Code = 100, Msg = "@未充值:" + order.Name });
                }
                else if (tr < order.Amount)
                {
                    rs.Add(new ReviewResult { Code = 101, Msg = "@充值:" + order.Name + "]" });
                }
            }

            return rs;

        }
    }
}

