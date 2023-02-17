using System;
using OpenQA.Selenium;

namespace boin
{
    public class WithdrawExpand
    {
        // 支付渠道
        public string PayWay { get; set; } = "";

        // 实名
        public string Name { get; set; } = "";

        // 账号/卡号
        public string CardNo { get; set; } = "";

        // 手续费
        public decimal Gas { get; set; } = 0;

        // 实际到账金额
        public decimal ActualAmount { get; set; } = 0;

        // 转账订单号
        public string TransferOrderId { get; set; } = "";

        // 拒绝理由
        public string Reasons { get; set; } = "";

        // 请求错误信息
        public string RequestError { get; set; } = "";

        // 用户显示错误
        public string UserError { get; set; } = "";

        public WithdrawExpand()
        {

        }

        public void ReadExpand(IWebElement rowEx)
        {
            var ex = readEx(rowEx);
            this.PayWay = Helper.GetValue(ex, "支付渠道：");
            this.Name = Helper.GetValue(ex, "实名：");
            this.CardNo = Helper.GetValue(ex, "账号/卡号：");
            this.Gas = Helper.GetDecimal(ex, "手续费：");
            this.ActualAmount = Helper.GetDecimal(ex, "实际到账金额：");
            this.TransferOrderId = Helper.GetValue(ex, "转账订单号：");
            this.Reasons = Helper.GetValue(ex, "拒绝理由：");
            this.RequestError = Helper.GetValue(ex, "请求错误信息：");
            this.UserError = Helper.GetValue(ex, "用户显示错误：");
        }

        Dictionary<string, string> readEx(IWebElement row)
        {
            var cells = row.FindElements(By.XPath(".//div[@class='ivu-col ivu-col-span-8']"));
            Dictionary<string, string> dic = new Dictionary<string, string>(cells.Count * 3 / 2);
            foreach (var cell in cells)
            {
                var txt = cell.Text;
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
                //var spanList = cell.FindElements(By.XPath(".//span[not (@style='display: none;')]"));
                //var count = spanList.Count;
                //if (count == 0)
                //{
                //    continue;
                //}
                //var key = spanList[0].Text;
                //if (string.IsNullOrEmpty(key))
                //{
                //    continue;
                //}
                //string value = string.Empty;
                //if (count >= 2)
                //{
                //    value = spanList[1].Text;
                //}
                //for (var i = 2; i < count; i++)
                //{
                //    value += "|" + spanList[i].Text;
                //}
                //dic.Add(key, value);
            }
            return dic;
        }
    }
}

