using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using boin.Review;
using OpenQA.Selenium.Internal;
using System.Linq;
using boin.Util;

namespace boin
{
    public class UserPage : PageBase
    {

        public UserPage(ChromeDriver driver, AppConfig cnf) : base(driver, cnf)
        {
        }


        public override bool Open()
        {
            return GoToPage(1, "用户列表");
        }

        public User Select(string gameid)
        {
            while (true)
            {
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
                }
                catch (WebDriverTimeoutException)
                {
                    Thread.Sleep(1000);
                }
                //catch (NoSuchElementException) { }
                //catch (ElementClickInterceptedException) { }
                //catch (ElementNotInteractableException) { }
                //catch (InvalidOperationException) { }
                catch (WebDriverException)
                {
                    throw;
                }
                catch(Exception err)
                {
                    SendMsg(err.Message);
                    SendMsg(err.StackTrace);
                    Thread.Sleep(10000);
                    throw;
                }
            }
            return null;
        }


        private bool trySelect(string gameid)
        {
            // 设置游戏ID
            var gameIdPath = "//div[@id='LiveGameRoleList']/div/div/div[contains(text(),'游戏ID')]/div/input";
            SetTextElementByXPath(gameIdPath, gameid);
            // 点击查询按钮
            //*[@id="LiveGameRoleList"]/div[1]/div/div[9]/button[1]/span
            var btnPath = "//div[@id='LiveGameRoleList']/div[1]/div/div[9]/button[1]/span[text()='查询']";
            if (TryClickByXPath(btnPath, 2000))
            {
                // 等待查询结果
                var path = "//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]/div[2]/table/tbody/tr/td[2]/div/div/div/div[2]/div[3]/div/span";
                path += "[contains(text(),'" + gameid + "')]";
                var t = FindElementByXPath(path);
                var gid = t.Text.Trim();
                if (gid == gameid)
                {
                    return true;
                }
            }
            return false;
        }

        public List<User> ReadTable(string gameid)
        {
            var table = FindElementByXPath("//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]");
            var tbody = FindElementByXPath(table, ".//tbody[@class='ivu-table-tbody']");

            //List<Head> head = ReadHeadList(table);
            //// fix head
            //for (var i = 0; i < User.Heads.Length && i < head.Count; i++)
            //{
            //    if (head[i].Name == string.Empty)
            //    {
            //        head[i].Name = User.Heads[i];
            //    }
            //}
            //Dictionary<string, string> dicHead = new Dictionary<string, string>(head.Count * 2);
            //foreach (var item in head)
            //{
            //    dicHead.Add(item.Name, item.Tag);
            //}

            // 展开所有显示
            var expandList = FindElementsByXPath(tbody, ".//button/span[text()='显示']");
            for (var i = 0; i < expandList.Count; i++)
            {
                var exBtn = expandList[i];
                if (exBtn.Enabled && exBtn.Displayed)
                {
                    SafeClick(exBtn, 5);
                }
            }

            var users = readUsers(tbody);
            int gameCount = 0;
            for (var i = 0; i < users.Count; i++)
            {
                if (readUserInfo(users[i], gameCount))
                {
                    gameCount++;
                }
            }
            return users;
        }

        // 读取每一项用户信息
        private List<User> readUsers(IWebElement tbody)
        {
            var allRows = FindElementsByXPath(tbody, ".//tr");
            var count = allRows.Count;
            var users = new List<User>(count);
            for (var i = 0; i < count; i++)
            {
                var row = allRows[i];
                var user = User.Create(row);
                if (user != null)
                {
                    users.Add(user);
                }
            }
            return users;
        }

        private bool readUserInfo(User user, int i)
        {
            Console.WriteLine("AppID:" + user.AppId + "; GameId:" + user.GameId);
            Int64 gid;
            if (Int64.TryParse(user.GameId, out gid))
            {
                // 注单(游戏)
                bool r = readGameLog(user, i);
                // 概况(资金)
                bool f = readFunding(user, i);
                return f && r;
            }
            else
            {
                // 用户不存在
                Console.WriteLine("无效的游戏ID：" + user.GameId);
            }
            return false;
        }

        // 出入金概况
        private bool readFunding(User user, int i)
        {
            if (moveToOp(user, i))
            {
                // 点击扩展按钮中的概况
                var xpath = "(//div[@id='timeListBox']/div/div[2]/button[3]/span[text()='概况']/..)[" + (i + 1).ToString() + "]";
                if (TryClickByXPath(xpath, 2000))
                {
                    using (FundingPage gl = new FundingPage(driver, cnf, user.GameId))
                    {
                        user.Funding = gl.Select();
                        return true;
                    }
                }
            }
            return false;
        }

        // 注单(游戏)
        private bool readGameLog(User user, int i)
        {
            if (moveToOp(user, i))
            {
                // 点击扩展按钮中的概况
                var xpath = "(//div[@id='timeListBox']/div/div[2]/button[4]/span[text()='注单']/..)[" + (i + 1).ToString() + "]";
                if (TryClickByXPath(xpath, 2000))
                {
                    using (GameLogPage gl = new GameLogPage(driver, cnf, user.GameId))
                    {
                        user.GameInfo = gl.Select(cnf.GameLogMaxHour);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool moveToOp(User user, int i)
        {
            var opBtn = FindElementByXPath(".//button/span[text()='操作 +']");
            // 移动到【操作+】，显示出扩展按钮
            new Actions(driver).MoveToElement(opBtn).Perform();
            Thread.Sleep(1000);
            return true;
        }
    }
}
