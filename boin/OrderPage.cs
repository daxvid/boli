using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Review;
using boin.Util;

namespace boin
{
    public class OrderPage : LablePage
    {
        public OrderPage(ChromeDriver driver, AppConfig cnf) : base(driver, cnf, 4, "提现管理")
        {
        }

        public void InitItem()
        {
            this.Open();
            // 全部，待审核
            // //*[@id="Cash"]/div[1]/div[9]/div/div[1]/div/i
            FindAndClickByXPath("//div[@id='Cash']/div[1]/div[9]/div/div[1]/div/i", 100);
            FindAndClickByXPath("//div[@id='Cash']/div[1]/div[9]/div/div[2]/ul[2]/li[2]", 100);

            // 选择200条记录
            // //*[@id="Cash"]/div[4]/div/div/div[1]/div/i
            // FindAndClickByXPath("//div[@id='Cash']/div[4]/div/div/div[1]/div/i", 100);
            // FindAndClickByXPath("//div[@id='Cash']/div[4]/div/div/div[2]/ul[2]/li[6]", 100);
        }

        // 查询订单
        public List<Order> Select(int hour, int orderAmountMax)
        {
            // 设置查询时间，12小时以内的订单
            var timeRang = FindElementByXPath("//div[@id='Cash']/div/div[12]/div/div/div/input");
            Helper.SetTimeRang(timeRang, hour);
            // 点击查询
            // //*[@id="Cash"]/div[1]/div[13]/button[1]/span
            FindAndClickByXPath("//div[@id='Cash']/div[1]/div[13]/button[1]/span[text()='查询']", 2000);

            var tablePath = "//*[@id='Cash']/div[2]/div[1]";
            var table = FindElementByXPath(tablePath);

            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = FindElementByXPath(table, bodyPath);

            var orders = ReadOrders(tbody, orderAmountMax);
            return orders;
        }

        // public List<Order> ReadTable()
        // {
        //     var tablePath = "//*[@id='Cash']/div[2]/div[1]";
        //     var table = FindElementByXPath(tablePath);
        //
        //     var bodyPath = ".//tbody[@class='ivu-table-tbody']";
        //     var tbody = FindElementByXPath(table, bodyPath);
        //     // var dicHead = ReadHeadDic(table);
        //
        //     // 展开所有列表
        //     // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[2]/td[1]/div/div/i
        //     var expandPath =
        //         "./tr/td[1]/div/div[@class='ivu-table-cell-expand']/i[@class='ivu-icon ivu-icon-ios-arrow-forward']";
        //     var expandItems = FindElementsByXPath(tbody, expandPath);
        //     for (var i = 0; i < expandItems.Count; i++)
        //     {
        //         SafeClick(expandItems[i], 10);
        //     }
        //
        //     Thread.Sleep(500);
        //
        //     var orders = ReadOrders(tbody);
        //     return orders;
        // }
        //
        // // 读取每一项的信息
        // private List<Order> ReadOrders(IWebElement tbody)
        // {
        //     var allRows = FindElementsByXPath(tbody, ".//tr");
        //     var count = allRows.Count;
        //     var orders = new List<Order>(count);
        //     for (var i = 0; i < count; i++)
        //     {
        //         var row = allRows[i];
        //         IWebElement rowEx = null;
        //         var className = row.GetAttribute("class");
        //         if (!className.StartsWith("ivu-table-row"))
        //         {
        //             continue;
        //         }
        //
        //         if (i + 1 < count)
        //         {
        //             rowEx = allRows[i + 1].FindElement(By.XPath(".//td[@class='ivu-table-expanded-cell']"));
        //             if (rowEx != null)
        //             {
        //                 i += 1;
        //             }
        //         }
        //
        //         var order = Order.Create(row, rowEx);
        //         if (order != null)
        //         {
        //             orders.Add(order);
        //         }
        //     }
        //
        //     return orders;
        // }

