﻿using System;
namespace boin.Review
{
	public class ReviewManager
    {
        public ReviewConfig Cnf;
        List<IReviewUser> userReviews = new List<IReviewUser>();
        List<IReviewOrder> orderReviews = new List<IReviewOrder>();

        //public 

        public ReviewManager(string cnfFile)
		{
            var cnf = ReviewConfig.FromYamlFile(cnfFile);
            Cnf = cnf;

            orderReviews.Add(new BankCardReview(cnf));

            userReviews.Add(new BankCardReview(cnf));
            userReviews.Add(new AmountReview(cnf));
            userReviews.Add(new WithdrwReview(cnf));
            userReviews.Add(new RechargeReview(cnf));
            userReviews.Add(new GameReview(cnf));
        }

        // 审核提现单
        public bool Review(Order order)
        {
            var results = new List<ReviewResult>();
            foreach (var review in orderReviews)
            {
                var rs = review.Review(order);
                if (rs != null && rs != ReviewResult.Empty)
                {
                    results.AddRange(rs);
                    foreach (var r in rs)
                    {
                        // 代码为负，强制中断
                        if (r.Code < 0)
                        {
                            order.ReviewResult = results;
                            return false;
                        }
                    }
                }
            }
            order.Pass = true;
            order.ReviewResult = results;
            return true;
        }

        // 审核用户
        public bool Review(User user)
		{
			List<ReviewResult> results = new List<ReviewResult>();
            results.AddRange(user.Order.ReviewResult);
            foreach (var review in userReviews)
			{
				var rs = review.Review(user);
                if (rs != null && rs != ReviewResult.Empty)
                {
                    results.AddRange(rs);
                    foreach (var r in rs)
                    {
                        // 代码为负，强制中断
                        if (r.Code < 0)
                        {
                            user.ReviewResult = results;
                            return false;
                        }
                    }
                }
			}
			user.Pass = true;
            user.ReviewResult = results;
			return true;
		}

    }
}

