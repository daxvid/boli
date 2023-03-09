namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Boin.Util;

// 用户游戏日志
public class GameLogPage : PopPage
{
    public GameLogPage(ChromeDriver driver, AppConfig config, string gameId) : base(driver, config, gameId,
        "//div[text()='用户游戏日志' and @class='ivu-modal-header-inner']/../.././/span")
    {
        this.MaxPage = config.GameLogMaxPage;
    }

    // 查询用户游戏日志
    public GameInfo Select(int hour)
    {
        GameInfo info = new GameInfo();
        var table = GetCurrentTable(1);
        if (hour > 0)
        {
            var timeRang = FindElementByXPath(table,
                ".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']");
            Helper.SetTimeRang(timeRang, hour);
            // 点击查询按钮;
            FindAndClickByXPath(table, ".//button/span[text()='查询']", 1000);
        }

        // 读取用户查询出的输赢情况
        var spath =
            ".//div[@class='countSty']/span[@class='total_item' and starts-with(text(),'下注金额：')]/../span";
        var items = FindElementsByXPath(table, spath);

        info.TotalBet = Helper.ReadBetDecimal(items[0]);
        info.TotalWin = Helper.ReadBetDecimal(items[1]);
        info.TotalValidBet = Helper.ReadBetDecimal(items[2]);
        info.GameLogs = ReadGameLog(table);
        return info;
    }

    // 读取日志数据 ivu-modal-content/ivu-modal-body
    private List<GameLog> ReadGameLog(IWebElement table)
    {
        // table = ivu-modal-content
        var bodyPath = ".//tbody[@class='ivu-table-tbody']";
        var tbody = FindElementByXPath(table, bodyPath);
        var allLogs = ReadLogs(tbody, 1);

        MaxPage = 1;
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
    private List<GameLog> ReadLogs(IWebElement tbody, int page)
    {
        var allRows = FindElementsByXPath(tbody, ".//tr");
        var count = allRows.Count;
        var logs = new List<GameLog>(count);
        for (var i = 0; i < count; i++)
        {
            var row = allRows[i];
            var log = GameLog.Create(row);
            logs.Add(log);
        }

        return logs;
    }
}

