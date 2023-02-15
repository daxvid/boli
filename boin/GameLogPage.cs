using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    public class GameLogPage
    {
        ChromeDriver driver;
        WebDriverWait wait;

        public GameLogPage(ChromeDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        private IWebElement getCurrentTable(string gameId, int page)
        {
            var p = "//div[text()='用户游戏日志' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";
            var pagePath = ".//div/span[@class='marginRight' and contains(text(),'第" + page.ToString() + "页')]";
            var result = wait.Until(driver =>
            {
                try
                {
                    var t = driver.FindElement(By.XPath(p));
                    var pageTag = t.FindElement(By.XPath(pagePath));
                    var id = ((WebElement)t).ToString();
                    Console.WriteLine("游戏" + page.ToString() + ":" + id);
                    Thread.Sleep(500);
                    return t;
                }
                catch (NoSuchElementException) { }
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

        public List<GameLog> Select(User user)
        {
            var table = getCurrentTable(user.GameId,1);
            // 用户游戏日志
            var timeRang = table.FindElement(By.XPath(".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']"));
            Helper.SetTimeRang(24, timeRang);
            // 点击查询按钮;
            var sub = table.FindElement(By.XPath(".//button/span[text()='查询']"));
            sub.Click();
            Thread.Sleep(1000);

            // 读取用户查询出的输赢情况
            var items = table.FindElements(By.XPath(".//div[@class='countSty']/span[@class='total_item' and contains(text(),'下注金额：')]/../span"));
            user.TotalBet = readBetDecimal(items[0]);
            user.TotalWin = readBetDecimal(items[1]);
            user.TotalValidBet = readBetDecimal(items[2]);

            return ReadGameLog(user, table);
        }

        // 读取日志数据 ivu-modal-content/ivu-modal-body
        private List<GameLog> ReadGameLog(User user, IWebElement table)
        {
            var head = Head.ReadHead2(table);
            // table = ivu-modal-content
            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = table.FindElement(By.XPath(bodyPath));
            var allLogs = ReadLogs(head, tbody, 1);

            const int maxPage = 5;
            for (var page = 2; page <= maxPage; page++)
            {
                // 检查是否有下一页
                var nextPage = table.FindElement(By.XPath(".//button/span/i[@class='ivu-icon ivu-icon-ios-arrow-forward']/../.."));
                var next = nextPage.Enabled;
                if (!next)
                {
                    break;
                }
                nextPage.Click();

                //TODO: 检查是否加载完成
                Thread.Sleep(500);

                table = getCurrentTable(user.GameId, page);
                tbody = table.FindElement(By.XPath(bodyPath));
                var logs = ReadLogs(head, tbody, page);
                allLogs.AddRange(logs);
            }

            // 关闭窗口
            Helper.SafeClose(driver, table);
            return allLogs;
        }

        // 读取每一项日志信息
        private List<GameLog> ReadLogs(List<Head> head, IWebElement tbody, int page)
        {
            var dicHead = new Dictionary<string, string>(head.Count * 2);
            foreach (var item in head)
            {
                dicHead.Add(item.Name, item.Tag);
            }

            var allRows = tbody.FindElements(By.XPath(".//tr"));
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

