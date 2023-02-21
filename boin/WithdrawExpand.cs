using System;
using OpenQA.Selenium;
using boin.Util;

namespace boin
{
    public class BankCardInfo
    {
        // 卡类型。值：DC: "储蓄卡",CC: "信用卡",SCC: "准贷记卡",PC: "预付费卡"
        public string cardType = string.Empty;
        // 银行代码
        public string bank = string.Empty;
        // 卡号
        public string key = string.Empty;
        // 有效性，是否正确有效。值：true为是，false为否。
        public bool validated;
        // 银行卡状态。值：ok，no。
        public string stat = string.Empty;
        // 
        public List<Dictionary<string, string>> messages;
    }

    public class WithdrawExpand
    {
        // 支付渠道
        public string PayChan { get; set; } = string.Empty;

        // 实名(收款人)
        public string Payee { get; set; } = string.Empty;

        // 账号/卡号
        public string CardNo { get; set; } = string.Empty;

        // 手续费
        public decimal Fee { get; set; } = 0;

        // 实际到账金额
        public decimal ActualAmount { get; set; } = 0;

        // 转账订单号
        public string TransferOrderId { get; set; } = string.Empty;

        // 拒绝理由
        public string Reasons { get; set; } = string.Empty;

        // 请求错误信息
        public string RequestError { get; set; } = string.Empty;

        // 用户显示错误
        public string UserError { get; set; } = string.Empty;

        public WithdrawExpand()
        {

        }

        public BankCardInfo BankCardInfo { get; set; }
        // 开户行名称
        public string BankName { get; set; } = string.Empty;

        public bool IsSyncName
        {
            get { return Interlocked.Read(ref nameLocker) == 2; }
        }

        private long nameLocker = 0;

        // 同步银行卡名称
        protected void syncName()
        {
            if (Interlocked.CompareExchange(ref nameLocker, 1, 0) == 0)
            {
                if ((string.IsNullOrEmpty(this.BankName)))
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        //var bankName =  GetRechargeName(this.CardNo);
                        var info = BankUtil.GetBankInfo(this.CardNo);
                        this.BankCardInfo = info;
                        this.BankName = BankUtil.GetNameOfBank(info.bank);
                        Interlocked.Increment(ref nameLocker);
                    });
                }
                else
                {
                    Interlocked.Increment(ref nameLocker);
                }
            }
        }


        public void ReadExpand(IWebElement rowEx, bool readBankInfo)
        {
            var ex = readEx(rowEx);
            this.PayChan = Helper.GetValue(ex, "支付渠道：");
            this.Payee = Helper.GetValue(ex, "实名：");
            this.CardNo = Helper.GetValue(ex, "账号/卡号：");
            this.Fee = Helper.GetDecimal(ex, "手续费：");
            this.ActualAmount = Helper.GetDecimal(ex, "实际到账金额：");
            this.TransferOrderId = Helper.GetValue(ex, "转账订单号：");
            this.Reasons = Helper.GetValue(ex, "拒绝理由：");
            this.RequestError = Helper.GetValue(ex, "请求错误信息：");
            this.UserError = Helper.GetValue(ex, "用户显示错误：");
            if (readBankInfo)
            {
                syncName();
            }
        }

        Dictionary<string, string> readEx(IWebElement row)
        {
            var cells = row.FindElements(By.XPath(".//div[@class='ivu-col ivu-col-span-8']"));
            Dictionary<string, string> dic = new Dictionary<string, string>(cells.Count * 3 / 2);
            foreach (var cell in cells)
            {
                var txt = Helper.ReadString(cell);
                int index = txt.IndexOf('：');
                if (index > 0 && index < txt.Length - 1)
                {
                    var k = txt.Substring(0, index + 1);
                    var v = txt.Substring(index + 1);
                    dic.Add(k, v);
                }
                else
                {
                    if(!string.IsNullOrEmpty(txt))
                    {
                        dic.TryAdd(txt, string.Empty);
                    }
                }
            }
            return dic;
        }
    }
}

