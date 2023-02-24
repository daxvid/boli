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

        public BoinClient(AppConfig cnf) : base(newDriver(cnf.Headless), cnf)
        {
            this.cnf = cnf;
            this.reviewer = new ReviewManager(cnf.ReviewFile);
            this.authenticator = new TwoStepsAuthenticator.TimeAuthenticator();
            Cache.Init(cnf.Redis);
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
            var pwdPath = "//div[@id=\"logins\"]/div/form/div[2]/div/div/input[@type='password' and @placeholder='请输入密码']";
            SetTextElementByXPath(pwdPath, cnf.Password);
            for (var i = 1; i < 1000; i++)
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
            //if (i <= 1) // test error 
            //{
            //    googlePwd.Clear();
            //    googlePwd.SendKeys("234678");
            //}

            // 登录按钮
            // //*[@id="logins"]/div/form/div[4]/div/button
            FindAndClickByXPath("//div[@id=\"logins\"]/div/form/div[4]/div/button",1000);

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
            }
            return false;
        }


        public void Run()
        {
            //{   //test1
            //    Recharge.GetRechargeName("OR1676436469554825");
            //}
            //{   // test2
            //    var testCard = "6222801251011210972";
            //    //var c = BankUtil.Chenk(testCard);
            //    var d = BankUtil.GetBankInfo(testCard);
            //}

            this.Login();

            //{   // test3
            //    var userPage2 = new UserPage(driver);
            //    userPage2.Open();
            //    userPage2.Select("325961309");   // 325961309
            //}

            ThreadPool.QueueUserWorkItem(state =>
            {
                while (true)
                {
                    // run
                    var orders = LoadOrders();
                    SendMsg("order count:" + orders.Count);
                    ReviewOrders(orders);
                }
            });
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
                using (var orderPage = new OrderPage(driver, cnf))
                {
                    orderPage.Open();
                    var orders = orderPage.Select(cnf.OrderHour, reviewer.Cnf.OrderAmountMax);
                    return orders;
                }
            },1000,60);
            
            var newOrders = new List<Order>();
            foreach (var order in orders)
            {
                // 过滤已经处理过的订单
                var msg = Cache.GetOrder(order.OrderID);
                if (string.IsNullOrEmpty(msg))
                {
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
                using (var userPage = new UserPage(driver, cnf))
                {
                    userPage.Open();
                    var user = userPage.Select(gameId);
                    return user;
                }
            }, 1000, 60);
            return user;
        }

        public List<GameBind> LoadBinds(string gameId)
        {
            var binds = SafeExec(() =>
            {
                using (var bindPage = new GameBindPage(driver, cnf))
                {
                    bindPage.Open();
                    var binds = bindPage.Select(gameId);
                    return binds;
                }
            },1000, 60);
            return binds;
        }

        public GameBind LoadBind(string gameId, string cardNo)
        {
            var bind = SafeExec(() =>
            {
                using (var bindPage = new GameBindPage(driver, cnf))
                {
                    bindPage.Open();
                    var bind = bindPage.Select(gameId, cardNo);
                    return bind;
                }
            },1000,30);
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
            // 通过
            if (success)
            {
                bool pass = true;
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
            var msg = user.ReviewNote();
            Cache.SaveOrder(user.Order.OrderID, msg);
            SendMsg(msg);
            
            user.Order.Processed = true;
            return success;
        }

        public void Quit()
        {
            driver.Quit();
        }

    }
}
