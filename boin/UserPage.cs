﻿using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using boin.Review;
using OpenQA.Selenium.Internal;
using System.Linq;

namespace boin
{
    public class UserPage : PageBase
    {

        public UserPage(ChromeDriver driver) : base(driver)
        {
        }


        public bool Open()
        {
            return GoToPage(1, "用户列表");
        }


        public List<User> Select(List<Order> orders)
        {
            List<User> users = new List<User>();
            List<User> tmp = new List<User>();
            foreach (var order in orders)
            {
                var user = Select(order.GameId);
                user.Order = order;
                users.Add(user);
                if (user.Funding.IsSync)
                {
                    Review(user);
                }
                else
                {
                    tmp.Add(user);
                }
            }

            foreach (var user in tmp)
            {
                for (int i = 0; i < 1000; i++)
                {
                    if (user.Funding.IsSync)
                    {
                        Review(user);
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            return users;
        }

        public User Select(string gameid)
        {
            if (trySelect(gameid))
            {
                var users = ReadTable(gameid);
                if (users.Count > 0)
                {
                    return users[0];
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
            TryClickByXPath(btnPath, 2000);
            // 等待查询结果
            var path = "//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]/div[2]/table/tbody/tr/td[2]/div/div/div/div[2]/div[3]/div/span";
            path += "[contains(text(),'" + gameid + "')]";
            var t = FindElementByXPath(path);
            var gid = t.Text.Trim();
            if (gid == gameid)
            {
                return true;
            }
            return false;
        }

        public List<User> ReadTable(string gameid)
        {
            var table = FindElementByXPath("//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]");
            var tbody = FindElementByXPath(table, ".//tbody[@class='ivu-table-tbody']");

            List<Head> head = ReadHeadList(table);
            // fix head
            for (var i = 0; i < User.Heads.Length && i < head.Count; i++)
            {
                if (head[i].Name == "")
                {
                    head[i].Name = User.Heads[i];
                }
            }
            Dictionary<string, string> dicHead = new Dictionary<string, string>(head.Count * 2);
            foreach (var item in head)
            {
                dicHead.Add(item.Name, item.Tag);
            }

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

            var users = readUsers(dicHead, tbody);
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
        private List<User> readUsers(Dictionary<string, string> dicHead, IWebElement tbody)
        {
            var allRows = FindElementsByXPath(tbody, ".//tr");
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


        private bool readUserInfo(User user, int i)
        {
            Console.WriteLine("AppID:" + user.AppId + "; GameId:" + user.GameId);
            Int64 gid;
            if (Int64.TryParse(user.GameId, out gid))
            {
                // 概况(资金)
                bool f = readFunding(user, i);
                // 注单
                bool r = readGameLog(user, i);
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
                    FundingPage gl = new FundingPage(driver);
                    user.Funding = gl.Select(user);
                    return true;
                }
            }
            return false;
        }

        // 注单
        private bool readGameLog(User user, int i)
        {
            if (moveToOp(user, i))
            {
                // 点击扩展按钮中的概况
                var xpath = "(//div[@id='timeListBox']/div/div[2]/button[4]/span[text()='注单']/..)[" + (i + 1).ToString() + "]";
                if (TryClickByXPath(xpath, 2000))
                {
                    GameLogPage gl = new GameLogPage(driver);
                    user.GameLogs = gl.Select(user);
                    return true;
                }
            }
            return false;
        }

        private bool moveToOp(User user, int i)
        {
            var opBtn = FindElementByXPath(".//button/span[text()='操作 +']");
            try
            {
                // 移动到【操作+】，显示出扩展按钮
                new Actions(driver).MoveToElement(opBtn).Perform();
                Thread.Sleep(1000);
                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
            return false;
        }


        private bool Review(User user)
        {
           bool r = ReviewManager.Review(user);
            // 通过
            if (r)
            {

            }
            // 
            else
            {

            }
            return r;
        }
    }
}