        // 读取每一项的信息
        private List<Order> ReadOrders(IWebElement tbody, int orderAmountMax)
        {
            // .//tr/td[7]/div/span[number(text())<=4000]
            // .//tr/td[13]/div/div[text()='--']
            var path = ".//tr/td[7]/div/span[number(text())<="
                       + orderAmountMax.ToString()
                       + "]/../../../td[13]/div/div[text()='--']/../../..";
            var allRows = FindElementsByXPath(tbody, path);

            // 展开所有列表
            // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[2]/td[1]/div/div/i
            var expandPath =
                "./td[1]/div/div[@class='ivu-table-cell-expand']/i[@class='ivu-icon ivu-icon-ios-arrow-forward']";
            foreach (var row in allRows)
            {
                FindAndClickByXPath(row, expandPath, 0);
            }

            Thread.Sleep(200);

            var count = allRows.Count;
            var orders = new List<Order>(count);
            for (var i = count-1; i >=0 ; i--)
            {
                // following-sibling::ul[1]
                var row = allRows[i];
                var rowEx = FindElementByXPath(row,
                    "./following-sibling::tr[1]/td[@class='ivu-table-expanded-cell']/..");
                if (rowEx == null)
                {
                    throw new NoSuchElementException("not find rowEx");
                }

                var order = Order.Create(row, rowEx);
                if (order != null && lockOrder(order))
                {
                    orders.Add(order);
                    if (orders.Count >= cnf.OrderMaxLock)
                    {
                        return orders;
                    }
                }
            }

            return orders;
        }


        private bool lockOrder(Order order)
        {
            this.Open();
            // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[02]/div/span
            // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[14]/div/div/div/div/div/button/span
            // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[4]/td[14]/div/div/div/div/div/button/span
            string td14Path = "//div[@id='Cash']/div[2]/div[1]/div[2]/table/tbody/tr/td[02]/div/span[text()='" +
                              order.OrderID +
                              "']/../../../td[14]/div/div/div/div/div";
            var td14 = FindElementByXPath(td14Path);
            try
            {
                string lockPath = "./button/span[text()='锁定']";
                var lockBtns = FindElementsByXPath(td14, lockPath);
                if (lockBtns.Count == 1)
                {
                    lockBtns[0].Click();
                    // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[14]/div/div/div/div/div/button[1]/span
                    string hit = "锁定";
                    while (hit == "锁定")
                    {
                        Thread.Sleep(1000);
                        td14 = FindElementByXPath(td14Path);
                        hit = td14.Text.Trim();
                    }

                    Console.WriteLine(order.OrderID+hit);
                    if (hit.StartsWith("审核"))
                    {
                        return true;
                    }
                }
                else if (lockBtns.Count == 0)
                {
                    string reviewBtn = "./button[1]/span[text()='审核']";
                    var reVeviewBtns = FindElementsByXPath(td14, reviewBtn);
                    return reVeviewBtns.Count == 1;
                }
            }
            catch(Exception err)
            {
                Console.WriteLine(err);
            }
            return false;
        }

        public bool Pass(Order order)
        {
            this.Open();
            string td14Path = "//div[@id='Cash']/div[2]/div[1]/div[2]/table/tbody/tr/td[02]/div/span[text()='" +
                              order.OrderID +
                              "']/../../../td[14]/div/div/div/div/div";

            string reviewBtn = td14Path + "/button[1]/span[text()='审核']";
            FindAndClickByXPath(reviewBtn, 4000);
            bool pass = false;
            try
            {
                using (var vp = new ReviewPage(driver, cnf, order))
                {
                    pass = vp.Review();
                }
            }
            finally
            {
                if (pass == false)
                {
                    Unlock(order);
                }
            }

            return pass;
        }

        // 解锁
        public bool Unlock(Order order)
        { 
            this.Open();
            string td14Path = "//div[@id='Cash']/div[2]/div[1]/div[2]/table/tbody/tr/td[02]/div/span[text()='" +
                              order.OrderID +
                              "']/../../../td[14]/div/div/div/div/div";

            // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[14]/div/div/div/div/div/button[3]/span
            string reviewBtn = td14Path + "/button[3]/span[text()='解锁']";
            FindAndClickByXPath(reviewBtn, 4000);
            return true;
        }
    }
}

