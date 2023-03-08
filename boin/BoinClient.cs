namespace boin;

using boin.Review;
using boin.Util;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class BoinClient:IDisposable
{
    public ChromeDriver driver;
    
    ReviewManager reviewer;

    private LoginPage loginPage;
    private UserPage userPage;
    private OrderPage orderPage;
    private GameBindPage bindPage;
    private AppConfig cnf;
    private AuthConfig authCnf;

    public BoinClient(AppConfig cnf, AuthConfig authCnf)
    {
        this.cnf = cnf;
        this.authCnf = authCnf;
        this.reviewer = new ReviewManager(cnf.ReviewFile);
        this.driver = newDriver(cnf.Headless);
        Cache.Init(authCnf.Redis, authCnf.Platform);
        
        loginPage = new LoginPage(driver, cnf, authCnf);
        bindPage = new GameBindPage(driver, cnf);
        userPage = new UserPage(driver, cnf);
        orderPage = new OrderPage(driver, cnf);
    }

    public void Dispose()
    {
        driver.Quit();
    }

    static ChromeDriver newDriver(bool headless)
    {
        var op = new ChromeOptions();

        if (headless)
        {
            // 为Chrome配置无头模式
            op.AddArgument("--headless");
            op.AddArgument("window-size=1920,1440");
        }

        //op.AddAdditionalChromeOption("excludeSwitches", new string[] { "enable-automation"});
        //op.AddAdditionalChromeOption("useAutomationExtension", false);

        var driver = new ChromeDriver(op);
        //var session = ((IDevTools)driver).GetDevToolsSession();
        return driver;
    }
    public void SendMsg(string msg)
    {
        Helper.SendMsg(msg);
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
                reviewOrders(orders);
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
                    SendMsg(now.ToString("ok[HH:mm:ss]"));
                }
            }
        }
    }

    private void reviewOrders(List<Order> orders)
    {
        foreach (var order in orders)
        {
            reviewOrder(order);
        }
        // 等待所有订单处理结束
        waitOrders(orders);
    }

    private void reviewOrder(Order order)
    {
        order.Bind = LoadBind(order.GameId, order.CardNo);
        reviewer.Review(order);
        if (order.CanReject)
        {
            order.ReviewMsg = OrderReviewEnum.Reject;
            SaveOrder(order);
            return;
        }

        orderCount++;
        var msg = orderCount.ToString() + ":" + order.OrderId;
        using var span = new Span(msg);
        SendMsg(msg);
        var user = LoadUser(order);
        ReviewUser(user);
    }

    private static void waitOrders(List<Order> orders)
    {
        foreach (var order in orders)
        {
            while (order.Processed == false)
            {
                Thread.Sleep((1000));
            }
        }
    }
    List<Order> LoadOrders()
    {
        while (true)
        {
            try
            {
                var orders = Helper.SafeExec(driver,() =>
                {
                    orderPage.Open();
                    var orders = orderPage.Select(cnf.OrderHour);
                    return orders;
                }, 1000, 10);
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

    User LoadUser(Order order)
    {
        while (true)
        {
            try
            {
                var user = Helper.SafeExec(driver,() =>
                {
                    userPage.Open();
                    var user = userPage.Select(order);
                    return user;
                }, 1000, 10);
                return user;
            }
            catch (WebDriverException)
            {
                userPage.Close();
                userPage = new UserPage(driver, cnf);
            }
        }
    }
    GameBind? LoadBind(string gameId, string cardNo)
    {
        while (true)
        {
            try
            {
                var bind = Helper.SafeExec(driver,() =>
                {
                    bindPage.Open();
                    var bind = bindPage.Select(gameId, cardNo);
                    return bind;
                }, 1000, 10);
                return bind;
            }
            catch (WebDriverException)
            {
                bindPage.Close();
                bindPage = new GameBindPage(driver, cnf);
            }
        }
    }

    static int orderCount = 0;

    void ReviewUser(User user)
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
    
    void Review(User user)
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

    void SaveOrder(Order order)
    {
        orderPage.SubmitOrder(order);
        var msg = order.ReviewNote();
        Cache.SaveOrder(order.OrderId, msg);
        order.Processed = true;
        SendMsg(msg);
    }
}