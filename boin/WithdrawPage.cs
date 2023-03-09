namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Boin.Util;

// 提现记录
public class WithdrawPage : PopPage
{
    public WithdrawPage(ChromeDriver driver, AppConfig config, string gameId) : base(driver, config, gameId,
        "//div[text()='用户提现详情' and @class='ivu-modal-header-inner']/../.././/span")
    {
        this.MaxPage = config.WithdrawMaxPage;
    }

    public List<Withdraw> Select(int maxDay)
    {
        var table = GetCurrentTable(1);
        if (maxDay > 0)
        {
            var dayRang = FindElementByXPath(table,
                ".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']");
            Helper.SetDayRang(dayRang, maxDay);
            // 点击查询按钮;
            FindAndClickByXPath(table, ".//button/span[text()='查询']", 1000);
            table = GetCurrentTable(1);
        }

        return ReadWithdrawLog(table);
    }

    // 读取日志数据 ivu-modal-content/ivu-modal-body
    private List<Withdraw> ReadWithdrawLog(IWebElement table)
    {
        // table = ivu-modal-content
        var bodyPath = ".//tbody[@class='ivu-table-tbody']";
        var tbody = FindElementByXPath(table, bodyPath);
        var allLogs = ReadWithdraws(tbody, 1);

        for (var page = 2; page <= MaxPage; page++)
        {
            // 去到下一页
            if (!GoToNextPage(table))
            {
                break;
            }

            table = GetCurrentTable(page);
            tbody = FindElementByXPath(table, bodyPath);

            var logs = ReadWithdraws(tbody, page);
            allLogs.AddRange(logs);
        }

        return allLogs;
    }

    // 读取每一项日志信息
    private List<Withdraw> ReadWithdraws(IWebElement tbody, int page)
    {
        var allRows = FindElementsByXPath(tbody, ".//tr");
        // 展开所有列表
        var expandPath = "./td[1]/div/div[@class='ivu-table-cell-expand']";
        foreach (var row in allRows)
        {
            FindAndClickByXPath(row, expandPath, 0);
        }

        Thread.Sleep(200);

        var allLogs = new List<Withdraw>();
        var count = allRows.Count;
        for (var i = 0; i < count; i++)
        {
            var row = allRows[i];
            var rowEx = FindElementByXPath(row,
                "./following-sibling::tr[1]/td[@class='ivu-table-expanded-cell']/..");
            if (rowEx == null)
            {
                throw new NoSuchElementException("not find rowEx");
            }

            var withdraw = Withdraw.Create(row, rowEx);
            allLogs.Add(withdraw);
        }

        return allLogs;
    }
}

