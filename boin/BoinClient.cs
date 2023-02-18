using System;
using System.IO;
using System.Reflection;
using boin.Review;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using TwoStepsAuthenticator;

namespace boin
{
    public class BoinClient: PageBase
    {
        ReviewManager reviewer;

        TimeAuthenticator authenticator;

        public BoinClient(AppConfig cnf) : base(newDriver(cnf.Headless), cnf)
        {
            this.cnf = cnf;
            this.reviewer = new ReviewManager(cnf.ReviewFile);
            this.authenticator = new TwoStepsAuthenticator.TimeAuthenticator();
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
            TryClickByXPath("//div[@id=\"logins\"]/div/form/div[4]/div/button");

            try
            {
                var e = FindElementByXPath("//*[@id='b_home_notice']/h1");
                var txt = e.Text;
                if (txt.Contains("登入成功"))
                {
                    SendMsg("登入成功:" + cnf.UserName);
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


            {   // run 
                var orderPage = new OrderPage(driver, cnf);
                orderPage.Open();

                orderPage.Select(cnf.OrderHour);
                var orders = orderPage.ReadTable();

                var passOrders = new List<Order>(orders.Count);
                foreach(var order in orders)
                {
                    if (reviewer.Review(order))
                    {
                        passOrders.Add(order);
                    }
                    else
                    {
                        // TODO:
                    }
                    SendMsg(order.ReviewNote());
                }

                var userPage = new UserPage(driver,cnf, reviewer);
                userPage.Open();
                userPage.Select(passOrders);
            }
        }

        public void Quit()
        {
            driver.Quit();
        }

    }
}
