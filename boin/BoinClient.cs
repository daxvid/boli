using System;
using System.IO;
using System.Reflection;
using boin.Review;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TwoStepsAuthenticator;

namespace boin
{
    public class BoinClient: PageBase
    {
        string home = "";
        string userName = "";
        string password = "";
        string googleKey = "";


        TimeAuthenticator authenticator = new TwoStepsAuthenticator.TimeAuthenticator();
        TelegramBot bot = new TelegramBot();

        public BoinClient():base(new ChromeDriver())
        {
        }

        public BoinClient(string home, string userName, string pwd, string googleKey):this()
        {
            this.home = home;
            this.userName = userName;
            this.password = pwd;
            this.googleKey = googleKey;
        }

        // 登录
        public bool Login()
        {
            driver.Navigate().GoToUrl(home);
            // //*[@id="logins"]/div/form/div[1]/div/div/input
            var namePath = "//div[@id=\"logins\"]/div/form/div[1]/div/div/input[@type='text' and @placeholder='请输入账号']";
            SetTextElementByXPath(namePath, userName);

            // //*[@id="logins"]/div/form/div[2]/div/div/input
            var pwdPath = "//div[@id=\"logins\"]/div/form/div[2]/div/div/input[@type='password' and @placeholder='请输入密码']";
            SetTextElementByXPath(pwdPath, password);
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
            var code = authenticator.GetCode(googleKey);
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
                    return true;
                }
            }
            catch (WebDriverTimeoutException)
            {
            }
            return false;
        }


        public void Run()
        {

            //{   //test3
            //    Recharge.GetRechargeName("OR1676436469554825");
            //}
            //{   // test1
            //    var testCard = "6222801251011210972";
            //    //var c = BankUtil.Chenk(testCard);
            //    var d = BankUtil.GetBankInfo(testCard);
            //}

            this.Login();
            //{   // test 2
            //    var userPage2 = new UserPage(driver);
            //    userPage2.Open();
            //    userPage2.Select("325961309");   // 325961309
            //}


            {   // run 
                var orderPage = new OrderPage(driver);
                orderPage.Open();
                orderPage.Select(12);
                var orders = orderPage.ReadTable();

                var userPage = new UserPage(driver);
                userPage.Open();
                userPage.Select(orders);
            }
        }

        public void Quit()
        {
            driver.Quit();
        }
    }
}
