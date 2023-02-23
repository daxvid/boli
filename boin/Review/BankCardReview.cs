﻿using System;
using System.Collections.ObjectModel;

using boin.Util;

namespace boin.Review
{
    // 银行卡审核
    public class BankCardReview : IReviewInterface
    {
        ReviewConfig cnf;
        public BankCardReview(ReviewConfig cnf)
        {
            this.cnf = cnf;
        }

        public ReadOnlyCollection<ReviewResult> Review(Order order)
        {
            List<ReviewResult> rs = new List<ReviewResult>();
            if (order.Way == "银行卡")
            {
                BankCardInfo bankInfo = order.BankCardInfo;
                while (true)
                {
                    if (bankInfo == null)
                    {
                        Thread.Sleep(1000);
                        bankInfo = order.BankCardInfo;
                        continue;
                    }
                    // 银行卡状态。值：ok，no。
                    if (!bankInfo.stat.Equals("ok"))
                    {
                        rs.Add(new ReviewResult { Code = -101, Msg = "@卡不可用" + order.CardNo });
                    }
                    // 有效性，是否正确有效。值：true为是，false为否。
                    else if (!bankInfo.validated)
                    {
                        rs.Add(new ReviewResult { Code = -102, Msg = "@卡不正确:" + order.CardNo });
                    }
                    else
                    {
                        var name = bankInfo.CardTypeName;
                        if(string.IsNullOrEmpty(name) || name=="未知卡")
                        {
                            rs.Add(new ReviewResult { Code = 101, Msg = "@卡需验证:" + order.CardNo });
                        }
                        else
                        {
                            rs.Add(new ReviewResult  { Code = 0, Msg = "@" + name + ":" + order.BankName });
                        }
                    }
                    break;
                }
            }
            else if (order.Way == "数字钱包")
            {
                // 波音没绑姓名的话不给予通过
                bool passName = false;
                var b = order.Bind;
                if (b != null)
                {
                    if (b.CardNo == order.CardNo && (!string.IsNullOrEmpty(b.Payee)))
                    {
                        if (b.Payee == order.Payee || order.Payee == string.Empty)
                        {
                            rs.Add(new ReviewResult { Code = 0, Msg = "@钱包正确:" + b.Payee });
                            passName = true;
                        }
                    }
                }
                if (!passName)
                {
                    rs.Add(new ReviewResult { Code = 0, Msg = "@钱包未认证:" + order.CardNo });
                }
            }
            else
            {
                rs.Add(new ReviewResult { Code = -103, Msg = "@未知的通道:" + order.Way });
            }
            return new ReadOnlyCollection<ReviewResult>(rs);
        }

        public ReadOnlyCollection<ReviewResult> Review(User user)
        {
            return ReviewResult.Empty;
        }
    }
}


