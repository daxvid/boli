using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Review;

namespace boin
{
    public class OrderPage : PageBase
    {
        public OrderPage(ChromeDriver driver, AppConfig cnf) : base(driver,cnf)
        {
        }

        public override bool Open()
        {
            GoToPage(4, "提现管理");
            return SetItem();
        }

        private bool SetItem()
        {
            // 全部，待审核
            // //*[@id="Cash"]/div[1]/div[9]/div/div[1]/div/i
            bool b1 = TryClickByXPath("//div[@id='Cash']/div[1]/div[9]/div/div[1]/div/i");
            bool s1 = TryClickByXPath("//div[@id='Cash']/div[1]/div[9]/div/div[2]/ul[2]/li[2]");

            // 选择200条记录
            // //*[@id="Cash"]/div[4]/div/div/div[1]/div/i
            bool b2 = true;
            bool s2 = true;
            //b2 = TryClickByXPath("//div[@id='Cash']/div[4]/div/div/div[1]/div/i");
            //s2 = TryClickByXPath("//div[@id='Cash']/div[4]/div/div/div[2]/ul[2]/li[6]");
            return b1 && s1 && b2 && s2;
        }

        // 查询订单
        public bool Select(int hour)
        {
            // 设置查询时间，12小时以内的订单
            var timeRang = FindElementByXPath("//div[@id='Cash']/div/div[12]/div/div/div/input");
            Helper.SetTimeRang(hour, timeRang);
            // 点击查询
            // //*[@id="Cash"]/div[1]/div[13]/button[1]/span
            var r = TryClickByXPath("//div[@id='Cash']/div[1]/div[13]/button[1]/span[text()='查询']", 5000);
            return r;
        }

        public List<Order> ReadTable()
        {
            var tablePath = "//*[@id=\"Cash\"]/div[2]/div[1]";
            var table = FindElementByXPath(tablePath);

            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = FindElementByXPath(table, bodyPath);
            // var dicHead = ReadHeadDic(table);

            // 展开所有列表
            // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[2]/td[1]/div/div/i
            var expandPath = "./tr/td[1]/div/div[@class='ivu-table-cell-expand']/i[@class='ivu-icon ivu-icon-ios-arrow-forward']";
            var expandItems = FindElementsByXPath(tbody, expandPath);
            for (var i = 0; i < expandItems.Count; i++)
            {
                SafeClick(expandItems[i], 10);
            }

            var orders = ReadOrders(tbody);
            return orders;
        }

        // 读取每一项的信息
        private List<Order> ReadOrders(IWebElement tbody)
        {
            var allRows = FindElementsByXPath(tbody, (".//tr"));
            var count = allRows.Count;
            SendMsg("order count:" + count.ToString());
            var orders = new List<Order>(count);
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
                var order = Order.Create(row, rowEx);
                if (order != null)
                {
                    orders.Add(order);
                }
            }
            return orders;
        }
    }
}

