using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    public class OrderPage
    {

        ChromeDriver driver;
        WebDriverWait wait;

        public OrderPage(ChromeDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        public bool Open()
        {
            BoinClient.GoToPage(driver, 4, "提现管理");
            return SetItem();
        }

        private bool SetItem()
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {

                    // 全部，待审核
                    var cash = driver.FindElement(By.XPath("//div[@id='Cash']/div/div[9]/div/div/div/i"));
                    cash.Click();
                    Thread.Sleep(1000);
                    //var waitItem = driver.FindElement(By.CssSelector(".ivu-select-visible .ivu-select-item:nth-child(2)"));
                    var waitItem = driver.FindElement(By.XPath("//div[@id='Cash']/div/div[9]/div/div[2]/ul[2]/li[2]"));
                    waitItem.Click();
                    Thread.Sleep(1000);

                    // 选择200条记录
                    var pageSzie = driver.FindElement(By.XPath("//div[@id='Cash']/div[4]/div/div/div/div/span"));
                    pageSzie.Click();
                    Thread.Sleep(1000);
                    var pageSzieItem = driver.FindElement(By.XPath("//div[@id='Cash']/div[4]/div/div/div[2]/ul[2]/li[6]"));
                    pageSzieItem.Click();
                    Thread.Sleep(1000);

                    return true;

                }
                catch { }
                Thread.Sleep(1000);
            }
            return false;
        }

        // 查询订单
        public bool Select(int hour)
        {

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    // 设置查询时间，12小时以内的订单
                    var timeRang = driver.FindElement(By.XPath("//div[@id='Cash']/div/div[12]/div/div/div/input"));
                    Helper.SetTimeRang(hour, timeRang);

                    // 点击查询
                    var select = driver.FindElement(By.XPath("//div[@id='Cash']/div/div[13]/button/span"));
                    select.Click();
                    Thread.Sleep(3000);

                    return true;

                }
                catch { }
                Thread.Sleep(1000);
            }
            return false;
        }


        public List<Order> ReadTable()
        {
            var table = driver.FindElement(By.XPath("//*[@id=\"Cash\"]/div[2]/div[1]"));
            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = table.FindElement(By.XPath(bodyPath));

            // 展开所有列表
            var expandList = tbody.FindElements(By.XPath("//*[ivu-table-cell-expand]"));
            foreach (var exBtn in expandList)
            {
                Helper.TryClick(wait, exBtn);
            }

            List<Head> head = Head.ReadHead(table);
            var orders = ReadOrders(head, tbody);
            foreach (var order in orders)
            {
                Console.WriteLine(order.OrderID);
            }
            return orders;
        }

        // 读取每一项的信息
        private List<Order> ReadOrders(List<Head> head, IWebElement body)
        {
            Dictionary<string, string> dicHead = new Dictionary<string, string>(19);
            foreach (var item in head)
            {
                dicHead.Add(item.Name, item.Tag);
            }

            var allRows = body.FindElements(By.XPath(".//tr"));
            var orders = new List<Order>();
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
                var order = Order.Create(dicHead, row, rowEx);
                if (order != null)
                {
                    orders.Add(order);
                }
            }
            return orders;
        }
    }
}

