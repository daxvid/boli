using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    // 用户游戏日志
    public class GameLogPage : PageBase
    {
        string gameId;
        string path;
        public GameLogPage(ChromeDriver driver, string gameId) : base(driver)
        {
            this.gameId = gameId;
            this.path = "//div[text()='用户游戏日志' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";
        }

        public override bool Close()
        {
            // 关闭窗口
            var table = FindElementByXPath(path);
            return Helper.SafeClose(driver, table);
        }

        private IWebElement getCurrentTable(int page)
        {
            var pagePath = ".//div/span[@class='marginRight' and contains(text(),'第" + page.ToString() + "页')]";
            var t = FindElementByXPath(path);
            var pageTag = FindElementByXPath(t, pagePath);
            return t;
        }

        public GameInfo Select()
        {
            GameInfo info = new GameInfo();
            var table = getCurrentTable(1);
            // 用户游戏日志
            var timeRang = FindElementByXPath(table,".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']");
            Helper.SetTimeRang(24, timeRang);
            // 点击查询按钮;
            TryClickByXPath(table, ".//button/span[text()='查询']", 1000);

            // 读取用户查询出的输赢情况
            var spath = ".//div[@class='countSty']/span[@class='total_item' and contains(text(),'下注金额：')]/../span";
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

            maxPage = 1;
            for (var page = 2; page <= maxPage; page++)
            {
                // 去到下一页
                if (!GoToNextPage(table))
                {
                    break;
                }
                table = getCurrentTable(page);
                tbody = FindElementByXPath(table,bodyPath);
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
                if (log != null)
                {
                    logs.Add(log);
                }
            }
            return logs;
        }
    }
}

