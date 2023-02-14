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
            //var t = driver.FindElement(By.XPath(p));
            //return t;
            var result = wait.Until(drv =>
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
            var table = getCurrentTable(user.GameId);
            var tbox = table.FindElement(By.XPath(".//div[@class='tab_box ivu-row']"));

            //// 点击刷新按钮;
            //var sub = table.FindElement(By.XPath(".//div[@class='ivu-modal-footer']/div/button/span[text()='刷新']"));
            //sub.Click();

            Funding fund = new Funding();

            // 余额
            var balTxt = table.FindElement(By.XPath(".//div/div/div/div[contains(text(),'余额：')]")).Text;
            balTxt = balTxt.Substring(balTxt.IndexOf('：') + 1);
            fund.Balance = decimal.Parse(balTxt);

            // 今日(默认)
            // tbox.FindElement(By.XPath(".//div/div[text()='今日']")).Click();
            // FillRechargeAndWithdraw(user, tbox, fund.ToDay);
            fund.ToDay.ReadFrom(tbox);

            // 昨日
            tbox.FindElement(By.XPath(".//div/div[text()='昨日']")).Click();
            //FillRechargeAndWithdraw(user, tbox, fund.Yesterday);
            fund.Yesterday.ReadFrom(tbox);

            // 近2月
            tbox.FindElement(By.XPath(".//div/div[text()='近期（2个月）']")).Click();
            Thread.Sleep(1000);
            FillRechargeAndWithdraw(user, tbox, fund.Nearly2Months);

            user.Funding = fund;

            // 关闭窗口
            Table.SafeClose(driver, table);

            return fund;
        }

        private void FillRechargeAndWithdraw(User user, IWebElement tbox, FundingDay fund)
        {
            fund.ReadFrom(tbox);
            new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(drv =>
            {
                try
                {
                    // 充值明细按钮
                    tbox.FindElement(By.XPath(".//div/table/tr/td[text()='充值']/../td[2]/a")).Click();
                    return true;
                }
                catch (NoSuchElementException) { }
                catch (ElementClickInterceptedException) { }
                catch
                {
                    throw;
                }
                return false;
            });

            // 充值/提现明细按钮
            //var btnRecharge = tbox.FindElement(By.XPath(".//div/table/tr/td[text()='充值']/../td[2]/a"));
            //var btnWithdraw = tbox.FindElement(By.XPath(".//div/table/tr/td[text()='提现']/../td[2]/a"));

            //读取充值明细
            var rg = new RechargePage(driver);
            var recharegeLogs = rg.Select(user);
            fund.RechargeLog = recharegeLogs;
            Thread.Sleep(1000);

            wait.Until(drv =>
            {
                try
                {
                    // 提现明细按钮
                    tbox.FindElement(By.XPath(".//div/table/tr/td[text()='提现']/../td[2]/a")).Click();
                    return true;
                }
                catch (NoSuchElementException) { }
                catch (ElementClickInterceptedException) { }
                catch
                {
                    throw;
                }
                return false;
            });

            // 提现明细按钮
            // btnWithdraw.Click();

            var wg = new WithdrawPage(driver);
            var withdrawLogs = wg.Select(user);
            fund.WithdrawLog = withdrawLogs;
            Thread.Sleep(100);
        }

    }
}

