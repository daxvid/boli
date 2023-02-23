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

        public User Select(string gameId)
        {
            GoToPage(1, "用户列表");
            // 设置游戏ID
            var gameIdPath = "//div[@id='LiveGameRoleList']/div/div/div[contains(text(),'游戏ID')]/div/input";
            SetTextElementByXPath(gameIdPath, gameId);
            
            // 点击查询按钮
            //*[@id="LiveGameRoleList"]/div[1]/div/div[9]/button[1]/span
            var btnPath = "//div[@id='LiveGameRoleList']/div[1]/div/div[9]/button[1]/span[text()='查询']";
            FindAndClickByXPath(btnPath, 1000);
            
            // 等待查询结果
            var idPath = "//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]/div[2]/table/tbody/tr/td[2]/div/div/div/div[2]/div[3]/div/span";
            idPath += "[contains(text(),'" + gameId + "')]";
            var xp = By.XPath(idPath);
            var t = FindElement(xp);
            var gid = Helper.ReadString(t);
            if (gid == gameId)
            {
                var table = FindElementByXPath("//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]");
                var tbody = FindElementByXPath(table, ".//tbody[@class='ivu-table-tbody']");
                var row = FindElementByXPath(tbody, ".//tr");
                var user = User.Create(row);
                readUserInfo(user, 0);
                return user;
            }
            throw new MoreSuchElementException(xp, "selectUser", null);
        }

        public List<User> ReadTable()
        {
            var table = FindElementByXPath("//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]");
            var tbody = FindElementByXPath(table, ".//tbody[@class='ivu-table-tbody']");

            // 展开所有显示
            // //*[@id="LiveGameRoleList"]/div[2]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[10]/div/div/button/span
            // //*[@id="LiveGameRoleList"]/div[2]/div[2]/div[1]/div[2]/table/tbody/tr[3]/td[10]/div/div/button/span
            var path = ".//td[10]/div/div/button/span[text()='显示']";
            var expandList = FindElementsByXPath(tbody, path);
            foreach (var exBtn in  expandList)
            {
                if (exBtn.Enabled && exBtn.Displayed)
                {
                    SafeClick(exBtn, 5);
                }
            }
            Thread.Sleep(100);

            var users = readUsers(tbody);
            var gameCount = 0;
            foreach (var user in  users)
            {
                if (readUserInfo(user, gameCount))
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
                readGameLog(user, i);
                // 概况(资金)
                readFunding(user, i);
                return true;
            }
            else
            {
                // 用户不存在
                Console.WriteLine("无效的游戏ID：" + user.GameId);
            }

            return false;
        }

        // 出入金概况
        private void readFunding(User user, int i)
        {
            var xpath = "(//div[@id='timeListBox']/div/div[2]/button[3]/span[text()='概况']/..)[" +
                        (i + 1).ToString() +  "]";
            moveToOp(user, i);
            // 点击扩展按钮中的概况
            FindAndClickByXPath(xpath, 2000);
            user.Funding = SafeExec(() =>
            {
                using (FundingPage gl = new FundingPage(driver, cnf, user.GameId))
                {
                    var funding = gl.Select();
                    return funding;
                }
            });
        }

        // 注单(游戏)
        private void readGameLog(User user, int i)
        {
            var xpath = "(//div[@id='timeListBox']/div/div[2]/button[4]/span[text()='注单']/..)[" +
                        (i + 1).ToString() + "]";
            moveToOp(user, i);
            // 点击扩展按钮中的概况
            FindAndClickByXPath(xpath, 2000);
            user.GameInfo = SafeExec(() =>
            {
                using (GameLogPage gl = new GameLogPage(driver, cnf, user.GameId))
                {
                    var gameInfo = gl.Select(cnf.GameLogMaxHour);
                    return gameInfo;
                }
            });
        }

        private bool moveToOp(User user, int i)
        {
            var opBtn = FindElementByXPath(".//button/span[text()='操作 +']");
            // 移动到【操作+】，显示出扩展按钮
            new Actions(driver).MoveToElement(opBtn).Perform();
            Thread.Sleep(500);
            return true;
        }
    }
}
