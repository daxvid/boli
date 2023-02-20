using System;
using System.Collections.Generic;
using System.Security.Principal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Telegram.BotAPI.AvailableTypes;
using boin.Util;

namespace boin
{
    public class Withdraw : WithdrawExpand
    {
        // 订单号
        public string OrderID { get; set; } = string.Empty;

        // 发起时间
        public string Created { get; set; } = string.Empty;

        // 到账时间
        public string TimeToAccount { get; set; } = string.Empty;

        // 游戏ID
        public string GameId { get; set; } = string.Empty;

        // 昵称
        public string NickName { get; set; } = string.Empty;

        // 提现金额
        public decimal Amount { get; set; } = 0;

        // 通道
        public string Way { get; set; } = string.Empty;

        // 审核状态
        public string Review { get; set; } = string.Empty;

        // 转账状态
        public string Transfer { get; set; } = string.Empty;

        // 操作类型
        public string Operating { get; set; } = string.Empty;

        public Withdraw()
        {
        }

        public static string[] Heads = new string[] {string.Empty, "订单号" , "发起时间", "到账时间", "游戏ID", "用户昵称", "提现金额",
            "通道", "状态", "转账", "操作类型"};


        public static Withdraw Create(IWebElement element, IWebElement rowEx)
        {
            using (var span = new Span())
            {
                var row = Helper.Ele2Dic(element);
                var ts = element.FindElements(By.XPath(".//td"));
                if (ts.Count != Heads.Length)
                {
                    throw new ArgumentException("Withdraw Create");
                }
                Withdraw order = new Withdraw();
                order.OrderID = ts[1].Text.Trim(); // 订单号
                order.Created = ts[2].Text.Trim(); // 发起时间
                order.TimeToAccount = ts[3].Text.Trim(); // 到账时间
                order.GameId = ts[4].Text.Trim(); // 游戏ID
                order.NickName = ts[5].Text.Trim(); // 用户昵称
                order.Amount = Helper.ReadDecimal(ts[6]); // 提现金额
                order.Way = ts[7].Text.Trim(); // 通道
                order.Review = ts[8].Text.Trim(); // 状态
                order.Transfer = ts[9].Text.Trim(); // 转账
                order.Operating = ts[10].Text.Trim(); // 操作类型

                order.ReadExpand(rowEx, false);

                span.Msg = "提现:" + order.OrderID;
                return order;
            }
        }

        public static Withdraw Create(Dictionary<string, string> head, IWebElement element, IWebElement rowEx)
        {
            using (var span = new Span())
            {
                var row = Helper.Ele2Dic(element);

                Withdraw order = new Withdraw();
                order.OrderID = Helper.ReadString(head, "订单号", row);
                order.Created = Helper.ReadString(head, "发起时间", row);
                order.TimeToAccount = Helper.ReadString(head, "到账时间", row);
                order.GameId = Helper.ReadString(head, "游戏ID", row);
                order.NickName = Helper.ReadString(head, "用户昵称", row);
                order.Amount = Helper.ReadDecimal(head, "提现金额", row);
                order.Way = Helper.ReadString(head, "通道", row);
                order.Review = Helper.ReadString(head, "状态", row);
                order.Transfer = Helper.ReadString(head, "转账", row);
                order.Operating = Helper.ReadString(head, "操作类型", row);

                order.ReadExpand(rowEx, false);

                span.Msg = "提现:" + order.OrderID;
                return order;
            }
        }
    }
}

