using System;
namespace boin.Review
{
	public class ReviewResult
    {
        public int Code;
        public string Msg;
    }

    // 审核接口
    public interface IReviewInterface
	{
        List<ReviewResult> Review(User user, Order order);

    }
}

