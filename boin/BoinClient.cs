using boin.Review;
using boin.Util;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TwoStepsAuthenticator;

namespace boin;

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
        Cache.Init(authCnf.Redis);
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

    public void SendMsg(Exception err)
    {
        Helper.SendMsg(err);
    }
    
    void initPage()
    {
        bindPage = new GameBindPage(driver, cnf);
        userPage = new UserPage(driver, cnf);
        orderPage = new OrderPage(driver, cnf, reviewer.Cnf.OrderAmountMax);
        orderPage.InitItem();
    }

    public void Run()
    {
        loginPage = new LoginPage(driver, cnf, authCnf);
        if (!loginPage.Login())
        {
            return;
        }

        initPage();

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
                    SendMsg(now.ToString("ok[HH:mm:ss]"));
                }
            }
        }
    }

    private void ReviewOrders(List<Order> orders)
    {
        foreach (var order in orders)
        {
            order.Bind = LoadBind(order.GameId, order.CardNo);
            if (reviewer.Review(order))
            {
                var user = LoadUser(order);
                ReviewUser(user);
            }
            else
            {
                order.Processed = true;
                order.ReviewMsg = "fail";
                
                orderPage.Unlock(order.OrderId);
                var msg = order.ReviewNote();
                Cache.SaveOrder(order.OrderId, msg);
                SendMsg(msg);
            }
        }

        // 等待所有订单处理结束
        WaitOrders(orders);
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

    public List<Order> LoadOrders()
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
                }, 1000, 60);
                return orders;
            }
            catch (WebDriverException e)
            {
                orderPage.Close();
                orderPage = new OrderPage(driver, cnf, reviewer.Cnf.OrderAmountMax);
                orderPage.InitItem();
            }
        }
    }


    public User LoadUser(string gameId)
    {
        while (true)
        {
            try
            {
                var user = Helper.SafeExec(driver,() =>
                {
                    userPage.Open();
                    var user = userPage.Select(gameId);
                    return user;
                }, 1000, 60);
                return user;
            }
            catch (WebDriverException e)
            {
                userPage.Close();
                userPage = new UserPage(driver, cnf);
            }
        }
    }

    public GameBind LoadBind(string gameId, string cardNo)
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
                }, 1000, 30);
                return bind;
            }
            catch (WebDriverException e)
            {
                bindPage.Close();
                bindPage = new GameBindPage(driver, cnf);
            }
        }
    }

    static int orderCount = 0;

    public bool ReviewUser(User user)
    {
        while (true)
        {
            if (user.Funding.IsSyncName)
            {
                return Review(user);
            }

            Thread.Sleep(1);
        }
    }

    public User LoadUser(Order order)
    {
        orderCount++;
        var msg = // "user:" + order.GameId + Environment.NewLine + "o_" +
            orderCount.ToString() + ":" + order.OrderId;
        using (var span = new Span())
        {
            SendMsg(msg);
            span.Msg = msg;
            var user = LoadUser(order.GameId);
            user.Order = order;
            return user;
        }
    }

    private bool Review(User user)
    {
        bool success = reviewer.Review(user);
        bool pass = success;
        // 通过
        if (success)
        {
            foreach (var v in user.ReviewResult)
            {
                if (v.Code > 0)
                {
                    pass = false;
                    break;
                }
            }

            if (pass)
            {
                user.ReviewMsg = "pass";
            }
            else
            {
                // 待定，进入人工
                user.ReviewMsg = "unknow";
            }
        }
        else
        {
            // 可以拒绝
            user.ReviewMsg = "fail";
        }

        if (pass)
        {
            orderPage.Pass(user.Order);
        }
        else
        {
            orderPage.Unlock(user.Order.OrderId);
        }

        var msg = user.ReviewNote();
        Cache.SaveOrder(user.Order.OrderId, msg);
        SendMsg(msg);

        user.Order.Processed = true;
        return pass;
    }
}