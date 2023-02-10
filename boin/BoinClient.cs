using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace boin
{
    public class BoinClient
    {
        string home = "";
        string userName = "";
        string password = "";
        string googleKey = "";
        ChromeDriver driver = new ChromeDriver();


        public BoinClient()
        {
        }


        public BoinClient(string home, string userName, string pwd, string googleKey)
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

            //var name = driver.FindElement(By.CssSelector(".ivu-form-item:nth-child(1) .ivu-input"));
            var name = driver.FindElement(By.XPath("//div[@id='logins']/div/form/div/div/div/input"));
            name.SendKeys(userName);

            //var pwd = driver.FindElement(By.CssSelector(".ivu-input-type-password > .ivu-input"));
            var pwd = driver.FindElement(By.XPath("//div[@id='logins']/div/form/div[2]/div/div/input"));
            pwd.SendKeys(password);

            // google认证
            var authenticator = new TwoStepsAuthenticator.TimeAuthenticator();
            var code = authenticator.GetCode(googleKey);
            //var googlePwd = driver.FindElement(By.CssSelector(".ivu-form-item:nth-child(3) .ivu-input"));
            var googlePwd = driver.FindElement(By.XPath("//div[@id='logins']/div/form/div[3]/div/div/input"));

            googlePwd.SendKeys(code);

            // 登录按钮
            var sub = driver.FindElement(By.CssSelector(".ivu-btn-primary"));
            sub.Click();

            for (var i = 60; i > 0; i--)
            {
                Thread.Sleep(1000);
                var urlNow = driver.Url;
                if (urlNow != home)
                {
                    return true;
                }

            }
            return false;
        }


        public static bool GoToPage(ChromeDriver driver, int index, string itme)
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    var money = driver.FindElement(By.CssSelector("nav li:nth-child(" + index + ")"));
                    money.Click();
                    Thread.Sleep(1000);
                    var tx = driver.FindElement(By.LinkText(itme));
                    tx.Click();
                    return true;

                }
                catch { }
                Thread.Sleep(1000);
            }
            return false;
        }


        public void Run()
        {
            this.Login();

            //var orderPage = new OrderPage(driver);
            //orderPage.Open();
            //orderPage.Select(12);
            //var orders = orderPage.ReadTable();
            //var userPage = new UserPage(driver);
            //userPage.Open();
            //userPage.Select(new List<Order>());

            var userPage2 = new UserPage(driver);
            userPage2.Open();
            userPage2.Select("");
            //userPage2.Select("107989384");
        }

        public void Quit()
        {
            driver.Quit();
        }
    }
}
