using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace boin
{
    public class UserPage
    {

        ChromeDriver driver;

        public UserPage(ChromeDriver driver)
        {
            this.driver = driver;
        }


        public bool Open()
        {
            return BoinClient.GoToPage(driver, 1, "用户列表");
        }


        public List<User> Select(List<Order> orders)
        {
            List<User> users = new List<User>();
            foreach (var order in orders)
            {
                var user = Select(order.GameId);
                users.Add(user);
            }
            return users;
        }


        public User Select(string gameid)
        {
            User user = new User();
            try
            {
                var name = driver.FindElement(By.XPath("//div[@id='LiveGameRoleList']/div/div/div[3]/div/input"));
                name.SendKeys(gameid);

                // 查询按钮.my-padding-bottom-s > .my-margin-right-s:nth-child(2) > span
                var sub = driver.FindElement(By.XPath("//div[@id='LiveGameRoleList']/div/div/div[9]/button/span"));
                sub.Click();
                var users = ReadTable();
                if (users.Count > 0)
                {
                    return users[0];
                }
                return user;
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
            return user;
        }


        public List<User> ReadTable()
        {
            Thread.Sleep(2000);
            var table = driver.FindElement(By.XPath("//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]"));
            var tbody = table.FindElement(By.XPath(".//tbody[@class='ivu-table-tbody']"));
            // 展开所有显示      
            var expandList = tbody.FindElements(By.TagName("button"));
            foreach (var exBtn in expandList)
            {
                if (exBtn.Text == "显示")
                {
                    exBtn.Click();
                    Thread.Sleep(20);
                }
            }
            Thread.Sleep(100);

            List<Head> head = Head.ReadHead2(table);
            // fix head
            for (var i = 0; i < User.Heads.Length && i < head.Count; i++)
            {
                if (head[i].Name == "")
                {
                    head[i].Name = User.Heads[i];
                }
            }

            var users = ReadUsers(head, tbody);
            foreach (var user in users)
            {
                Console.WriteLine(user.GameId);
            }

            foreach (var opBtn in expandList)
            {
                if (opBtn.Text == "操作 +")
                {
                    readGameLog(opBtn);
                }

            }
            Thread.Sleep(2000);


            return users;
        }

        private void readGameLog(IWebElement? opBtn)
        {
            // 移动在操作+，显示出扩展按钮
            new Actions(driver).MoveToElement(opBtn).Perform();
            Thread.Sleep(1000);
            // 点击扩展按钮中的注单
            var btn = driver.FindElement(By.XPath("//div[@id='timeListBox']/div/div[2]/button[4]/span[text()='注单']"));
            btn.Click();
            Thread.Sleep(100);

            // 用户游戏日志
            var gameLog = driver.FindElement(By.XPath("//div[text()='用户游戏日志' and @class='ivu-modal-header-inner']/parent::div/parent::div"));
            var timeRang = gameLog.FindElement(By.XPath(".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']"));
            Helper.SetTimeRang(24, timeRang);

            // 点击查询按钮;
            var sub = gameLog.FindElement(By.XPath(".//button/span[text()='查询']"));
            sub.Click();
            Thread.Sleep(2000);

            // 读取查询结果
        }


        // 读取每一项用户信息
        private List<User> ReadUsers(List<Head> head, IWebElement body)
        {
            Dictionary<string, string> dicHead = new Dictionary<string, string>(head.Count * 2);
            foreach (var item in head)
            {
                dicHead.Add(item.Name, item.Tag);
            }

            var allRows = body.FindElements(By.XPath(".//tr"));
            var count = allRows.Count;
            var users = new List<User>(count);
            for (var i = 0; i < count; i++)
            {
                var row = allRows[i];
                var user = User.Create(dicHead, row);
                if (user != null)
                {
                    users.Add(user);
                }
            }
            return users;
        }
    }
}

