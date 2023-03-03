using System;
using boin.Util;
using OpenQA.Selenium;

namespace boin;

public class FundingDay
{
    // 有效投注
    public decimal ValidBet { get; set; }

    // 游戏损益
    public decimal GameProfitLoss { get; set; }

    // 充值次数
    public int RechargeCount { get; set; }

    // 充值金额
    public decimal RechargeAmount { get; set; }

    // 提现次数
    public int WithdrawCount { get; set; }

    // 提现金额
    public decimal WithdrawAmount { get; set; }

    // 提充客损
    public decimal ChargeCustomerLoss { get; set; }

    // 优惠赠送
    public string Offers { get; set; } = string.Empty;

    // 返利 rebate
    public decimal Rebate { get; set; }

    // 筹码兑钻石次数
    public int ChipToDiamondCount { get; set; }

    // 筹码兑钻石金额
    public decimal ChipToDiamondAmount { get; set; }

    public FundingDay()
    {
    }

    public void ReadFrom(IWebElement tbox)
    {
        // 有效投注
        ValidBet = Helper.ReadDecimal(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='有效投注']/../td[2]")));
        // 游戏损益
        GameProfitLoss = Helper.ReadDecimal(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='游戏损益']/../td[2]")));
        var index = 0;
        // 充值
        var recharge = Helper.ReadString(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='充值']/../td[2]")));
        index = recharge.IndexOf('\n');
        if (index > 0)
        {
            recharge = recharge.Substring(0, index);
        }

        index = recharge.IndexOf('/');
        RechargeCount = int.Parse(recharge.Substring(0, index - 1));
        RechargeAmount = decimal.Parse(recharge.Substring(index + 1));


        // 提现
        var withdraw = Helper.ReadString(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='提现']/../td[2]")));
        index = withdraw.IndexOf('\n');
        if (index > 0)
        {
            withdraw = withdraw.Substring(0, index);
        }

        index = withdraw.IndexOf('/');
        WithdrawCount = int.Parse(withdraw.Substring(0, index - 1));
        WithdrawAmount = decimal.Parse(withdraw.Substring(index + 1));

        // 筹码兑钻石
        var chipToDiamond =
            Helper.ReadString(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='筹码兑钻石']/../td[2]")));
        index = chipToDiamond.IndexOf('\n');
        if (index > 0)
        {
            chipToDiamond = chipToDiamond.Substring(0, index);
        }

        index = chipToDiamond.IndexOf('/');
        ChipToDiamondCount = int.Parse(chipToDiamond.Substring(0, index - 1));
        ChipToDiamondAmount = decimal.Parse(chipToDiamond.Substring(index + 1));


        // 提充客损
        ChargeCustomerLoss =
            Helper.ReadDecimal(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='提充客损']/../td[2]")));
        // 优惠赠送
        Offers = Helper.ReadString(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='优惠赠送']/../td[2]")));
        // 返利
        Rebate = Helper.ReadDecimal(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='返利']/../td[2]")));
    }
}

// 资金情况
public class Funding
{
    // 余额
    public decimal Balance { get; set; }
    public FundingDay ToDay { get; set; } = new FundingDay();
    public FundingDay Yesterday { get; set; } = new FundingDay();
    public FundingDay Nearly2Months { get; set; } = new FundingDay();

    // 充值记录
    public List<Recharge> RechargeLog { get; set; }

    // 提现记录
    public List<Withdraw> WithdrawLog { get; set; }

    public Funding()
    {
    }


