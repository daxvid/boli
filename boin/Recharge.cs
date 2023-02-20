using System;
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

        // 充值接口
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
                if ((string.IsNullOrEmpty(this.Depositor))
                    && (this.RechargeChannel.Contains("银联") && this.RechargeChannel.Contains("四方")))
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
                log.GameId = ts[0].Text.Trim(); // 充值账户游戏ID
                log.Nickname = ts[1].Text.Trim(); //  用户昵称
                log.Depositor = ts[2].Text.Trim(); // 存款人
                log.OrderId = ts[3].Text.Trim(); // 订单号
                log.OutsideOrderId = ts[4].Text.Trim(); // 外部订单号

                log.RechargeAmount = Helper.ReadDecimal(ts[5]); // 充值金额
                log.FirstRecharge = ts[6].Text.Trim(); // 首充
                log.ActualAmount = Helper.ReadDecimal(ts[7]); // 实际到账金额
                log.RechargeType = ts[8].Text.Trim(); // 充值类型
                log.RechargeChannel = ts[9].Text.Trim(); // 充值接口
                log.VipPeriod = ts[10].Text.Trim(); // VIP期数
                log.Created = Helper.ReadDateTime(ts[11]); // 时间
                log.Mark = ts[12].Text.Trim(); // 说明
                if (log.Depositor == "--")
                {
                    log.Depositor = string.Empty;
                }
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
                if (log.Depositor == "--")
                {
                    log.Depositor = string.Empty;
                }
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
                HttpClient client = new HttpClient();
                var task = client.GetStringAsync(url);
                var content = task.Result;
                //content = "{ \"userId\": \"1591389834\", \"name\":\"df\", \"orderId\": \"AD7evE7ANDpuXzL2\", \"orderNo\": \"OR1676436469554825\", \"passageNo\": \"\", \"amount\": 300000, \"realAmount\": 300000, \"status\": 2, \"duplicate\": 0, \"createTime\": 1676436469, \"finishTime\": 1676437144, \"callbackStatus\": 5, \"callbackTime\": 1676437144, \"passageName\": \"\\u5361\\u5361\", \"orderTypeName\": \"\\u94f6\\u8054\"}";
                int index = content.IndexOf("\"name\"");
                if (index > 0)
                {
                    int start = content.IndexOf("\"", index + 6, 16);
                    int end = content.IndexOf("\"", start + 1, 64);
                    var name = content.Substring(start + 1, end - start - 1);
                    return name;
                }
            }
            catch{}
            return string.Empty;
        }
    }
}


