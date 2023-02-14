using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace boin
{
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
        public string Offers { get; set; } = "";

        // 返利 rebate
        public decimal Rebate { get; set; }

        // 筹码兑钻石次数
        public int ChipToDiamondCount { get; set; }

        // 筹码兑钻石金额
        public decimal ChipToDiamondAmount { get; set; }

        // 充值记录
        public List<Recharge> RechargeLog { get; set; }

        // 提现记录
        public List<Withdraw> WithdrawLog { get; set; }

        public FundingDay()
        {
            RechargeLog = null;
            WithdrawLog = null;
        }

        public void ReadFrom(IWebElement tbox)
        {

            // 有效投注
            ValidBet = decimal.Parse(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='有效投注']/../td[2]")).Text);
            // 游戏损益
            GameProfitLoss = decimal.Parse(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='游戏损益']/../td[2]")).Text);
            var index = 0;
            // 充值
            var recharge = tbox.FindElement(By.XPath(".//div/table/tr/td[text()='充值']/../td[2]")).Text;
            index = recharge.IndexOf('\n');
            if (index > 0)
            {
                recharge = recharge.Substring(0, index);
            }
            index = recharge.IndexOf('/');
            RechargeCount = int.Parse(recharge.Substring(0, index - 1));
            RechargeAmount = decimal.Parse(recharge.Substring(index + 1));


            // 提现
            var withdraw = tbox.FindElement(By.XPath(".//div/table/tr/td[text()='提现']/../td[2]")).Text;
            index = withdraw.IndexOf('\n');
            if (index > 0)
            {
                withdraw = withdraw.Substring(0, index);
            }
            index = withdraw.IndexOf('/');
            WithdrawCount = int.Parse(withdraw.Substring(0, index - 1));
            WithdrawAmount = decimal.Parse(withdraw.Substring(index + 1));

            // 筹码兑钻石
            var chipToDiamond = tbox.FindElement(By.XPath(".//div/table/tr/td[text()='筹码兑钻石']/../td[2]")).Text;
            index = chipToDiamond.IndexOf('\n');
            if (index > 0)
            {
                chipToDiamond = chipToDiamond.Substring(0, index);
            }
            index = chipToDiamond.IndexOf('/');
            ChipToDiamondCount = int.Parse(chipToDiamond.Substring(0, index - 1));
            ChipToDiamondAmount = decimal.Parse(chipToDiamond.Substring(index + 1));


            // 提充客损
            ChargeCustomerLoss = decimal.Parse(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='提充客损']/../td[2]")).Text);
            // 优惠赠送
            Offers = tbox.FindElement(By.XPath(".//div/table/tr/td[text()='优惠赠送']/../td[2]")).Text;
            // 返利
            Rebate = decimal.Parse(tbox.FindElement(By.XPath(".//div/table/tr/td[text()='返利']/../td[2]")).Text);
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

        public Funding()
		{
		}
	}
}

