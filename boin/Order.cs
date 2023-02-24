using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Util;

namespace boin
{
    // 提现订单
    public class Order: WithdrawExpand
    {  
        // 订单号
        public string OrderID { get; set; } = string.Empty;

        // 创建时间
        public DateTime Created { get; set; }

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

        // 操作人
        public string Operator { get; set; } = string.Empty;

        // 提现备注
        public string Remark { get; set; } = string.Empty;

        // 订单状态
        public string Status { get; set; } = string.Empty;

        public bool Pass { get; set; }
        public string ReviewMsg { get; set; } = string.Empty;
        public List<Review.ReviewResult> ReviewResult { get; set; } = null;

        public GameBind Bind { get; set; }
        
        // 是否已处理
        public bool Processed { get; set; } 

        public Order()
        {
        }

        public string ReviewNote()
        {
            StringBuilder sb = new StringBuilder(1024);
            sb.Append("card:").AppendLine(this.CardNo);
            if (!string.IsNullOrEmpty(ReviewMsg))
            {
                sb.Append(OrderID).Append(":").AppendLine(ReviewMsg);
            }
            if (ReviewResult != null)
            {
                foreach (var r in ReviewResult)
                {
                    if (r.Code != 0)
                    {
                        sb.Append(r.Code).Append(":");
                    }
                    sb.AppendLine(r.Msg);
                }
            }
            var m = sb.ToString();
            return m;
        }


        public static string[] Heads = new string[] {string.Empty, "订单号" , "发起时间", "到账时间", "游戏ID",
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
                order.OrderID = Helper.ReadString(ts[1]); // 订单号
                order.Created = Helper.ReadShortTime(ts[2]); // 发起时间
                order.TimeToAccount = Helper.ReadString(ts[3]); // 到账时间
                order.GameId = Helper.ReadString(ts[4]); // 游戏ID"
                order.NickName = Helper.ReadString(ts[5]); // 用户昵称
                order.Amount = Helper.ReadDecimal(ts[6]); // 提现金额
                order.Way = Helper.ReadString(ts[7]); // 通道
                order.Review = Helper.ReadString(ts[8]); // 状态
                order.Transfer = Helper.ReadString(ts[9]); // 转账
                order.Operating = Helper.ReadString(ts[10]); // 操作类型
                order.Operator = Helper.ReadString(ts[11]); //  操作人
                order.Remark = Helper.ReadString(ts[12]); // 提现备注
                order.Status = Helper.ReadString(ts[13]); //  操作
                order.ReadExpand(rowEx, order.Way == "银行卡");

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
                //order.Created = Helper.ReadString(head, "发起时间", row);
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

                order.ReadExpand(rowEx, order.Way == "银行卡");
                

                span.Msg = "订单:" + order.OrderID;
                return order;
            }
        }
    }
}

