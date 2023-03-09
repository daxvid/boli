namespace Boin.Review;

using System.Collections.ObjectModel;
using Boin.Util;

// 银行卡审核
public class BankCardReview : IReviewInterface
{
    private readonly ReviewConfig config;

    public BankCardReview(ReviewConfig config)
    {
        this.config = config;
    }

    public ReadOnlyCollection<ReviewResult> Review(Order order)
    {
        List<ReviewResult> rs = new List<ReviewResult>();

        var re = order.Way switch
        {
            "银行卡" => ReviewBank(order),
            "数字钱包" => ReviewBobi(order),
            _ => new ReviewResult { Code = -103, Msg = "未知的通道:" + order.Way }
        };
        rs.Add(re);

        if ((!string.IsNullOrEmpty(order.Payee)) && (!IsChineseName(order.Payee)))
        {
            rs.Add(new ReviewResult { Code = 103, Msg = "姓名格式可疑" });
        }

        return new ReadOnlyCollection<ReviewResult>(rs);
    }

    private ReviewResult ReviewBank(Order order)
    {
        BankCardInfo? bankInfo = order.BankCardInfo;
        while (bankInfo == null)
        {
            Thread.Sleep(1000);
            bankInfo = order.BankCardInfo;
        }

        // 银行卡状态。值：ok，no。
        if (!bankInfo.stat.Equals("ok"))
        {
            return new ReviewResult { Code = 101, Msg = "卡不可用" + order.CardNo };
        }
        // 有效性，是否正确有效。值：true为是，false为否。
        else if (!bankInfo.validated)
        {
            return new ReviewResult { Code = 102, Msg = "卡号不正确"};
        }
        else
        {
            var name = bankInfo.CardTypeName;
            if (string.IsNullOrEmpty(name) || name == "未知卡")
            {
                return new ReviewResult { Code = 101, Msg = "卡需验证:" + order.CardNo };
            }
            else
            {
                return new ReviewResult { Code = 0, Msg = "@" + name + ":" + order.BankName };
            }
        }
    }

    private ReviewResult ReviewBobi(Order order)
    {
        // 波币地址格式检查
        if (!CheckBobiAddress(order.CardNo))
        {
            return new ReviewResult { Code = -101, Msg = "波币资讯有误请填写正确地址谢谢" };
        }

        // 波币没绑姓名的话不给予通过
        var b = order.Bind;
        if (b != null && b.CardNo == order.CardNo && (!string.IsNullOrEmpty(b.Payee)))
        {
            // 修复钱包姓名
            if (string.IsNullOrEmpty(order.Payee))
            {
                order.Payee = b.Payee;
            }

            if (b.Payee == order.Payee)
            {
                var maskName = Helper.MaskName(order.Payee);
                return new ReviewResult { Code = 0, Msg = "@钱包正确:" + maskName };
            }
        }

        return new ReviewResult { Code = -100, Msg = "波币没有绑定姓名，请绑定姓名 谢谢" };
    }

    public ReadOnlyCollection<ReviewResult> Review(User user)
    {
        return ReviewResult.Empty;
    }

    private bool CheckBobiAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 12)
        {
            return false;
        }

        if (address[0] != 'B' && address[0] != 'b')
        {
            return false;
        }

        return Helper.IsHexadecimal(address);
    }
    
    private bool IsChineseName(string name)
    {
        if (name.Length < 2)
        {
            return false;
        }
        foreach (var n in name)
        {
            // [\u4e00-\u9fcb]
            if (n != '.' && (n < '\u4E00' || n > '\u9FCB'))
            {
                return false;
            }
        }
        return true;
    }
}


