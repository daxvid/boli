using System;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;

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

        public bool IsSync
        {
            get { return Interlocked.Read(ref sync) == 2; }
        }

        private long sync = 0;

        // 同步姓名
        private void syncName()
        {
            if (Interlocked.CompareExchange(ref sync, 0, 1) == 0)
            {
                if ((string.IsNullOrEmpty(this.Depositor))
                    && (this.RechargeChannel.Contains("银联") && this.RechargeChannel.Contains("四方")))
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        var depositor = GetRechargeName(this.OutsideOrderId);
                        this.Depositor = depositor;
                        Interlocked.Increment(ref sync);
                    });
                }
                else
                {
                    Interlocked.Increment(ref sync);
                }
            }
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
            if(log.Depositor == "--")
            {
                log.Depositor = string.Empty;
            }
            log.syncName();

            return log;
        }

        public static string GetRechargeName(string orderId)
        {
            const string host = "http://man.xyyj315.com/order/query_order/?orderId=";
            var url = host + orderId;
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
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
            return string.Empty;
        }
    }
}


