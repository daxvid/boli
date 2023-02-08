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

        // 转到提现网页
        public bool GoToTx()
        {
            for (var i = 0; i < 100; i++)
            {
                try
                {
                    // 选择资金，提现管理 nav li:nth-child(4)
                    var money = driver.FindElement(By.CssSelector("nav li:nth-child(4)"));
                    //var money = driver.FindElement(By.LinkText("资金"));
                    money.Click();
                    Thread.Sleep(1000);
                    var tx = driver.FindElement(By.LinkText("提现管理"));
                    tx.Click();
                    return true;

                }
                catch { }
                Thread.Sleep(1000);
            }
            return false;
        }

        // 查询订单
        public bool SelectOrder()
        {

            for (var i = 0; i < 100; i++)
            {
                try
                {

                    // 全部，待审核
                    var cash = driver.FindElement(By.XPath("//div[@id='Cash']/div/div[9]/div/div/div/i"));
                    cash.Click();
                    Thread.Sleep(1000);
                    //var waitItem = driver.FindElement(By.CssSelector(".ivu-select-visible .ivu-select-item:nth-child(2)"));
                    var waitItem = driver.FindElement(By.XPath("//div[@id='Cash']/div/div[9]/div/div[2]/ul[2]/li[2]"));
                    waitItem.Click();
                    Thread.Sleep(1000);

                    // 日期
                    // //div[@id='Cash']/div/div[12]/div/div/div/input
                    var orderDay = driver.FindElement(By.XPath("//div[@id='Cash']/div/div[12]/div/div/div/input"));
                    orderDay.Click();
                    orderDay.SendKeys(Keys.Control + "a");
                    orderDay.SendKeys(Keys.Delete);
                    orderDay.SendKeys(Keys.Command + "a");
                    orderDay.SendKeys(Keys.Delete);
                    Thread.Sleep(1000);

                    //orderDay.SendKeys("2023-02-05 00:00:00 - 2023-02-07 00:00:00");
                    var now = DateTime.Now;
                    string start = now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss");
                    string end = now.ToString("yyyy-MM-dd HH:mm:ss");
                    orderDay.SendKeys(start + " - " + end);
                    Thread.Sleep(1000);

                    // 选择200条记录
                    var pageSzie = driver.FindElement(By.XPath("//div[@id='Cash']/div[4]/div/div/div/div/span"));
                    pageSzie.Click();
                    Thread.Sleep(1000);
                    var pageSzieItem = driver.FindElement(By.XPath("//div[@id='Cash']/div[4]/div/div/div[2]/ul[2]/li[6]"));
                    pageSzieItem.Click();
                    Thread.Sleep(1000);

                    // 点击查询
                    //var select = driver.FindElement(By.CssSelector(".my-padding-bottom-s > .marginRight:nth-child(1) > span"));
                    var select = driver.FindElement(By.XPath("//div[@id='Cash']/div/div[13]/button/span"));
                    select.Click();
                    Thread.Sleep(3000);

                    return true;

                }
                catch { }
                Thread.Sleep(1000);
            }
            return false;
        }

        public List<KeyValuePair<IWebElement, IWebElement>> ReadOrder()
        {

            var tb = driver.FindElement(By.ClassName("ivu-table-tbody"));

            // 展开所有列表
            var exBtnList = tb.FindElements(By.ClassName("ivu-table-cell-expand"));
            foreach (var exBtn in exBtnList)
            {
                exBtn.Click();
                Thread.Sleep(20);
            }
            Thread.Sleep(3000);

            var allChildrens = tb.FindElements(By.XPath(".//tr"));
            var pairs = new List<KeyValuePair<IWebElement, IWebElement>>();
            var count = allChildrens.Count;
            for (var i = 0; i < count; i++)
            {
                var item = allChildrens[i];
                IWebElement exItem = null;
                var className = item.GetAttribute("class");
                if (className != null && className.StartsWith("ivu-table-row"))
                {
                    if (i + 1 < count)
                    {
                        try
                        {
                            exItem = allChildrens[i + 1].FindElement(By.ClassName("ivu-table-expanded-cell"));
                            if (exItem != null)
                            {
                                i += 1;
                            }
                        }
                        catch { }
                    }
                    pairs.Add(new KeyValuePair<IWebElement, IWebElement>(item, exItem));
                }
            }
            //var lists = tb.FindElements(By.ClassName("ivu-table-row"));
            //var listsEx = tb.FindElements(By.ClassName("ivu-table-expanded-cell"));
            return pairs;
        }

        // 读取每一项的信息
        public void ReadInfo(KeyValuePair<IWebElement, IWebElement> kv)
        {

        }

        public void Run()
        {
            this.Login();
            this.GoToTx();
            this.SelectOrder();
            var orders =  this.ReadOrder();
            foreach(var kv in orders)
            {
                this.ReadInfo(kv);
            }
        }

        public void Quit()
        {
            driver.Quit();
        }
    }
}
