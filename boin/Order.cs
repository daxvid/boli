using System;
using System.Collections.Generic;
using System.Security.Principal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace boin
{
    // 提现订单
    public class Order: WithdrawExpand
    {
        // 订单号
        public string OrderID { get; set; } = "";

        // 创建时间
        public string Created { get; set; } = "";

        // 到账时间
        public string TimeToAccount { get; set; } = "";

        // 游戏ID
        public string GameId { get; set; } = "";

        // 昵称
        public string NickName { get; set; } = "";

        // 提现金额
        public decimal Amount { get; set; } = 0;

        // 通道
        public string Way { get; set; } = "";

        // 审核状态
        public string Review { get; set; } = "";

        // 转账状态
        public string Transfer { get; set; } = "";

        // 操作类型
        public string Operating { get; set; } = "";

        // 操作人
        public string Operator { get; set; } = "";

        // 提现备注
        public string Remark { get; set; } = "";

        // 订单状态
        public string Status { get; set; } = "";

        public Order()
        {
        }

        public static string[] Heads = new string[] {"", "订单号" , "发起时间", "到账时间", "游戏ID",
            "用户昵称", "提现金额", "通道", "状态", "转账", "操作类型", "操作人", "提现备注", "操作" };

        public static Order Create(IWebElement element, IWebElement rowEx)
        {
            using (var span = new Span())
            {
                var ts = element.FindElements(By.XPath(".//td"));
                if (ts.Count != Heads.Length)
                {
                    throw new ArgumentException("Order Create");
                }
                Order order = new Order();
                order.OrderID = ts[1].Text.Trim(); // 订单号
                order.Created = ts[2].Text.Trim(); // 发起时间
                order.TimeToAccount = ts[3].Text.Trim(); // 到账时间
                order.GameId = ts[4].Text.Trim(); // 游戏ID"
                order.NickName = ts[5].Text.Trim(); // 用户昵称
                order.Amount = decimal.Parse(ts[6].Text.Trim()); // 提现金额
                order.Way = ts[7].Text.Trim(); // 通道
                order.Review = ts[8].Text.Trim(); // 状态
                order.Transfer = ts[9].Text.Trim(); // 转账
                order.Operating = ts[10].Text.Trim(); // 操作类型
                order.Operator = ts[11].Text.Trim(); //  操作人
                order.Remark = ts[12].Text.Trim(); // 提现备注
                order.Status = ts[13].Text.Trim(); //  操作

                 order.ReadExpand(rowEx);

                span.Msg = "订单:" + order.OrderID;
                return order;
            }
        }

        public static Order Create(Dictionary<string, string> head, IWebElement element, IWebElement rowEx)
        {
            using (var span = new Span())
            {
                var row = Helper.Ele2Dic(element);
                var tdList = element.FindElements(By.XPath(".//td"));

                Order order = new Order();
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
                order.Operator = Helper.ReadString(head, "操作人", row);
                order.Remark = Helper.ReadString(head, "提现备注", row);
                order.Status = Helper.ReadString(head, "操作", row);

                order.ReadExpand(rowEx);

                span.Msg = "订单:" + order.OrderID;
                return order;
            }
        }
    }
}

