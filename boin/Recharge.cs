﻿using System;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using boin.Util;

namespace boin
{
    // 充值
    public class Recharge
    {

        // 游戏ID	
        public string GameId { get; set; } = string.Empty;

        // 用户昵称
        public string Nickname { get; set; } = string.Empty;

        // 存款人	
        public string Depositor { get; set; } = string.Empty;

        // 订单号
        public string OrderId { get; set; } = string.Empty;

        // 外部订单号
        public string OutsideOrderId { get; set; } = string.Empty;

        // 充值金额
        public Decimal RechargeAmount { get; set; }

        // 首充
        public string FirstRecharge { get; set; } = string.Empty;

        // 实际到账金额
        public Decimal ActualAmount { get; set; }

        // 充值类型
        public string RechargeType { get; set; } = string.Empty;

        // 充值接口(客单充值(不能删)/)
        public string RechargeChannel { get; set; } = string.Empty;

        // VIP期数
        public string VipPeriod { get; set; } = string.Empty;

        // 小费
        public Decimal Tip { get; set; }

        // 取消下注
        public string CancelBet { get; set; } = string.Empty;

        // 时间
        public DateTime Created { get; set; }

        // 说明
        public string Mark { get; set; } = string.Empty;

        public Recharge()
        {
        }

        public bool IsSyncName
        {
            get { return Interlocked.Read(ref nameLocker) == 2; }
        }

        private long nameLocker = 0;

        // 同步姓名
        private void syncName()
        {
            if (Interlocked.CompareExchange(ref nameLocker, 1, 0) == 0)
            {
                var chan = this.RechargeChannel;
                if (string.IsNullOrEmpty(this.Depositor) && chan.Contains("四方")
                  && ((chan.Contains("银联") || chan.Contains("卡卡"))))
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        var depositor = GetRechargeName(this.OutsideOrderId);
                        this.Depositor = depositor;
                        Interlocked.Increment(ref nameLocker);
                    });
                }
                else
                {
                    Interlocked.Increment(ref nameLocker);
                }
            }
        }

        public static string[] Heads = new string[] {"充值账户游戏ID", "用户昵称" , "存款人", "订单号", "外部订单号",
            "充值金额", "首充", "实际到账金额", "充值类型", "充值接口", "VIP期数", "时间", "说明"};

        public static Recharge Create(IWebElement element)
        {
            using (var span = new Span())
            {
                var ts = element.FindElements(By.XPath(".//td"));
                if (ts.Count != Heads.Length)
                {
                    throw new ArgumentException("Recharge Create");
                }

                Recharge log = new Recharge();
                log.GameId = Helper.ReadString(ts[0]); // 充值账户游戏ID
                log.Nickname = Helper.ReadString(ts[1]); //  用户昵称
                log.Depositor = Helper.ReadString(ts[2]); // 存款人
                log.OrderId = Helper.ReadString(ts[3]); // 订单号
                log.OutsideOrderId = Helper.ReadString(ts[4]); // 外部订单号

                log.RechargeAmount = Helper.ReadDecimal(ts[5]); // 充值金额
                log.FirstRecharge = Helper.ReadString(ts[6]); // 首充
                log.ActualAmount = Helper.ReadDecimal(ts[7]); // 实际到账金额
                log.RechargeType = Helper.ReadString(ts[8]); // 充值类型
                log.RechargeChannel = Helper.ReadString(ts[9]); // 充值接口
                log.VipPeriod = Helper.ReadString(ts[10]); // VIP期数
                log.Created = Helper.ReadDateTime(ts[11]); // 时间
                log.Mark = Helper.ReadString(ts[12]); // 说明

                log.syncName();

                span.Msg = "充值:" + log.OrderId;
                return log;
            }
        }

        public static Recharge Create(Dictionary<string, string> head, IWebElement element)
        {
            using (var span = new Span())
            {
                var row = Helper.Ele2Dic(element);

                Recharge log = new Recharge();
                log.GameId = Helper.ReadString(head, "充值账户游戏ID", row);
                log.Nickname = Helper.ReadString(head, "用户昵称", row);
                log.Depositor = Helper.ReadString(head, "存款人", row);
                log.OrderId = Helper.ReadString(head, "订单号", row);
                log.OutsideOrderId = Helper.ReadString(head, "外部订单号", row);

                log.RechargeAmount = Helper.ReadDecimal(head, "充值金额", row);
                log.FirstRecharge = Helper.ReadString(head, "首充", row);
                log.ActualAmount = Helper.ReadDecimal(head, "实际到账金额", row);
                log.RechargeType = Helper.ReadString(head, "充值类型", row);
                log.RechargeChannel = Helper.ReadString(head, "充值接口", row);
                log.VipPeriod = Helper.ReadString(head, "VIP期数", row);
                log.Created = Helper.ReadTime(head, "时间", row);
                log.Mark = Helper.ReadString(head, "说明", row);

                log.syncName();

                span.Msg = "充值:" + log.OrderId;
                return log;
            }
        }

        public static string RechargeHost = "";
        public static string GetRechargeName(string orderId)
        {
            var url = RechargeHost + orderId;
            try
            {
                var name = Cache.GetRecharge(orderId);
                if (name != null)
                {
                    return name;
                }
                HttpClient client = new HttpClient();
                var task = client.GetStringAsync(url);
                var content = task.Result;
                //content = "{ \"userId\": \"1591389834\", \"name\":\"df\", \"orderId\": \"AD7evE7ANDpuXzL2\", \"orderNo\": \"OR1676436469554825\", \"passageNo\": \"\", \"amount\": 300000, \"realAmount\": 300000, \"status\": 2, \"duplicate\": 0, \"createTime\": 1676436469, \"finishTime\": 1676437144, \"callbackStatus\": 5, \"callbackTime\": 1676437144, \"passageName\": \"\\u5361\\u5361\", \"orderTypeName\": \"\\u94f6\\u8054\"}";
                int index = content.IndexOf("\"name\"");
                if (index > 0)
                {
                    int start = content.IndexOf("\"", index + 6, 16);
                    int end = content.IndexOf("\"", start + 1, 64);
                    name = content.Substring(start + 1, end - start - 1);
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = System.Text.RegularExpressions.Regex.Unescape(name);
                    }
                    Cache.SaveRecharge(orderId, name);
                    return name;
                }
            }
            catch{}
            return string.Empty;
        }
    }
}


