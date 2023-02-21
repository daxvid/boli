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
    public class GameBindPage : PageBase
    {

        public GameBindPage(ChromeDriver driver, AppConfig cnf) : base(driver, cnf)
        {
        }


        public override bool Open()
        {
            return GoToPage(1, "绑定管理");
        }

        public List<GameBind> Select(string gameid)
        {
            while (true)
            {
                try
                {
                    int t = trySelect(gameid);
                    if (t >= 0)
                    {
                        if (t == 0)
                        {
                            return new List<GameBind>();
                        }
                        var binds = ReadTable(gameid);
                        return binds;
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    Thread.Sleep(1000);
                }
                catch (ElementNotInteractableException) { }
                catch (WebDriverException)
                {
                    throw;
                }
                catch (Exception err)
                {
                    SendMsg(err.Message);
                    SendMsg(err.StackTrace);
                    Thread.Sleep(10000);
                    throw;
                }
            }
            return null;
        }

        private int trySelect(string gameid)
        {
            // 设置游戏ID
            // //*[@id="GameBindList"]/div[1]/div[2]/input
            var gameIdPath = "//div[@id='GameBindList']/div[1]/div[2]/input[@placeholder='请输入查询游戏ID']";
            SetTextElementByXPath(gameIdPath, gameid);
            // 点击查询按钮
            // //*[@id="GameBindList"]/div[1]/button/span
            var btnPath = "//div[@id='GameBindList']/div[1]/button/span[text()='查询']";
            if (TryClickByXPath(btnPath, 2000))
            {
                // 暂无数据 //*[@id="GameBindList"]/div[2]/div[1]/div[3]/table/tbody/tr/td/span
                var pathNone = "div[@id='GameBindList']/div[2]/div[1]/div[3]/table/tbody/tr/td/span[text()='暂无数据']";
                if (FindElementsByXPath(pathNone).Count == 1)
                {
                    return 0;
                }

                // 等待查询结果
                // //*[@id="GameBindList"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[2]/div/div/div/a/span
                // //*[@id="GameBindList"]/div[2]/div[1]/div[2]/table/tbody/tr[2]/td[2]/div/div/div/a/span
                var path = "//div[@id='GameBindList']/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[2]/div/div/div/a/span";
                path += "[contains(text(),'" + gameid + "')]";
                var t = FindElementsByXPath(path);
                return t.Count;
            }
            return -1;
        }

        public List<GameBind> ReadTable(string gameid)
        {
            var table = FindElementByXPath("//div[@id='GameBindList']/div[2]/div[1]/div[2]/table");
            var tbody = FindElementByXPath(table, ".//tbody[@class='ivu-table-tbody']");

            //  点开所有查看实名
            // //*[@id="GameBindList"]/div[2]/div[1]/div[2]/table/tbody/tr[4]/td[5]/div/div/div/button/span
            // //*[@id="GameBindList"]/div[2]/div[1]/div[2]/table/tbody/tr[5]/td[5]/div/div/div/button/span
            var expandList = FindElementsByXPath(tbody, ".//td[5]/div/div/div/button/span[text()='查看实名']");
            for (var i = 0; i < expandList.Count; i++)
            {
                var exBtn = expandList[i];
                if (exBtn.Enabled && exBtn.Displayed)
                {
                    SafeClick(exBtn, 5);
                }
            }
            Thread.Sleep(200);

            var binds = readBinds(tbody);
            return binds;
        }

        // 读取每一项用户信息
        private List<GameBind> readBinds(IWebElement tbody)
        {
            var allRows = FindElementsByXPath(tbody, ".//tr");
            var count = allRows.Count;
            var Binds = new List<GameBind>(count);
            for (var i = 0; i < count; i++)
            {
                var row = allRows[i];
                var bind = GameBind.Create(row);
                if (bind != null)
                {
                    Binds.Add(bind);
                }
            }
            return Binds;
        }

        public GameBind Select(string gameid, string cardNo)
        {
            while (true)
            {
                try
                {
                    if (trySelect(gameid, cardNo))
                    {
                        var binds = ReadTable(gameid);
                        if (binds.Count > 0)
                        {
                            return binds[0];
                        }
                    }
                    return null;
                }
                catch (WebDriverTimeoutException)
                {
                    Thread.Sleep(1000);
                }
                catch (ElementNotInteractableException) { }
                catch (WebDriverException)
                {
                    throw;
                }
                catch (Exception err)
                {
                    SendMsg(err.Message);
                    SendMsg(err.StackTrace);
                    Thread.Sleep(10000);
                    throw;
                }
            }
            return null;
        }

        private bool trySelect(string gameid, string cardNo)
        {
            // 设置游戏ID
            // //*[@id="GameBindList"]/div[1]/div[2]/input
            var gameIdPath = "//div[@id='GameBindList']/div[1]/div[2]/input[@placeholder='请输入查询游戏ID']";
            SetTextElementByXPath(gameIdPath, gameid);

            // 设置账号
            // //*[@id="GameBindList"]/div[1]/div[4]/input
            var cardNoPath = "//div[@id='GameBindList']/div[1]/div[4]/input[@placeholder='请输入查询账号']";
            SetTextElementByXPath(cardNoPath, cardNo);

            // 点击查询按钮
            // //*[@id="GameBindList"]/div[1]/button/span
            var btnPath = "//div[@id='GameBindList']/div[1]/button/span[text()='查询']";
            if (TryClickByXPath(btnPath, 2000))
            {
                // 暂无数据 //*[@id="GameBindList"]/div[2]/div[1]/div[3]/table/tbody/tr/td/span
                var pathNone = "div[@id='GameBindList']/div[2]/div[1]/div[3]/table/tbody/tr/td/span[text()='暂无数据']";
                if (FindElementsByXPath(pathNone).Count == 1)
                {
                    return false;
                }

                // 等待查询结果
                // //*[@id="GameBindList"]/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[6]/div/span
                var path = "//div[@id='GameBindList']/div[2]/div[1]/div[2]/table/tbody/tr[1]/td[6]/div/span";
                path += "[text()='" + cardNo + "']";
                var t = FindElementByXPath(path);
                var gid = Helper.ReadString(t);
                if (gid == cardNo)
                {
                    return true;
                }
            }
            return false;
        }



    }
}
