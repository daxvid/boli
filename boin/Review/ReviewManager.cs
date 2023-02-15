using System;
namespace boin.Review
{
	public class ReviewManager
	{
        static List<IReviewInterface> reviews = new List<IReviewInterface>();

		// 审核
		public static List<ReviewResult> Review(User user, Order order)
		{
			List<ReviewResult> results = new List<ReviewResult>();
			foreach (var review in reviews)
			{
				var rs = review.Review(user, order);
				results.AddRange(rs);
				foreach (var r in rs)
				{
					// 代码为负，强制中断
					if (r.Code < 0)
					{
						return results;
					}
				}
			}
			return results;
		}
    }
}

