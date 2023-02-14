using System;
using System.IO;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    public class BoinClient
    {
        string home = "";
        string userName = "";
        string password = "";
        string googleKey = "";
        ChromeDriver driver;
        WebDriverWait wait;

        public BoinClient()
        {
            driver = new ChromeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
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
            //var name = driver.FindElement(By.CssSelector(".ivu-form-item:nth-child(1) .ivu-input"));
            var name = driver.FindElement(By.XPath("//div[@id='logins']/div/form/div/div/div/input"));
            name.SendKeys(userName);

            //var pwd = driver.FindElement(By.CssSelector(".ivu-input-type-password > .ivu-input"));
            var pwd = driver.FindElement(By.XPath("//div[@id='logins']/div/form/div[2]/div/div/input"));
            pwd.SendKeys(password);
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
            var authenticator = new TwoStepsAuthenticator.TimeAuthenticator();
            var code = authenticator.GetCode(googleKey);
            var googlePwd = driver.FindElement(By.XPath("//div[@id='logins']/div/form/div[3]/div/div/input"));
            googlePwd.Clear();
            googlePwd.SendKeys(code);
            //if (i < 3) // test error 
            //{
            //    googlePwd.Clear();
            //    googlePwd.SendKeys("234678");
            //}

            // 登录按钮
            var sub = driver.FindElement(By.CssSelector(".ivu-btn-primary"));
            sub.Click();

            try
            {
                var path = "//*[@id='b_home_notice']/h1";
                var result = wait.Until(driver =>
                {
                    try
                    {
                        var e = driver.FindElement(By.XPath(path));
                        var txt = e.Text;
                        if (txt.Contains("登入成功"))
                        {
                            return true;
                        }
                    }
                    catch (NoSuchElementException) //TargetInvocationException/InvalidOperationException
                    {
                    }
                    catch
                    {
                        throw;
                    }
                    return false;
                }
                );
                return result;
            }
            catch (WebDriverTimeoutException)
            {
            }
            catch
            {
                throw;
            }
            return false;
        }

        public static bool GoToPage(ChromeDriver driver, int index, string itme)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.PollingInterval = TimeSpan.FromSeconds(1);
            var result = wait.Until(driver =>
            {
                try
                {
                    driver.FindElement(By.CssSelector("nav li:nth-child(" + index + ")")).Click();
                    driver.FindElement(By.LinkText(itme)).Click();
                    return true;

                }
                catch (NoSuchElementException) { }
                catch (InvalidOperationException) { }
                catch
                {
                    throw;
                }
                return false;
            });
            return result;
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
            //userPage.Select(orders);

            var userPage2 = new UserPage(driver);
            userPage2.Open();
            userPage2.Select("325961309");   // 325961309
            //userPage2.Select("");

        }

        public void Quit()
        {
            driver.Quit();
        }
    }
}
