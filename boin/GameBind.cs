using System;
using OpenQA.Selenium;
using boin.Util;

namespace boin;

public class GameBind
{
    // 游戏ID
    public string GameId { get; set; } = string.Empty;

    // 支付渠道
    public string PayChan { get; set; } = string.Empty;

    // 银行名称
    public string BankName { get; set; } = string.Empty;

    // 收款人
    public string Payee { get; set; } = string.Empty;

    // 收款账号/卡号
    public string CardNo { get; set; } = string.Empty;

    // 开户支行
    public string BankDeposit { get; set; } = string.Empty;

    // 创建时间
    public DateTime Created { get; set; }

    // 备注
    public string Remark { get; set; } = string.Empty;


    public static string[] Heads = new string[]
    {
        "", "游戏ID", "支付渠道", "银行名称", "收款人", "收款账号/卡号", "开户支行",
        "创建时间", "备注", "操作"
    };

    public GameBind()
    {
    }

    public bool IsNewBind()
    {
        var day = DateTime.Now.Subtract(this.Created).TotalDays;
        return day < 30;
    }

    public static GameBind Create(IWebElement element)
    {
        using (var span = new Span())
        {
            var ts = element.FindElements(By.XPath(".//td"));
            if (ts.Count != Heads.Length)
            {
                throw new ArgumentException("GameBind Create");
            }

            GameBind bind = new GameBind();
            bind.GameId = Helper.ReadString(ts[1]); // 商户
            bind.PayChan = Helper.ReadString(ts[2]); // 支付渠道
            bind.BankName = Helper.ReadString(ts[3]); // 银行名称
            bind.Payee = Helper.ReadString(ts[4]); // 收款人
            bind.CardNo = Helper.ReadString(ts[5]); // 收款账号/卡号
            bind.BankDeposit = Helper.ReadString(ts[6]); // 开户支行
            bind.Created = Helper.ReadDateTime(ts[7]); // 创建时间
            bind.Remark = Helper.ReadString(ts[8]); // 备注

            span.Msg = "绑定:" + bind.GameId;
            return bind;
        }
    }
}

