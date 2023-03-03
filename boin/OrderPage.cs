using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Review;
using boin.Util;

namespace boin;

public class OrderPage : LablePage
{
    private string maxAmount;

    public OrderPage(ChromeDriver driver, AppConfig cnf, int orderAmountMax) : base(driver, cnf, 4, "提现管理")
    {
        this.maxAmount = orderAmountMax.ToString();
    }

    public void InitItem()
    {
        this.Open();

        // 设置最小金额
        // <input autocomplete="off" spellcheck="false" type="text" placeholder="最小金额" class="ivu-input ivu-input-default">
        // //*[@id="Cash"]/div[1]/div[7]/div[1]/input
        SetTextElementByXPath("//div[@id='Cash']/div[1]/div[7]/div[1]/input", "100");

        // 设置最大金额
        // <input autocomplete="off" spellcheck="false" type="text" placeholder="最大金额" class="ivu-input ivu-input-default">
        // //*[@id="Cash"]/div[1]/div[7]/div[2]/input
        SetTextElementByXPath("//div[@id='Cash']/div[1]/div[7]/div[2]/input", maxAmount);

        // 全部，待审核
        // //*[@id="Cash"]/div[1]/div[9]/div/div[1]/div/i
        FindAndClickByXPath("//div[@id='Cash']/div[1]/div[9]/div/div[1]/div/i", 100);
        FindAndClickByXPath("//div[@id='Cash']/div[1]/div[9]/div/div[2]/ul[2]/li[2]", 100);

        // 选择每页记录条数(10/20/30/60/100/200)
        // //*[@id="Cash"]/div[4]/div/div/div[1]/div/i
        FindAndClickByXPath("//div[@id='Cash']/div[4]/div/div/div[1]/div/i", 100);
        // //*[@id="Cash"]/div[4]/div/div/div[2]/ul[2]/li[5]
        // //*[@id="Cash"]/div[4]/div/div/div[2]/ul[2]/li[6]
        FindAndClickByXPath("//div[@id='Cash']/div[4]/div/div/div[2]/ul[2]/li[6]", 100);
    }

    // 查询订单
    public List<Order> Select(int hour)
    {
        // 设置查询时间
        var timeRang = FindElementByXPath("//div[@id='Cash']/div/div[12]/div/div/div/input");
        Helper.SetTimeRang(timeRang, hour);
        // 点击查询
        // //*[@id="Cash"]/div[1]/div[13]/button[1]/span
        FindAndClickByXPath("//div[@id='Cash']/div[1]/div[13]/button[1]/span[text()='查询']", 2000);

        var tablePath = "//*[@id='Cash']/div[2]/div[1]";
        var table = FindElementByXPath(tablePath);

        var bodyPath = ".//tbody[@class='ivu-table-tbody']";
        var tbody = FindElementByXPath(table, bodyPath);

        // 锁定一批
        lockOrders(tbody);

        // 处理等待审核的单
        var orders = ReadOrders(tbody);
        return orders;
    }

    private int waitOrders(IWebElement tbody)
    {
        // 查询出已经锁定的还没有处理的单数
        var waitPath = ".//tr/td[14]/div/div/div/div/div/button[1]/span[text()='审核']";
        var waitRows = FindElementsByXPath(tbody, waitPath);
        return waitRows.Count;
    }

    // 锁定订单
    private int lockOrders(IWebElement tbody)
    {
        int count = waitOrders(tbody);
        if (count > 0)
        {
            return count;
        }

        // 查询出可以锁定的单
        // td[7]/div/span[number(text())<=4000]
        // td[13]/div/div[text()='--']
        // td[14]/div/div/div/div/div/button[1]/span[锁定]
        var path = ".//tr/" +
                   //"td[7]/div/span[number(text())<=" + maxAmount + "]/../../../" +
                   "td[13]/div/div[text()='--']/../../../" +
                   "td[14]/div/div/div/div/div/button[1]/span[text()='锁定']/../../../../../../../..";
        var allRows = FindElementsByXPath(tbody, path);
        // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[14]/td[2]/div

        for (var i = allRows.Count - 1; (i >= 0 && count < cnf.OrderMaxLock); i--)
        {
            var row = allRows[i];
            try
            {
                var orderId = Helper.ReadString(FindElementByXPath(row, "./td[2]/div"));
                // 过滤已经处理过的订单
                var msg = Cache.GetOrder(orderId);
                if (string.IsNullOrEmpty(msg))
                {
                    FindAndClickByXPath(row, "./td[14]/div/div/div/div/div/button[1]/span[text()='锁定']", 10);
                    count++;
                }
            }
            catch
            {
            }
        }

        Console.WriteLine("lockOrders:" + allRows.Count.ToString() + "_" + count);
        Thread.Sleep(1000);
        return count;
    }

