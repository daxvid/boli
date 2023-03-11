namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Boin.Review;
using Boin.Util;

public class BoinClient : IDisposable
{
    private static int _orderCount;
    private readonly ChromeDriver driver;

    private readonly ReviewManager reviewer;

    private readonly LoginPage loginPage;
    private UserPage userPage;
    private OrderPage orderPage;
    private GameBindPage bindPage;
    
    private readonly AppConfig cnf;
    private readonly AuthConfig authCnf;
    private bool closed;

    public BoinClient(AppConfig cnf, AuthConfig authCnf)
    {
        this.cnf = cnf;
        this.authCnf = authCnf;
        this.reviewer = new ReviewManager(cnf.ReviewFile);
        this.driver = NewDriver(cnf.Headless, cnf.WindowSize);
        Cache.Init(authCnf.Redis, authCnf.Platform);

        loginPage = new LoginPage(driver, cnf, authCnf);
        bindPage = new GameBindPage(driver, cnf);
        userPage = new UserPage(driver, cnf);
        orderPage = new OrderPage(driver, cnf);
    }

    public void Close()
    {
        if (closed)
        {
            return;
        }

        closed = true;
        driver.Quit();
    }
    
    public void SaveException(Exception err)
    {
        try
        {
            Log.SaveException(err, driver, string.Empty);
        }
        catch
        {
        }
    }
    
    public void Dispose()
    {
        Close();
    }

    private static ChromeDriver NewDriver(bool headless, string windowSize)
    {
        var op = new ChromeOptions();

        if (headless)
        {
            // 为Chrome配置无头模式
            op.AddArgument("--headless");
        }

        if (!string.IsNullOrEmpty(windowSize))
        {
            //windowSize = "window-size=1366,768";
            op.AddArgument("window-size=" + windowSize);
        }
        //op.AddAdditionalChromeOption("excludeSwitches", new string[] { "enable-automation"});
        //op.AddAdditionalChromeOption("useAutomationExtension", false);

        var driver = new ChromeDriver(op);
        //var session = ((IDevTools)driver).GetDevToolsSession();
        return driver;
    }

    public void Run()
    {
        if (!loginPage.Login())
        {
            return;
        }

        orderPage.InitItem();

        int zeroCount = 0;
        DateTime heartbeatTime = DateTime.Now;
        while (true)
        {
            var orders = LoadOrders();
            //SendMsg("order count:" + orders.Count);
            if (orders.Count > 0)
            {
                zeroCount = 0;
                ReviewOrders(orders);
                heartbeatTime = DateTime.Now;
            }
            else
            {
                Thread.Sleep((zeroCount > 20 ? 20 : zeroCount) * 1000);
                zeroCount++;
                var now = DateTime.Now;
                if ((now - heartbeatTime).TotalSeconds >= 60)
                {
                    heartbeatTime = now;
                    Helper.SendMsg(now.ToString("ok[HH:mm:ss]"));
                }
            }
        }
    }

    private void ReviewOrders(List<Order> orders)
    {
        foreach (var order in orders)
        {
            ReviewOrder(order);
        }

        // 等待所有订单处理结束
        WaitOrders(orders);
    }

    private void ReviewOrder(Order order)
    {
        order.Bind = LoadBind(order.GameId, order.CardNo);
        reviewer.Review(order);
        if (order.CanReject)
        {
            order.ReviewMsg = OrderReviewEnum.Reject;
            SaveOrder(order);
            return;
        }

        _orderCount++;
        var msg = _orderCount.ToString() + ":" + order.OrderId;
        using var span = new Span(msg);
        Helper.SendMsg(msg);
        var user = LoadUser(order);
        ReviewUser(user);
    }

    private static void WaitOrders(List<Order> orders)
    {
        foreach (var order in orders)
        {
            while (order.Processed == false)
            {
                Thread.Sleep((1000));
            }
        }
    }

    private List<Order> LoadOrders()
    {
        while (true)
        {
            try
            {
                var orders = Helper.SafeExec(driver, () =>
                {
                    orderPage.Open();
                    var orders = orderPage.Select(cnf.OrderHour);
                    return orders;
                }, 2000, 5);
                return orders;
            }
            catch (WebDriverException)
            {
                orderPage.Close();
                orderPage = new OrderPage(driver, cnf);
                orderPage.InitItem();
            }
        }
    }

    private User LoadUser(Order order)
    {
        while (true)
        {
            try
            {
                var user = Helper.SafeExec(driver, () =>
                {
                    userPage.Open();
                    var user = userPage.Select(order);
                    return user;
                }, 2000, 5);
                return user;
            }
            catch (WebDriverException)
            {
                userPage.Close();
                userPage = new UserPage(driver, cnf);
            }
        }
    }

    private GameBind? LoadBind(string gameId, string cardNo)
    {
        while (true)
        {
            try
            {
                var bind = Helper.SafeExec(driver, () =>
                {
                    bindPage.Open();
                    var bind = bindPage.Select(gameId, cardNo);
                    return bind;
                }, 2000, 5);
                return bind;
            }
            catch (WebDriverException)
            {
                bindPage.Close();
                bindPage = new GameBindPage(driver, cnf);
            }
        }
    }

    private void ReviewUser(User user)
    {
        while (true)
        {
            if (user.Funding.IsSyncName)
            {
                Review(user);
                return;
            }

            Thread.Sleep(1);
        }
    }

    private void Review(User user)
    {
        reviewer.Review(user);
        var order = user.Order;
        if (order.CanPass)
        {
            order.ReviewMsg = OrderReviewEnum.Pass;
        }
        else if (order.CanReject)
        {
            // 可以拒绝
            order.ReviewMsg = OrderReviewEnum.Reject;
        }
        else
        {
            // 待定，进入人工
            order.ReviewMsg = OrderReviewEnum.Doubt;
        }

        SaveOrder(order);
    }

    private void SaveOrder(Order order)
    {
        orderPage.SubmitOrder(order);
        var msg = order.ReviewNote();
        Cache.SaveOrder(order.OrderId, msg);
        order.Processed = true;
        Helper.SendMsg(msg);
    }
}