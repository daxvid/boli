using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
	// 充值记录
	public class RechargePage : PageBase
    {

        public RechargePage(ChromeDriver driver) : base(driver)
        {
        }

        private IWebElement getCurrentTable(string gameId, int page)
        {
            var p = "//div[text()='用户充值详情' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";
            var pagePath = ".//div/span[@class='marginRight' and contains(text(),'第" + page.ToString() + "页')]";
            var t = FindElementByXPath(p);
            var pageTag = FindElementByXPath(t, pagePath);
            //var id = ((WebElement)t).ToString();
            //Console.WriteLine("充值" + page.ToString() + ":" + id);
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

        public List<Recharge> Select(User user)
        {
            var table = getCurrentTable(user.GameId, 1);

            //// 日期区间
            //var timeRang = gameLog.FindElement(By.XPath(".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']"));
            //Helper.SetTimeRang(24, timeRang);
            //// 点击查询按钮;
            //var sub = gameLog.FindElement(By.XPath(".//button/span[text()='查询']"));
            //sub.Click();
            //Thread.Sleep(2000);

            // 在线充值次数/后台充值次数/等
            var items = FindElementsByXPath(table, ".//div[@class='countSty']/span[contains(text(),'在线充值次数：')]/../span");
            List<string> onlineRechargeList = new List<string>();
            foreach(var item in items)
            {
                onlineRechargeList.Add(item.Text);
            }
            return ReadRechargeLog(user, table);
        }

        // 读取日志数据 ivu-modal-content/ivu-modal-body
        private List<Recharge> ReadRechargeLog(User user, IWebElement table)
        {
            var headDic = ReadHeadDic(table);
            // table = ivu-modal-content
            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = FindElementByXPath(table,bodyPath);
            var bodys = new List<IWebElement>() { tbody };
            var tables = new List<IWebElement>() { table };
            var allLogs = ReadLogs(headDic, tbody, 1);

            for (var page = 2; page <= maxPage; page++)
            {
                // 检查是否有下一页
                var nextPage = FindElementByXPath(table, ".//button/span/i[@class='ivu-icon ivu-icon-ios-arrow-forward']/../..");
                var next = nextPage.Enabled;
                if (!next)
                {
                    break;
                }
                nextPage.Click();

                //TODO: 检查是否加载完成
                Thread.Sleep(500);

                table = getCurrentTable(user.GameId, page);
                //head = Head.ReadHead2(table);
                tbody = FindElementByXPath(table,bodyPath);
                bodys.Add(tbody);
                tables.Add(table);

                var logs = ReadLogs(headDic, tbody, page);
                allLogs.AddRange(logs);
            }
            // 关闭窗口
            Helper.SafeClose(driver, table);
            return allLogs;
        }

        // 读取每一项日志信息
        private List<Recharge> ReadLogs(Dictionary<string, string> dicHead, IWebElement tbody, int page)
        {
            // 点开所有的查询按钮
            var selBtns =  FindElementsByXPath(tbody,".//td/div/div/div/button/span[contains(text(),'查询')]/..");
            foreach (var sel in selBtns)
            {
                Helper.TryClick(wait, sel);
            }

            var allRows = FindElementsByXPath(tbody, ".//tr");
            var count = allRows.Count;
            var logs = new List<Recharge>(count);
            for (var i = 0; i < count; i++)
            {
                var row = allRows[i];
                var log = Recharge.Create(dicHead, row);
                if (log != null)
                {
                    logs.Add(log);
                }
            }
            return logs;
        }
    }
}
