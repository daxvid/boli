namespace Boin.Review;

using System.Collections.ObjectModel;

public class UserReview : IReviewUser
{

    private readonly ReviewConfig config;

    public UserReview(ReviewConfig config)
    {
        this.config = config;
    }

    public ReadOnlyCollection<ReviewResult> Review(User user)
    {
        if (!string.IsNullOrEmpty(user.Remark))
        {
            foreach (var key in config.RemarkKeys)
            {
                if (user.Remark.Contains(key))
                {
                    return new ReadOnlyCollection<ReviewResult>(
                        new List<ReviewResult>() { new ReviewResult { Code = 501, Msg = "备注包含:" + key } });
                }
            }
        }

        return ReviewResult.Empty;
    }
}
