﻿using System;
using OpenQA.Selenium;

namespace boin
{
    public class Recharge
    {

        // 游戏ID	
        public string GameId { get; set; } = "";

        // 用户昵称
        public string Nickname { get; set; } = "";

        // 存款人	
        public string Depositor { get; set; } = "";

        // 订单号
        public string OrderId { get; set; } = "";

        // 外部订单号
        public string OutsideOrderId { get; set; } = "";

        // 充值金额
        public Decimal RechargeAmount { get; set; }

        // 首充
        public string FirstRecharge { get; set; } = "";

        // 实际到账金额
        public Decimal ActualAmount { get; set; }

        // 充值类型
        public string RechargeType { get; set; } = "";

        // 充值接口
        public string RechargeChannel { get; set; } = "";

        // VIP期数
        public string VipPeriod { get; set; } = "";

        // 小费
        public Decimal Tip { get; set; }

        // 取消下注
        public string CancelBet { get; set; } = "";

        // 时间
        public DateTime Created { get; set; }

        // 说明
        public string Mark { get; set; } = "";

        public Recharge()
        {
        }


        public static Recharge Create(Dictionary<string, string> head, IWebElement element)
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

            // 
            return log;
        }
    }
}


