using System;
using System.Collections.ObjectModel;

namespace boin.Review;

public class UserReview : IReviewUser
{
    static readonly ReadOnlyCollection<ReviewResult> empty =
        new ReadOnlyCollection<ReviewResult>(new List<ReviewResult>());

    ReviewConfig cnf;

    public UserReview(ReviewConfig cnf)
    {
        this.cnf = cnf;
    }

    public ReadOnlyCollection<ReviewResult> Review(User user)
    {
        if (string.IsNullOrEmpty(user.Remark))
        {
            return empty;
        }

        foreach (var key in cnf.RemarkKeys)
        {
            if (user.Remark.Contains(key))
            {
                return new ReadOnlyCollection<ReviewResult>(
                    new List<ReviewResult>(){new ReviewResult { Code = 501, Msg = "@备注包含:" + key }});
            }
        }

        return empty;
    }
}
