using boin.Review;
using boin.Util;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TwoStepsAuthenticator;

namespace boin
{
    public class BoinClient : PageBase
    {
        ReviewManager reviewer;
        TimeAuthenticator authenticator;

        private UserPage userPage;
        private OrderPage orderPage;
        private GameBindPage bindPage;

        public BoinClient(AppConfig cnf) : base(newDriver(cnf.Headless), cnf)
        {
            this.cnf = cnf;
            this.reviewer = new ReviewManager(cnf.ReviewFile);
            this.authenticator = new TwoStepsAuthenticator.TimeAuthenticator();
            Cache.Init(cnf.Redis);
        }

        public override void Dispose()
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
                op.AddArgument("window-size=1920,1080");
            }

            //op.AddAdditionalChromeOption("excludeSwitches", new string[] { "enable-automation"});
            //op.AddAdditionalChromeOption("useAutomationExtension", false);

            var driver = new ChromeDriver(op);
            //var session = ((IDevTools)driver).GetDevToolsSession();
            return driver;
        }


        // 登录
        public bool Login()
        {
            driver.Navigate().GoToUrl(cnf.Home);
            // //*[@id="logins"]/div/form/div[1]/div/div/input
            var namePath = "//div[@id=\"logins\"]/div/form/div[1]/div/div/input[@type='text' and @placeholder='请输入账号']";
            SetTextElementByXPath(namePath, cnf.UserName);

            // //*[@id="logins"]/div/form/div[2]/div/div/input
            var pwdPath =
                "//div[@id=\"logins\"]/div/form/div[2]/div/div/input[@type='password' and @placeholder='请输入密码']";
            SetTextElementByXPath(pwdPath, cnf.Password);
            for (var i = 1; i < 100; i++)
            {
                if (login(i))
                {
                    return true;
                }
            }

            return false;
        }


        private bool login(int i)
        {
            // google认证
            var code = authenticator.GetCode(cnf.GoogleKey);
            // //*[@id="logins"]/div/form/div[3]/div/div/input
            var glPath = "//div[@id=\"logins\"]/div/form/div[3]/div/div/input";
            var googlePwd = SetTextElementByXPath(glPath, code);

            // 登录按钮
            // //*[@id="logins"]/div/form/div[4]/div/button
            FindAndClickByXPath("//div[@id=\"logins\"]/div/form/div[4]/div/button", 1000);

            try
            {
                var e = FindElementByXPath("//*[@id='b_home_notice']/h1");
                var txt = Helper.ReadString(e);
                if (txt.Contains("登入成功"))
                {
                    SendMsg("登入成功:" + cnf.UserName);
                    TakeScreenshot(null);
                    return true;
                }
            }
            catch (WebDriverTimeoutException)
            {
                SendMsg("登入超时:" + cnf.UserName + "_" + i.ToString());
                Thread.Sleep(1000 * i);
            }

            return false;
        }

        void initPage()
        {
            bindPage = new GameBindPage(driver, cnf);
            userPage = new UserPage(driver, cnf);
            orderPage = new OrderPage(driver, cnf);
            orderPage.InitItem();
        }

        public void Run()
        {
            if (!this.Login())
            {
                return;
            }

            initPage();

            int zeroCount = 0;
            while (true)
            {
                // run
                try
                {
                    var orders = LoadOrders();
                    SendMsg("order count:" + orders.Count);
                    if (orders.Count > 0)
                    {
                        zeroCount = 0;
                        ReviewOrders(orders);
                    }
                    else
                    {
                        Thread.Sleep((zeroCount > 30 ? 30 : zeroCount) * 2000);
                        zeroCount++;
                    }
                }
                catch (WebDriverException e)
                {
                    zeroCount = 0;
                    bindPage.Close();
                    userPage.Close();
                    orderPage.Close();
                    initPage();
                }
            }
        }

        private void ReviewOrders(List<Order> orders)
        {
            foreach (var order in orders)
            {
                if (reviewer.Review(order))
                {
                    var user = LoadUser(order);
                    ReviewUser(user);
                }
                else
                {
                    order.Processed = true;
                    order.ReviewMsg = "fail";
                    SendMsg(order.ReviewNote());
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
            var orders = SafeExec(() =>
            {
                orderPage.Open();
                var orders = orderPage.Select(cnf.OrderHour, reviewer.Cnf.OrderAmountMax);
                return orders;
            }, 1000, 60);

            var newOrders = new List<Order>();
            foreach (var order in orders)
            {
                // 过滤已经处理过的订单
                //var msg = Cache.GetOrder(order.OrderID);
                //if (string.IsNullOrEmpty(msg))
                {
                    // 锁定订单
                    
                    // 查询绑定
                    var bind = LoadBind(order.GameId, order.CardNo);
                    order.Bind = bind;
                    newOrders.Add(order);
                }
            }

            return newOrders;
        }


        public User LoadUser(string gameId)
        {
            var user = SafeExec(() =>
            {
                userPage.Open();
                var user = userPage.Select(gameId);
                return user;
            }, 1000, 60);
            return user;
        }

        public GameBind LoadBind(string gameId, string cardNo)
        {
            var bind = SafeExec(() =>
            {
                bindPage.Open();
                var bind = bindPage.Select(gameId, cardNo);
                return bind;
            }, 1000, 30);
            return bind;
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
                orderCount.ToString() + ":" + order.OrderID;
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
                orderPage.Unlock(user.Order);
            }

            var msg = user.ReviewNote();
            Cache.SaveOrder(user.Order.OrderID, msg);
            SendMsg(msg);

            user.Order.Processed = true;
            return pass;
        }
    }
}