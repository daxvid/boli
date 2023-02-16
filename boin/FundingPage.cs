using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    // 资金概况
    public class FundingPage: PageBase
    {

        public FundingPage(ChromeDriver driver) : base(driver)
        {
        }

        private IWebElement getCurrentTable(string gameId)
        {
            var p = "//div[text()='概况' and @class='ivu-modal-header-inner']/../.././/div[text()='游戏ID：" + gameId + "']/../../../../..";
            var result = FindElementByXPath(p);
            return result;
        }

        private decimal readBetDecimal(IWebElement e)
        {
            var txt = e.Text;
            var index = txt.IndexOf('：');
            txt = txt.Substring(index + 1);
            decimal r;
            decimal.TryParse(txt, out r);
            return r;
        }

        public Funding Select(User user)
        {
            var tboxPath = ".//div[@class='tab_box ivu-row']";
            var table = getCurrentTable(user.GameId);
            var tbox = FindElementByXPath(table, tboxPath);

            //// 点击刷新按钮;
            //var sub = FindElementByXPath(table,".//div[@class='ivu-modal-footer']/div/button/span[text()='刷新']"));
            //sub.Click();

            Funding fund = new Funding();

            // 余额
            var balTxt = FindElementByXPath(table,".//div/div/div/div[contains(text(),'余额：')]").Text;
            balTxt = balTxt.Substring(balTxt.IndexOf('：') + 1);
            fund.Balance = decimal.Parse(balTxt);

            //今日(默认)
            TryClickByXPath(tbox, ".//div/div[text()='今日']");
            FillRechargeAndWithdraw(user, fund.ToDay);

            // 昨日
            TryClickByXPath(tbox, ".//div/div[text()='昨日']");
            FillRechargeAndWithdraw(user, fund.Yesterday);

            // 近2月
            TryClickByXPath(tbox, ".//div/div[text()='近期（2个月）']");
            FillRechargeAndWithdraw(user, fund.Nearly2Months);

            user.Funding = fund;

            // 关闭窗口
            Helper.SafeClose(driver, table);

            return fund;
        }

        private void FillRechargeAndWithdraw(User user, FundingDay fund)
        {
            var tboxPath = ".//div[@class='tab_box ivu-row']";
            var table = getCurrentTable(user.GameId);
            var tbox = FindElementByXPath(table,tboxPath);

            fund.ReadFrom(tbox);

            //读取充值明细
            TryClickByXPath(tbox, ".//div/table/tr/td[text()='充值']/../td[2]/a");
            var rg = new RechargePage(driver);
            var recharegeLogs = rg.Select(user);
            fund.RechargeLog = recharegeLogs;

            // 读取提现明细
            table = getCurrentTable(user.GameId);
            tbox = FindElementByXPath(table,tboxPath);
            TryClickByXPath(tbox, ".//div/table/tr/td[text()='提现']/../td[2]/a");
            var wg = new WithdrawPage(driver);
            var withdrawLogs = wg.Select(user);
            fund.WithdrawLog = withdrawLogs;
        }

    }
}

