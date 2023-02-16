﻿using System;

namespace boin.Review
{
	public class WithdrwReview : IReviewInterface
    {
		public WithdrwReview()
		{
		}

        public List<ReviewResult> Review(User user, Order order)
        {
            List<ReviewResult> rs = new List<ReviewResult>();

            // 当前提现金额大于指定额度
            const int maxAmount = 2000;
            var amount = order.Amount;
            if (amount > maxAmount)
            {
                rs.Add(new ReviewResult { Code = 200, Msg = "@提现" + amount + ">" + maxAmount.ToString() });
            }

            // 当日提现金额大于指定额度
            const int maxDayAmount = 10000;
            amount = user.Funding.ToDay.WithdrawAmount;
            if (amount > maxDayAmount)
            {
                rs.Add(new ReviewResult { Code = 201, Msg = "@当日提现" + amount + ">" + maxDayAmount.ToString() });
            }

   
            return rs;
        }
    }
}

