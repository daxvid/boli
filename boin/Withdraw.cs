﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace boin
{
    public class Withdraw
    {
        // 订单号
        public string OrderID { get; set; } = "";

        // 发起时间
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

        // 实名
        public string Name { get; set; } = "";

        // 账号/卡号：
        public string CardNo { get; set; } = "";

        // 实际到账金额
        public decimal ActualAmount { get; set; } = 0;

        // 转账订单号
        public string TransferOrderId { get; set; } = "";

        // 请求错误信息
        public string RequestError { get; set; } = "";

        // 用户显示错误
        public string UserError { get; set; } = "";

        public Withdraw()
        {
        }

        public static string[] Heads = new string[] { "订单号" , "发起时间", "到账时间", "游戏ID", "用户昵称", "提现金额",
            "通道", "状态", "转账", "操作类型"};


        public static Withdraw Create(Dictionary<string, string> head, IWebElement element, IWebElement rowEx)
        {
            var row = Table.Ele2Dic(element);

            Withdraw order = new Withdraw();
            order.OrderID = Table.ReadString(head, "订单号", row);
            order.Created = Table.ReadString(head, "发起时间", row);
            order.TimeToAccount = Table.ReadString(head, "到账时间", row);
            order.GameId = Table.ReadString(head, "游戏ID", row);
            order.NickName = Table.ReadString(head, "用户昵称", row);
            order.Amount = Table.ReadDecimal(head, "提现金额", row);
            order.Way = Table.ReadString(head, "通道", row);
            order.Review = Table.ReadString(head, "状态", row);
            order.Transfer = Table.ReadString(head, "转账", row);
            order.Operating = Table.ReadString(head, "操作类型", row);

            var ex = readEx(rowEx);
            //order.PayWay = Table.GetValue(ex, "支付渠道：");
            order.Name = Table.GetValue(ex, "实名：");
            order.CardNo = Table.GetValue(ex, "账号/卡号：");
            //order.Gas = Table.GetDecimal(ex, "手续费：");
            order.ActualAmount = Table.GetDecimal(ex, "实际到账金额：");
            order.TransferOrderId = Table.GetValue(ex, "转账订单号：");
            //order.Reasons = Table.GetValue(ex, "拒绝理由：");
            order.RequestError = Table.GetValue(ex, "请求错误信息：");
            order.UserError = Table.GetValue(ex, "用户显示错误：");
            return order;
        }

        static Dictionary<string, string> readEx(IWebElement rowEx)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            var cells = rowEx.FindElements(By.ClassName("ivu-col"));
            foreach (var cell in cells)
            {
                var spanList = cell.FindElements(By.TagName("span"));
                if (spanList.Count > 0)
                {
                    var key = spanList[0].Text;
                    string value = string.Empty;
                    for (var i = 1; i < spanList.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            value += "|";
                        }
                        value += spanList[i].Text;
                    }
                    dic.Add(key, value);
                }
            }

            return dic;
        }

    }
}

