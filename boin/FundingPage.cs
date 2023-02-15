using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    // 资金概况
    public class FundingPage
    {
        ChromeDriver driver;
        WebDriverWait wait;

        public FundingPage(ChromeDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        private IWebElement getCurrentTable(string gameId)
        {
            var p = "//div[text()='概况' and @class='ivu-modal-header-inner']/../.././/div[text()='游戏ID：" + gameId + "']/../../../../..";
            var result = wait.Until(driver =>
            {
                try
                {
                    var t = driver.FindElement(By.XPath(p));
                    return t;
                }
                catch (NoSuchElementException) { }
                catch
                {
                    throw;
                }
                return null;
            });
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
            var tbox = table.FindElement(By.XPath(tboxPath));

            //// 点击刷新按钮;
            //var sub = table.FindElement(By.XPath(".//div[@class='ivu-modal-footer']/div/button/span[text()='刷新']"));
            //sub.Click();

            Funding fund = new Funding();

            // 余额
            var balTxt = table.FindElement(By.XPath(".//div/div/div/div[contains(text(),'余额：')]")).Text;
            balTxt = balTxt.Substring(balTxt.IndexOf('：') + 1);
            fund.Balance = decimal.Parse(balTxt);

            //今日(默认)
            tbox.FindElement(By.XPath(".//div/div[text()='今日']")).Click();
            Thread.Sleep(1000);
            FillRechargeAndWithdraw(user, fund.ToDay);

            // 昨日
            tbox.FindElement(By.XPath(".//div/div[text()='昨日']")).Click();
            Thread.Sleep(1000);
            FillRechargeAndWithdraw(user, fund.Yesterday);

            // 近2月
            tbox.FindElement(By.XPath(".//div/div[text()='近期（2个月）']")).Click();
            Thread.Sleep(1000);
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
            var tbox = table.FindElement(By.XPath(tboxPath));

            fund.ReadFrom(tbox);

            //读取充值明细
            var btnRecharge = tbox.FindElement(By.XPath(".//div/table/tr/td[text()='充值']/../td[2]/a"));
            Helper.TryClick(wait, btnRecharge);
            Thread.Sleep(500);

            var rg = new RechargePage(driver);
            var recharegeLogs = rg.Select(user);
            fund.RechargeLog = recharegeLogs;
            Thread.Sleep(500);

            table = getCurrentTable(user.GameId);
            tbox = table.FindElement(By.XPath(tboxPath));

            // 读取提现明细
            var btnWithdraw = tbox.FindElement(By.XPath(".//div/table/tr/td[text()='提现']/../td[2]/a"));
            Helper.TryClick(wait, btnWithdraw);
            Thread.Sleep(500);

            var wg = new WithdrawPage(driver);
            var withdrawLogs = wg.Select(user);
            fund.WithdrawLog = withdrawLogs;
            Thread.Sleep(500);
        }

    }
}

