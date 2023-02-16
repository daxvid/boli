using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    // 充值记录
    public class WithdrawPage : PageBase
    {

        public WithdrawPage(ChromeDriver driver) : base(driver)
        {
        }


        private IWebElement getCurrentTable(string gameId, int page)
        {
            var p = "//div[text()='用户提现详情' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";
            var pagePath = ".//div/span[@class='marginRight' and contains(text(),'第" + page.ToString() + "页')]";
            var t = FindElementByXPath(p);
            var pageTag = FindElementByXPath(t, pagePath);
            //var id = ((WebElement)t).ToString();
            //Console.WriteLine("提现" + page.ToString() + ":" + id);
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

        public List<Withdraw> Select(User user)
        {
            var table = getCurrentTable(user.GameId, 1);

            return ReadWithdrawLog(user, table);
        }

        // 读取日志数据 ivu-modal-content/ivu-modal-body
        private List<Withdraw> ReadWithdrawLog(User user, IWebElement table)
        {
            var dicHead = ReadHeadDic(table);
            // table = ivu-modal-content
            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = FindElementByXPath(table, bodyPath);
            var allLogs = ReadLogs(dicHead, tbody, 1);

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
                tbody = FindElementByXPath(table, bodyPath);

                var logs = ReadLogs(dicHead, tbody, page);
                allLogs.AddRange(logs);
            }
            // 关闭窗口
            Helper.SafeClose(driver, table);
            return allLogs;
        }

        // 读取每一项日志信息
        private List<Withdraw> ReadLogs(Dictionary<string, string> dicHead, IWebElement tbody, int page)
        {
            // 展开所有列表
            var expandList = FindElementsByXPath (tbody, ".//tr/td/div/div[@class='ivu-table-cell-expand']");
            foreach (var exBtn in expandList)
            {
                Helper.TryClick(wait, exBtn);
            }

            var allRows = FindElementsByXPath(tbody, ".//tr");
            var allLogs = new List<Withdraw>();
            var count = allRows.Count;
            for (var i = 0; i < count; i++)
            {
                var row = allRows[i];
                IWebElement rowEx = null;
                var className = row.GetAttribute("class");
                if (!className.StartsWith("ivu-table-row"))
                {
                    continue;
                }
                if (i + 1 < count)
                {
                    try
                    {
                        rowEx = allRows[i + 1].FindElement(By.ClassName("ivu-table-expanded-cell"));
                        if (rowEx != null)
                        {
                            i += 1;
                        }
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                    }
                }
                var withdraw = Withdraw.Create(dicHead, row, rowEx);
                if (withdraw != null)
                {
                    allLogs.Add(withdraw);
                }
            }
            return allLogs;
        }
    }
}

