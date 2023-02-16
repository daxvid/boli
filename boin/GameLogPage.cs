using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    public class GameLogPage : PageBase
    {
        public GameLogPage(ChromeDriver driver):base(driver)
        {
        }

        private IWebElement getCurrentTable(string gameId, int page)
        {
            var p = "//div[text()='用户游戏日志' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";
            var pagePath = ".//div/span[@class='marginRight' and contains(text(),'第" + page.ToString() + "页')]";
            var t = FindElementByXPath(p);
            var pageTag = FindElementByXPath(t, pagePath);
            //var id = ((WebElement)t).ToString();
            //Console.WriteLine("游戏" + page.ToString() + ":" + id);
            Thread.Sleep(500);
            return t;
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

        public List<GameLog> Select(User user)
        {
            var table = getCurrentTable(user.GameId,1);
            // 用户游戏日志
            var timeRang = FindElementByXPath(table,".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']");
            Helper.SetTimeRang(24, timeRang);
            // 点击查询按钮;
            // /html/body/div[34]/div[2]/div/div/div[2]/div/div[1]/button[1]/span
            // /html/body/div[34]/div[2]/div/div/div[2]/div/div[1]/button[1]/span
            TryClickByXPath(table, ".//button/span[text()='查询']", 1000);

            // 读取用户查询出的输赢情况
            var spath = ".//div[@class='countSty']/span[@class='total_item' and contains(text(),'下注金额：')]/../span";
            var items = FindElementsByXPath(table, spath);
            user.TotalBet = readBetDecimal(items[0]);
            user.TotalWin = readBetDecimal(items[1]);
            user.TotalValidBet = readBetDecimal(items[2]);

            return ReadGameLog(user, table);
        }

        // 读取日志数据 ivu-modal-content/ivu-modal-body
        private List<GameLog> ReadGameLog(User user, IWebElement table)
        {
            var dicHead = ReadHeadDic(table);
            // table = ivu-modal-content
            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = FindElementByXPath(table, bodyPath);
            var allLogs = ReadLogs(dicHead, tbody, 1);

            maxPage = 1;
            for (var page = 2; page <= maxPage; page++)
            {
                // 检查是否有下一页
                var nextPage = FindElementByXPath(table,".//button/span/i[@class='ivu-icon ivu-icon-ios-arrow-forward']/../..");
                var next = nextPage.Enabled;
                if (!next)
                {
                    break;
                }
                nextPage.Click();

                //TODO: 检查是否加载完成
                Thread.Sleep(500);

                table = getCurrentTable(user.GameId, page);
                tbody = FindElementByXPath(table,bodyPath);
                var logs = ReadLogs(dicHead, tbody, page);
                allLogs.AddRange(logs);
            }

            // 关闭窗口
            Helper.SafeClose(driver, table);
            return allLogs;
        }

        // 读取每一项日志信息
        private List<GameLog> ReadLogs(Dictionary<string, string> dicHead, IWebElement tbody, int page)
        {
            var allRows = FindElementsByXPath(tbody, ".//tr");
            var count = allRows.Count;
            var logs = new List<GameLog>(count);
            for (var i = 0; i < count; i++)
            {
                var row = allRows[i];
                var log = GameLog.Create(dicHead, row);
                if (log != null)
                {
                    logs.Add(log);
                }
            }
            return logs;
        }
    }
}

