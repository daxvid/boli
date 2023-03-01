using System;
using System.Collections.ObjectModel;

namespace boin.Review;

// 游戏审核
public class GameReview : IReviewUser
{
    ReviewConfig cnf;

    public GameReview(ReviewConfig cnf)
    {
        this.cnf = cnf;
    }

    public ReadOnlyCollection<ReviewResult> Review(User user)
    {
        List<ReviewResult> rs = new List<ReviewResult>();
        var order = user.Order;
        bool isNew = user.IsNewUser();
        var ac = cnf.GetAmountConfig(order.Way, isNew);

        // 检查用户玩的游戏
        bool pass = true;
        if (user.GameInfo.GameLogs != null)
        {
            foreach (var g in user.GameInfo.GameLogs)
            {
                var game = ac.ExistsGame(g.GamePlatform, g.GameName);
                if (!string.IsNullOrEmpty(game))
                {
                    rs.Add(new ReviewResult { Code = 301, Msg = "@游戏:" + game });
                    pass = false;
                    break;
                }
            }
        }

        if (pass)
        {
            rs.Add(new ReviewResult { Msg = "@游戏通过" });
        }

        return new ReadOnlyCollection<ReviewResult>(rs);
    }
}
