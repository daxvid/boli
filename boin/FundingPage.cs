namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Boin.Util;

// 资金概况
public class FundingPage : PopPage
{
    public FundingPage(ChromeDriver driver, AppConfig config, string gameId) : base(driver, config, gameId,
        "//div[text()='概况' and @class='ivu-modal-header-inner']/../.././/div")
    {
    }

    private IWebElement GetCurrentTable()
    {
        return MainTable;
    }

    public Funding Select()
    {
        const string tboxPath = ".//div[@class='tab_box ivu-row']";
        var table = GetCurrentTable();
        var tbox = FindElementByXPath(table, tboxPath);

        Funding fund = new Funding();

        // 余额
        var balTxt = FindElementByXPath(table, ".//div/div/div/div[starts-with(text(),'余额：')]");
        fund.Balance = Helper.ReadBetDecimal(balTxt);

        //今日(默认)
        FindAndClickByXPath(tbox, ".//div/div[text()='今日' and starts-with(@class,'tab_sty')]", 500);
        FillRechargeAndWithdraw(fund, fund.ToDay, Config.RechargeMaxDay, Config.WithdrawMaxDay);

        // 昨日
        FindAndClickByXPath(tbox, ".//div/div[text()='昨日' and starts-with(@class,'tab_sty')]", 500);
        FillRechargeAndWithdraw(fund, fund.Yesterday, 0, 0);

        // 近2月
        FindAndClickByXPath(tbox, ".//div/div[text()='近期（2个月）' and starts-with(@class,'tab_sty')]", 500);
        FillRechargeAndWithdraw(fund, fund.Nearly2Months, 0, 0);
        return fund;
    }

    private void FillRechargeAndWithdraw(Funding f, FundingDay fund, int rechargeMaxDay, int withdrawMaxDay)
    {
        const string tboxPath = ".//div[@class='tab_box ivu-row']";
        var table = GetCurrentTable();
        var tbox = FindElementByXPath(table, tboxPath);

        fund.ReadFrom(tbox);

        const int maxDay = 30;
        if (rechargeMaxDay > 0)
        {
            //读取充值明细
            f.RechargeLog = Helper.SafeExec(Driver, () =>
            {
                FindAndClickByXPath(tbox, ".//div/table/tr/td[text()='充值']/../td[2]/a", 1000);
                using var rg = new RechargePage(Driver, Config, GameId);
                var rechargeLogs = rg.Select(rechargeMaxDay);
                // 如果没有数据查则查询最近30天的数据
                if ((rechargeLogs.Count < 3) && rechargeMaxDay < maxDay)
                {
                    rechargeLogs = rg.Select(maxDay);
                }

                return rechargeLogs;
            }, 2000, 5);
        }

        if (withdrawMaxDay > 0)
        {
            table = GetCurrentTable();
            tbox = FindElementByXPath(table, tboxPath);

            // 读取提现明细
            f.WithdrawLog = Helper.SafeExec(Driver, () =>
            {
                FindAndClickByXPath(tbox, ".//div/table/tr/td[text()='提现']/../td[2]/a", 1000);
                using var wg = new WithdrawPage(Driver, Config, GameId);
                var withdrawLogs = wg.Select(withdrawMaxDay);
                // 如果没有数据查查询最近30天的数据
                if ((withdrawLogs.Count <= 1) && withdrawMaxDay < maxDay)
                {
                    withdrawLogs = wg.Select(maxDay);
                }

                return withdrawLogs;
            }, 2000, 5);
        }
    }
}

