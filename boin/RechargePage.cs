﻿using System;
using System.Reflection.Emit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
	// 充值记录
	public class RechargePage : PageBase
    {

        string gameId;
        string path;
        public RechargePage(ChromeDriver driver, AppConfig cnf, string gameId) : base(driver, cnf)
        {
            this.maxPage = cnf.RechargeMaxPage;
            this.gameId = gameId;
            this.path = "//div[text()='用户充值详情' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";

        }

        public override bool Close()
        {
            // 关闭窗口
            var table = FindElementByXPath(path);
            return Helper.SafeClose(driver, table);
        }

        private IWebElement getCurrentTable(int page)
        {
            var pagePath = ".//div/span[@class='marginRight' and contains(text(),'第" + page.ToString() + "页')]";
            var t = FindElementByXPath(path);
            var pageTag = FindElementByXPath(t, pagePath);
            return t;
        }

        public List<Recharge> Select()
        {
            var table = getCurrentTable(1);

            //// 日期区间
            //var timeRang = gameLog.FindElement(By.XPath(".//div[@class='ivu-date-picker-rel']/div/input[@placeholder='开始时间-结束时间']"));
            //Helper.SetTimeRang(24, timeRang);
            //// 点击查询按钮;
            //var sub = gameLog.FindElement(By.XPath(".//button/span[text()='查询']"));
            //sub.Click();
            //Thread.Sleep(2000);

            // 在线充值次数/后台充值次数/等
            var items = FindElementsByXPath(table, ".//div[@class='countSty']/span[contains(text(),'在线充值次数：')]/../span");
            List<string> onlineRechargeList = new List<string>();
            foreach(var item in items)
            {
                onlineRechargeList.Add(item.Text);
            }
            return ReadRechargeLog(table);
        }

        // 读取日志数据 ivu-modal-content/ivu-modal-body
        private List<Recharge> ReadRechargeLog(IWebElement table)
        {
            // table = ivu-modal-content
            var bodyPath = ".//tbody[@class='ivu-table-tbody']";
            var tbody = FindElementByXPath(table,bodyPath);
            var bodys = new List<IWebElement>() { tbody };
            var tables = new List<IWebElement>() { table };
            var allLogs = ReadLogs(tbody, 1);

            for (var page = 2; page <= maxPage; page++)
            {
                // 去到下一页
                if (!GoToNextPage(table))
                {
                    break;
                }

                table = getCurrentTable(page);
                tbody = FindElementByXPath(table,bodyPath);
                bodys.Add(tbody);
                tables.Add(table);

                var logs = ReadLogs(tbody, page);
                allLogs.AddRange(logs);
            }
            return allLogs;
        }

        // 读取每一项日志信息
        private List<Recharge> ReadLogs(IWebElement tbody, int page)
        {
            // 点开所有的查询按钮
            var selBtns =  FindElementsByXPath(tbody,".//td/div/div/div/button/span[contains(text(),'查询')]/..");
            foreach (var sel in selBtns)
            {
                Helper.TryClick(wait, sel);
            }

            var allRows = FindElementsByXPath(tbody, ".//tr");
            var count = allRows.Count;
            var logs = new List<Recharge>(count);
            for (var i = 0; i < count; i++)
            {
                var row = allRows[i];
                var log = Recharge.Create(row);
                if (log != null)
                {
                    logs.Add(log);
                }
            }
            return logs;
        }
    }
}
