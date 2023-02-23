using System;
using System.Collections.ObjectModel;

namespace boin.Review
{
    // 金额审核
    public class AmountReview : IReviewUser
    {
        static readonly ReadOnlyCollection<ReviewResult> empty = new ReadOnlyCollection<ReviewResult>(new List<ReviewResult>());
        ReviewConfig cnf;
        public AmountReview(ReviewConfig cnf)
        {
            this.cnf = cnf;
        }

        //public ReadOnlyCollection<ReviewResult> Review(Order order)
        //{
        //    List<ReviewResult> rs = new List<ReviewResult>();
        //    var ac = cnf.GetAmountConfig(order.Way, false);
        //    var r1 = checkOnceMax(order.Way, ac.OnceMax, order.Amount);
        //    if (r1 != null)
        //    {
        //        rs.Add(r1);
        //    }
        //    return new ReadOnlyCollection<ReviewResult>(rs);
        //}


        public ReadOnlyCollection<ReviewResult> Review(User user)
        {
            List<ReviewResult> rs = new List<ReviewResult>();
            var order = user.Order;
            bool isNew = user.IsNewUser();
            var ac = cnf.GetAmountConfig(order.Way, isNew);

            // 检查单笔额度
            var r1 = checkOnceMax(order.Way, ac.OnceMax, order.Amount);
            if (r1 != null)
            {
                rs.Add(r1);
            }

            // 检查当日总额度
            var withdraw = user.Funding.ToDay.WithdrawAmount + order.Amount;
            var r2 = checkDayMax(order.Way, ac.DayMax, withdraw);
            if (r2 != null)
            {
                rs.Add(r2);
            }
            
            // 检查玩游戏

            // 检查当日总充值
            var dayRecharge = user.Funding.ToDay.RechargeAmount;
            var r3 = checkDayRecharge(order.Way, ac.DayRecharge, dayRecharge);
            if (r3 != null)
            {
                rs.Add(r3);
            }
            
            // 检查如果玩了某种游戏，当日提现总额限制
            var dayWithdraw = user.Funding.TotalDayWithdraw(order.Way, order.OrderID) + order.Amount;
            var r4 = checkDayGames(user, ac.DayMaxGames, dayWithdraw);
            if (r4 != null)
            {
                rs.Add(r4);
            }
            
            return new ReadOnlyCollection<ReviewResult>(rs);
        }

        // 检查每笔提现金额
        ReviewResult checkOnceMax(string way, decimal max, decimal amount)
        {
            if (way == "银行卡")
            {
                // 检查老用户单笔银行卡限制
                if (amount > max)
                {
                    return (new ReviewResult { Code = -201, Msg = "@卡单笔限制" + max + ":" + amount });
                }

            }
            else if (way == "数字钱包")
            {
                // 检查老用户单笔波币限制
                if (amount > max)
                {
                    return (new ReviewResult { Code = -202, Msg = "@币单笔限制" + max + ":" + amount });
                }
            }
            else
            {
                return (new ReviewResult { Code = -203, Msg = "@未知的通道:" + way });
            }
            return (new ReviewResult { Msg = "@单笔通过:" + amount });
        }

        // 检查当日提现金额
        ReviewResult checkDayMax(string way, decimal max, decimal amount)
        {
            if (way == "银行卡")
            {
                // 检查老用户单笔银行卡限制
                if (amount > max)
                {
                    return (new ReviewResult { Code = -201, Msg = "@卡当日限制" + max + ":" + amount });
                }

            }
            else if (way == "数字钱包")
            {
                // 检查老用户单笔波币限制
                if (amount > max)
                {
                    return (new ReviewResult { Code = -202, Msg = "@币当日限制" + max + ":" + amount });
                }
            }
            else
            {
                return (new ReviewResult { Code = -203, Msg = "@未知的通道:" + way });
            }
            return (new ReviewResult { Msg = "@当日通过:" + amount });
        }

        // 检查当日充值金额
        ReviewResult checkDayRecharge(string way, decimal max, decimal amount)
        {
            if (way == "银行卡")
            {
                // 检查老用户单笔银行卡限制
                if (amount > max)
                {
                    return (new ReviewResult { Code = 401, Msg = "@卡提日充值限制" + max + ":" + amount });
                }

            }
            else if (way == "数字钱包")
            {
                // 检查老用户单笔波币限制
                if (amount > max)
                {
                    return (new ReviewResult { Code = 402, Msg = "@币提日充值限制" + max + ":" + amount });
                }
            }
            else
            {
                return (new ReviewResult { Code = -203, Msg = "@未知的通道:" + way });
            }
            return (new ReviewResult { Msg = "@日充通过:" + amount });
        }

        // 检查如果玩了某种游戏，当日提现总额限制
        ReviewResult checkDayGames(User user, Dictionary<string, decimal> dayMaxGames, decimal amount)
        {
            if ((dayMaxGames != null) && (dayMaxGames.Count > 0))
            {
                foreach (var kv in dayMaxGames)
                {
                    if (user.GameInfo.PlayGame(string.Empty,kv.Key))
                    {
                        // 游戏日提限制
                        if (amount > kv.Value)
                        {
                            var gameName = kv.Key;//.Replace("all", "");
                            var msg = "@游戏[" + gameName + "]日提限制:" + kv.Value + "<" + amount;
                            return (new ReviewResult { Code = -401, Msg = msg});
                        }
                    }
                }
            }
            return null;
        }
    }
}


