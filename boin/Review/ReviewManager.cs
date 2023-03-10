namespace Boin.Review;


public class ReviewManager
{
    private readonly ReviewConfig config;
    private readonly List<IReviewUser> userReviews = new List<IReviewUser>();
    private readonly List<IReviewOrder> orderReviews = new List<IReviewOrder>();

    //public 

    public ReviewManager(string cnfFile)
    {
        var cnf = ReviewConfig.FromYamlFile(cnfFile);
        config = cnf;

        orderReviews.Add(new BankCardReview(cnf));

        userReviews.Add(new UserReview(cnf));
        userReviews.Add(new BankCardReview(cnf));
        userReviews.Add(new AmountReview(cnf));
        userReviews.Add(new WithdrawReview(cnf));
        userReviews.Add(new RechargeReview(cnf));
        userReviews.Add(new GameReview(cnf));
    }

    // 审核提现单
    public void Review(Order order)
    {
        order.ReviewResult = new List<ReviewResult>();
        foreach (var review in orderReviews)
        {
            var rs = review.Review(order);
            if (rs != ReviewResult.Empty)
            {
                order.ReviewResult.AddRange(rs);
                foreach (var r in rs)
                {
                    // 代码为负，强制中断
                    if (r.Code < 0)
                    {
                        return;
                    }
                }
            }
        }
    }

    // 审核用户
    public void Review(User user)
    {
        foreach (var review in userReviews)
        {
            var rs = review.Review(user);
            if (rs != ReviewResult.Empty)
            {
                user.Order.ReviewResult.AddRange(rs);
                foreach (var r in rs)
                {
                    // 代码为负，强制中断
                    if (r.Code < 0)
                    {
                        return;
                    }
                }
            }
        }
    }
}

