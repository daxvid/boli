using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    // 提现记录
    public class WithdrawPage : PageBase
    {
        string gameId;
        string path;
        public WithdrawPage(ChromeDriver driver, string gameId) : base(driver)
        {
            this.gameId = gameId;
            this.path = "//div[text()='用户提现详情' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";

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

        private decimal readBetDecimal(IWebElement e)
        {
            var txt = e.Text;
            var index = txt.IndexOf('：');
            txt = txt.Substring(index + 1);
            decimal r;
            decimal.TryParse(txt, out r);
            return r;
        }

        public List<Withdraw> Select()
        {
            var table = getCurrentTable(1);

            return ReadWithdrawLog(table);
        }

        // 读取日志数据 ivu-modal-content/ivu-modal-body
        private List<Withdraw> ReadWithdrawLog(IWebElement table)
        {
            // table = ivu-modal-content
            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = FindElementByXPath(table, bodyPath);
            var allLogs = ReadLogs(tbody, 1);

            for (var page = 2; page <= maxPage; page++)
            {
                // 去到下一页
                if (!GoToNextPage(table))
                {
                    break;
                }

                table = getCurrentTable(page);
                tbody = FindElementByXPath(table, bodyPath);

                var logs = ReadLogs(tbody, page);
                allLogs.AddRange(logs);
            }
            return allLogs;
        }

        // 读取每一项日志信息
        private List<Withdraw> ReadLogs(IWebElement tbody, int page)
        {
            // 展开所有列表
            var expandList = FindElementsByXPath(tbody, ".//tr/td/div/div[@class='ivu-table-cell-expand']");
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
                        rowEx = allRows[i + 1].FindElement(By.XPath(".//td[@class='ivu-table-expanded-cell']"));
                        if (rowEx != null)
                        {
                            i += 1;
                        }
                }
                var withdraw = Withdraw.Create(row, rowEx);
                if (withdraw != null)
                {
                    allLogs.Add(withdraw);
                }
            }
            return allLogs;
        }
    }
}

