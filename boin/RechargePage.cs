namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Boin.Util;

// 充值记录
public class RechargePage : PopPage
{
    public RechargePage(ChromeDriver driver, AppConfig config, string gameId) : base(driver, config, gameId,
        "//div[text()='用户充值详情' and @class='ivu-modal-header-inner']/../.././/span")
    {
        this.MaxPage = config.RechargeMaxPage;
    }

    public List<Recharge> Select(int maxDay)
    {
        var table = GetCurrentTable(1);
        if (maxDay > 0)
        {
            var dayRang = FindElementByXPath(table,
                ".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']");
            Helper.SetDayRang(dayRang, maxDay);
            // 点击查询按钮;
            FindAndClickByXPath(table, ".//div/button[1]/span[text()='查询']", 1000);
            table = GetCurrentTable(1);
        }

        // 在线充值次数/后台充值次数/等
        var items = FindElementsByXPath(table,
            ".//div[@class='countSty']/span[starts-with(text(),'在线充值次数：')]/../span");
        var onlineRechargeList = new List<string>();
        foreach (var item in items)
        {
            var value = Helper.ReadString(item);
            onlineRechargeList.Add(value);
        }

        return ReadRechargeLog(table);
    }

    // 读取日志数据 ivu-modal-content/ivu-modal-body
    private List<Recharge> ReadRechargeLog(IWebElement table)
    {
        // table = ivu-modal-content
        const string bodyPath = ".//tbody[@class='ivu-table-tbody']";
        var tbody = FindElementByXPath(table, bodyPath);
        var allLogs = ReadLogs(tbody, 1);

        for (var page = 2; page <= MaxPage; page++)
        {
            // 去到下一页
            if (!GoToNextPage(table))
            {
                break;
            }

            table = GetCurrentTable(page);
            tbody = FindElementByXPath(table, bodyPath);

            var logs = ReadLogs(tbody, page);
            allLogs.AddRange(logs);
        }

        return allLogs;
    }

    // 读取每一项日志信息
    private List<Recharge> ReadLogs(IWebElement tbody, int page)
    {
        // 点开所有的查询按钮
        var selBtns = FindElementsByXPath(tbody, ".//td/div/div/div/button/span[contains(text(),'查询')]/..");
        foreach (var sel in selBtns)
        {
            if (sel.Enabled && sel.Displayed)
            {
                SafeClick(sel, 5);
            }
        }

        Thread.Sleep(1000);

        var allRows = FindElementsByXPath(tbody, ".//tr");
        var count = allRows.Count;
        var logs = new List<Recharge>(count);
        for (var i = 0; i < count; i++)
        {
            var row = allRows[i];
            var log = Recharge.Create(row);
            logs.Add(log);
        }

        return logs;
    }
}
