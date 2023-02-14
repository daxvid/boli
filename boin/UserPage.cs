using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    public class UserPage
    {
        ChromeDriver driver;
        WebDriverWait wait;

        public UserPage(ChromeDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
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
                if (trySelect(gameid))
                {
                    var users = ReadTable(gameid);
                    if (users.Count > 0)
                    {
                        return users[0];
                    }
                }
                return user;
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
            return user;
        }


        private bool trySelect(string gameid)
        {
            var result = wait.Until(driver =>
            {
                try
                {
                    // 设置游戏ID
                    var path = "//div[@id='LiveGameRoleList']/div/div/div[contains(text(),'游戏ID')]/div/input";
                    var name = driver.FindElement(By.XPath(path));
                    name.SendKeys(gameid);
                    // 查询按钮
                    path = "//div[@id='LiveGameRoleList']/div/div/div/button/span[text()='查询']";
                    driver.FindElement(By.XPath(path)).Click();
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
            if (result && gameid != string.Empty)
            {
                // TODO: 等待查询结果
                // /html/body/div[1]/div/div/div[2]/div[2]/div/div/div/div[2]/div/div[2]/div[2]/div[1]/div[2]/table/tbody/tr/td[2]/div/div/div/div[2]/div[3]/div/span
                // //*[@id="LiveGameRoleList"]/div[2]/div[2]/div[1]/div[2]/table/tbody/tr/td[2]/div/div/div/div[2]/div[3]/div/span
                var path = "//*[@id=\"LiveGameRoleList\"]/div[2]/div[2]/div[1]/div[2]/table/tbody/tr/td[2]/div/div/div/div[2]/div[3]/div/span[text()='" + gameid + "']";
                result = wait.Until(driver => {
                    try
                    {
                        driver.FindElement(By.XPath(path));
                        return true;
                    }
                    catch (NoSuchElementException) { }
                    return false;
                });
            }
            return result;
        }


        public List<User> ReadTable(string gameid)
        {
            var table = driver.FindElement(By.XPath("//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]"));
            var tbody = table.FindElement(By.XPath(".//tbody[@class='ivu-table-tbody']"));
            // 展开所有显示
            var expandList = tbody.FindElements(By.XPath(".//button/span[text()='显示']"));
            foreach (var exBtn in expandList)
            {
                try
                {
                    if (exBtn.Enabled && exBtn.Displayed)
                    {
                        exBtn.Click();
                    }
                }
                catch { }
            }

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
            int gameCount = 0;
            for (var i = 0; i < users.Count; i++)
            {
                var user = users[i];
                Console.WriteLine("AppID:" + user.AppId + "; GameId:" + user.GameId);
                Int64 gid;
                if (Int64.TryParse(user.GameId, out gid))
                {
                    // 概况(资金)
                    var ls = readFunding(user.OpButton, user, gameCount);
                    user.Funding = ls;

                    // 注单
                    List<GameLog> logs = readGameLog(user.OpButton, user, gameCount);
                    user.GameLogs = logs;

                    gameCount++;
                }
                else
                {
                    // 用户不存在
                    var all1 = driver.FindElements(By.XPath("//div[@id='timeListBox']/div/div[2]/button[4]/span"));
                    var all2 = driver.FindElements(By.XPath("//div[@id='timeListBox']/div/div[2]/button[4]/span[text()='注单']"));
                    Console.WriteLine("无效的游戏ID：" + user.GameId);
                }
            }

            return users;
        }

        // 读取每一项用户信息
        private List<User> ReadUsers(List<Head> head, IWebElement tbody)
        {
            Dictionary<string, string> dicHead = new Dictionary<string, string>(head.Count * 2);
            foreach (var item in head)
            {
                dicHead.Add(item.Name, item.Tag);
            }

            var allRows = tbody.FindElements(By.XPath(".//tr"));
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


        // 注单
        private List<GameLog> readGameLog(IWebElement opBtn, User user, int i)
        {
            // 移动在操作+，显示出扩展按钮
            new Actions(driver).MoveToElement(opBtn).Perform();
            var xpath = "(//div[@id='timeListBox']/div/div[2]/button[4]/span[text()='注单'])[" + (i + 1).ToString() + "]";
            var result = wait.Until(driver =>
            {
                try
                {
                    // 点击扩展按钮中的注单
                    driver.FindElement(By.XPath(xpath)).Click();
                    return true;
                }
                catch (ElementNotInteractableException) { }
                catch (NoSuchElementException) { }
                catch (InvalidOperationException) { }
                catch
                {
                    throw;
                }
                return false;
            });

            GameLogPage gl = new GameLogPage(driver);
            var logs = gl.Select(user);
            return logs;
        }

        // 出入金概况
        private Funding readFunding(IWebElement opBtn, User user, int i)
        {
            // 移动在操作+，显示出扩展按钮
            new Actions(driver).MoveToElement(opBtn).Perform();
            var xpath = "(//div[@id='timeListBox']/div/div[2]/button[3]/span[text()='概况'])[" + (i + 1).ToString() + "]";
            var result = wait.Until(driver =>
            {
                try
                {
                    // 点击扩展按钮中的概况
                    driver.FindElement(By.XPath(xpath)).Click();
                    return true;
                }
                catch (ElementNotInteractableException) { }
                catch (NoSuchElementException) { }
                catch (InvalidOperationException) { }
                catch
                {
                    throw;
                }
                return false;
            });

            FundingPage gl = new FundingPage(driver);
            var funding = gl.Select(user);
            Thread.Sleep(100);
            return funding;
        }


    }
}
