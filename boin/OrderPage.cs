using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Review;
using boin.Util;

namespace boin
{
    public class OrderPage : PageBase
    {
        public OrderPage(ChromeDriver driver, AppConfig cnf) : base(driver,cnf)
        {
        }

        public override bool Open()
        {
            return GoToPage(4, "提现管理");
        }
        
        public override bool Close()
        {
            // //*[@id="layout"]/div/div[2]/div[2]/div/div/div/div[1]/div[1]/div/div[1]/div/div/div/div/div[2]/i
            var path = "//div[@id='layout']/div/div[2]/div[2]/div/div/div/div[1]/div[1]/div/div[1]/div/div/div/div/div[contains(text(),'提现管理')]/i";
            // 关闭窗口
            FindAndClickByXPath(path,100);
            return base.Close();
        }

        private void SetItem()
        {
            // 全部，待审核
            // //*[@id="Cash"]/div[1]/div[9]/div/div[1]/div/i
            FindAndClickByXPath("//div[@id='Cash']/div[1]/div[9]/div/div[1]/div/i",100);
            FindAndClickByXPath("//div[@id='Cash']/div[1]/div[9]/div/div[2]/ul[2]/li[2]",100);

            // 选择200条记录
            // //*[@id="Cash"]/div[4]/div/div/div[1]/div/i
            FindAndClickByXPath("//div[@id='Cash']/div[4]/div/div/div[1]/div/i",100);
            FindAndClickByXPath("//div[@id='Cash']/div[4]/div/div/div[2]/ul[2]/li[6]",100);
        }

        // 查询订单
        public List<Order> Select(int hour, int orderAmountMax)
        {
            SetItem();
            // 设置查询时间，12小时以内的订单
            var timeRang = FindElementByXPath("//div[@id='Cash']/div/div[12]/div/div/div/input");
            Helper.SetTimeRang(timeRang, hour);
            // 点击查询
            // //*[@id="Cash"]/div[1]/div[13]/button[1]/span
            FindAndClickByXPath("//div[@id='Cash']/div[1]/div[13]/button[1]/span[text()='查询']", 2000);
            
            var tablePath = "//*[@id=\"Cash\"]/div[2]/div[1]";
            var table = FindElementByXPath(tablePath);

            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = FindElementByXPath(table, bodyPath);

            var orders = ReadOrders(tbody, orderAmountMax);
            return orders;
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
            Thread.Sleep(500);

            var orders = ReadOrders(tbody);
            return orders;
        }

        // 读取每一项的信息
        private List<Order> ReadOrders(IWebElement tbody)
        {
            var allRows = FindElementsByXPath(tbody, ".//tr");
            var count = allRows.Count;
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

        // 读取每一项的信息
        private List<Order> ReadOrders(IWebElement tbody, int orderAmountMax)
        {
            // .//tr/td[7]/div/span[number(text())<=4000]
            // .//tr/td[13]/div/div[text()='--']
            var path = ".//tr/td[7]/div/span[number(text())<="
                       +orderAmountMax.ToString()
                       +"]/../../../td[13]/div/div[text()='--']/../../..";
            var allRows = FindElementsByXPath(tbody, path);
            
            // 展开所有列表
            // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[2]/td[1]/div/div/i
            var expandPath = "./td[1]/div/div[@class='ivu-table-cell-expand']/i[@class='ivu-icon ivu-icon-ios-arrow-forward']";
            foreach (var row in allRows)
            {
                FindAndClickByXPath(row, expandPath,0);
            }
            Thread.Sleep(200);
            
            var count = allRows.Count;
            var orders = new List<Order>(count);
            for (var i = 0; i < count; i++)
            {
                // following-sibling::ul[1]
                var row = allRows[i];
                var rowEx = FindElementByXPath(row, "./following-sibling::tr[1]/td[@class='ivu-table-expanded-cell']/..");
                if (rowEx == null)
                {
                    throw new NoSuchElementException("not find rowEx");
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

