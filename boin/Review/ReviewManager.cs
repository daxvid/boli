using System;
namespace boin.Review
{
	public class ReviewManager
	{
        static List<IReviewInterface> reviews = new List<IReviewInterface>();

		static ReviewManager()
		{
			reviews.Add(new BankCardReview());
            reviews.Add(new WithdrwReview());
            reviews.Add(new RechargeReview());
            reviews.Add(new GameReview());
        }

        // 审核
        public static bool Review(User user)
		{
			List<ReviewResult> results = new List<ReviewResult>();
			foreach (var review in reviews)
			{
				var rs = review.Review(user, user.Order);
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
			user.Pass = true;
            user.ReviewResult = results;
			return true;
		}
    }
}

