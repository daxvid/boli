using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;
using boin.Util;

namespace boin
{
    // 资金概况
    public class FundingPage : PageBase
    {
        string gameId;
        string path;
        public FundingPage(ChromeDriver driver, AppConfig cnf, string gameId) : base(driver, cnf)
        {
            this.gameId = gameId;
            this.path = "//div[text()='概况' and @class='ivu-modal-header-inner']/../.././/div[text()='游戏ID：" + gameId + "']/../../../../..";
        }

        public override bool Close()
        {
            // 关闭窗口
            var table = FindElementByXPath(path);
            return Helper.SafeClose(driver, table);
        }

        private IWebElement getCurrentTable()
        {
            var result = FindElementByXPath(path);
            return result;
        }

        public Funding Select()
        {
            var tboxPath = ".//div[@class='tab_box ivu-row']";
            var table = getCurrentTable();
            var tbox = FindElementByXPath(table, tboxPath);

            //// 点击刷新按钮;
            //var sub = FindElementByXPath(table,".//div[@class='ivu-modal-footer']/div/button/span[text()='刷新']"));
            //sub.Click();

            Funding fund = new Funding();

            // 余额
            var balTxt = FindElementByXPath(table, ".//div/div/div/div[contains(text(),'余额：')]");
            fund.Balance = Helper.ReadBetDecimal(balTxt);

            //今日(默认)
            TryClickByXPath(tbox, ".//div/div[text()='今日' and contains(@class,'tab_sty')]");
            FillRechargeAndWithdraw(fund, fund.ToDay, cnf.RechargeMaxDay, cnf.WithdrawMaxDay);

            // 昨日
            if (TryClickByXPath(tbox, ".//div/div[text()='昨日' and contains(@class,'tab_sty')]"))
            {
                FillRechargeAndWithdraw(fund, fund.Yesterday, 0, 0);
            }

            // 近2月
            if (TryClickByXPath(tbox, ".//div/div[text()='近期（2个月）' and contains(@class,'tab_sty')]"))
            {
                FillRechargeAndWithdraw(fund, fund.Nearly2Months, 0, 0);
            }
            return fund;
        }

        private void FillRechargeAndWithdraw(Funding f, FundingDay fund, int rechargeMaxDay, int withdrawMaxDay)
        {
            var tboxPath = ".//div[@class='tab_box ivu-row']";
            var table = getCurrentTable();
            var tbox = FindElementByXPath(table, tboxPath);
            fund.ReadFrom(tbox);

            if (rechargeMaxDay > 0)
            {
                //读取充值明细
                if (TryClickByXPath(tbox, ".//div/table/tr/td[text()='充值']/../td[2]/a"))
                {
                    using (var rg = new RechargePage(driver, cnf, gameId))
                    {
                        var recharegeLogs = rg.Select(rechargeMaxDay);
                        f.RechargeLog = recharegeLogs;
                    }
                }
            }
            if (withdrawMaxDay > 0)
            {
                // 读取提现明细
                table = getCurrentTable();
                tbox = FindElementByXPath(table, tboxPath);
                TryClickByXPath(tbox, ".//div/table/tr/td[text()='提现']/../td[2]/a");
                using (var wg = new WithdrawPage(driver, cnf, gameId))
                {
                    var withdrawLogs = wg.Select(withdrawMaxDay);
                    f.WithdrawLog = withdrawLogs;
                }
            }
        }

    }
}

