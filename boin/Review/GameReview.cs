namespace Boin.Review;

using System.Collections.ObjectModel;

// 游戏审核
public class GameReview : IReviewUser
{
    private readonly ReviewConfig config;

    public GameReview(ReviewConfig config)
    {
        this.config = config;
    }

    public ReadOnlyCollection<ReviewResult> Review(User user)
    {
        List<ReviewResult> rs = new List<ReviewResult>();
        var order = user.Order;
        var isNew = user.IsNewUser();
        var ac = config.GetAmountConfig(order.Way, isNew);

        // 检查用户玩的游戏
        var pass = true;
        foreach (var g in user.GameInfo.GameLogs)
        {
            var game = ac.ExistsGame(g.GamePlatform, g.GameName);
            if (!string.IsNullOrEmpty(game))
            {
                rs.Add(new ReviewResult { Code = 301, Msg = "游戏:" + game });
                pass = false;
                break;
            }
        }

        if (pass)
        {
            rs.Add(new ReviewResult { Code = 0, Msg = "@游戏通过" });
        }

        return new ReadOnlyCollection<ReviewResult>(rs);
    }
}