    public bool IsSyncName
    {
        get
        {
            if (RechargeLog != null)
            {
                foreach (var r in RechargeLog)
                {
                    if (r.IsSyncName == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }


    // 成功提现总额
    public decimal SuccessAmount(string card, string name)
    {
        decimal total = 0;
        foreach (var w in WithdrawLog)
        {
            if (w.ActualAmount > 0 && w.CardNo == card && w.Transfer == "成功")
            {
                total += w.Amount;
            }
        }

        return total;
    }

    // 是否第一次提现
    public bool FirstWithdraw(string card, string name)
    {
        foreach (var w in WithdrawLog)
        {
            if (w.ActualAmount > 0 && w.CardNo == card && w.Transfer == "成功")
            {
                return false;
            }
        }

        return true;
    }

    // 最近提现的名字
    public string NearBankName(string orderId, string way)
    {
        foreach (var w in WithdrawLog)
        {
            if (w.OrderId != orderId)
            {
                if (w.Way == way && w.Transfer == "成功" && (!string.IsNullOrEmpty(w.Payee)) )
                {
                    return w.Payee;
                }
            }
        }

        return string.Empty;
    }

    // 最近的单是否成功
    public bool NearPass(string orderId)
    {
        foreach (var w in WithdrawLog)
        {
            if (w.OrderId != orderId)
            {
                return w.Pass();
            }
        }
        return true;
    }

    // 最近多少笔波币提现
    public int NearBobiCount(string orderId, int maxCount)
    {
        if (WithdrawLog == null)
        {
            return 0;
        }

        int checkCount = 0;
        int bobiCount = 0;
        for (var i = 0; (i < WithdrawLog.Count && checkCount < maxCount); i++)
        {
            var w = WithdrawLog[i];
            if (w.OrderId != orderId)
            {
                checkCount++;
                if (w.Way == "数字钱包" && w.Review == "已通过" && w.Transfer != "失败")
                {
                    bobiCount++;
                }
            }
        }

        return bobiCount;
    }

    // 用户名总的充值金额
    public decimal TotalRechargeAmount(string name)
    {
        decimal total = 0;
        foreach (var r in RechargeLog)
        {
            if (string.IsNullOrEmpty(r.Depositor) || r.Depositor == name)
            {
                total += r.RechargeAmount;
            }
        }

        return total;
    }

    // 其它用户名充值金额
    public decimal OtherRechargeAmount(string name)
    {
        decimal total = 0;
        foreach (var r in RechargeLog)
        {
            if ((!string.IsNullOrEmpty(r.Depositor)) && r.Depositor != name)
            {
                total += r.RechargeAmount;
            }
        }

        return total;
    }

    // 当日提现金额
    public decimal TotalDayWithdraw(string way, string orderId)
    {
        decimal total = 0;
        var now = DateTime.Now;
        foreach (var w in WithdrawLog)
        {
            if ((w.OrderId != orderId) && w.Way == way && w.Review == "已通过" && w.Transfer != "失败")
            {
                if (w.Created.Year == now.Year && w.Created.Month == now.Month && w.Created.Day == now.Day)
                {
                    total += w.Amount;
                }
            }
        }

        return total;
    }

    // 最后一笔成功提现的时间
    public bool LastSuccessWithdrawTime(string orderId, out DateTime time)
    {
        foreach (var w in WithdrawLog)
        {
            if (w.OrderId != orderId)
            {
                if (w.Pass())
                {
                    time = w.Created;
                    return true;
                }
            }
        }

        time = DateTime.Now;
        return false;
    }

    // 第一个其它充值用户名
    public string FirstOtherRechargeName(string name)
    {
        foreach (var r in RechargeLog)
        {
            if ((!string.IsNullOrEmpty(r.Depositor)) && (r.Depositor != name))
            {
                return r.Depositor;
            }
        }

        return string.Empty;
    }

    // 所有其它充值用户充值
    public Dictionary<string, decimal> AllOtherRecharge(string name)
    {
        Dictionary<string, decimal> names = new Dictionary<string, decimal>();
        foreach (var r in RechargeLog)
        {
            if ((!string.IsNullOrEmpty(r.Depositor)) && (r.Depositor != name))
            {
                decimal t;
                if (names.TryGetValue(r.Depositor, out t))
                {
                    names[r.Depositor] = t + r.RechargeAmount;
                }
                else
                {
                    names.Add(r.Depositor, r.RechargeAmount);
                }
            }
        }

        return names;
    }

    // 统计某个渠道从指定时间开始的总充值
    public Tuple<decimal, int> TotalRechargeByChannel(string chan, DateTime startTime)
    {
        decimal total = 0;
        int count = 0;
        foreach (var r in RechargeLog)
        {
            if (r.RechargeChannel == chan)
            {
                if (r.Created > startTime)
                {
                    total += r.RechargeAmount;
                    count++;
                }
                else
                {
                    break;
                }
            }
        }

        return new Tuple<decimal, int>(total, count);
    }
}