    // 读取每一项的信息
    private List<Order> ReadOrders(IWebElement tbody)
    {
        var path = ".//tr/td[14]/div/div/div/div/div/button[1]/span[text()='审核']/../../../../../../../..";
        var allRows = FindElementsByXPath(tbody, path);

        // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[30]/td[1]/div/div/i
        // 未展开 <div class="ivu-table-cell-expand"><i class="ivu-icon ivu-icon-ios-arrow-forward"></i></div>
        // 已展开 <div class="ivu-table-cell-expand ivu-table-cell-expand-expanded"><i class="ivu-icon ivu-icon-ios-arrow-forward"></i></div>
        // 展开所有列表
        // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[2]/td[1]/div/div/i
        var expandPath = "./td[1]/div/div";
        foreach (var row in allRows)
        {
            var exBtn = FindElementByXPath(row, expandPath);
            var className = exBtn.GetAttribute("class");
            if (className == "ivu-table-cell-expand")
            {
                exBtn.Click();
            }
        }

        Thread.Sleep(200);

        var count = allRows.Count;
        var orders = new List<Order>(count);
        for (var i = count - 1; i >= 0; i--)
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
            orders.Add(order);
        }

        return orders;
    }

    private string makePath(string orderId)
    {
        // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[02]/div/span
        // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[4]/td[14]/div/div/div/div/div/button/span
        string td14Path = "//div[@id='Cash']/div[2]/div[1]/div[2]/table/tbody/tr/td[02]/div/span[text()='" +
                          orderId +
                          "']/../../../td[14]/div/div/div/div/div";
        return td14Path;
    }

    // 提交审核结果
    public bool SubmitOrder(Order order, bool pass)
    {
        this.Open();
        string reviewBtn = makePath(order.OrderId) + "/button[1]/span[text()='审核']";
        bool success = false;
        try
        {
            success = Helper.SafeExec(driver, () =>
            {
                FindAndClickByXPath(reviewBtn, 2000);
                using (var vp = new ReviewPage(driver, cnf, order))
                {
                    return vp.Submit(pass);
                }
            }, 1000, 10);
        }
        finally
        {
            if (success == false)
            {
                Unlock(order.OrderId);
            }
        }

        return success;
    }

    // 解锁
    public bool Unlock(string orderId)
    {
        this.Open();
        // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[14]/div/div/div/div/div/button[3]/span
        string reviewBtn = makePath(orderId) + "/button[3]/span[text()='解锁']";
        FindAndClickByXPath(reviewBtn, 4000);
        return true;
    }

    private bool lockOrder(string orderId)
    {
        string td14Path = makePath(orderId);
        var td14 = FindElementByXPath(td14Path);
        string lockPath = "./button[1]/span[text()='锁定' or text()='审核']";
        var lockBtn = FindElementByXPath(td14, lockPath);
        var hit = lockBtn.Text;
        if (hit == "审核")
        {
            return true;
        }

        if (hit == "锁定")
        {
            lockBtn.Click();
            // //*[@id="Cash"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[14]/div/div/div/div/div/button[1]/span
            for (int i = 0; (i < 100 && hit == "锁定"); i++)
            {
                Thread.Sleep(200);
                td14 = FindElementByXPath(td14Path);
                hit = td14.Text.Trim();
            }

            if (hit.StartsWith("审核"))
            {
                return true;
            }
        }

        return false;
    }
}

