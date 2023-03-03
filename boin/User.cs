using System;
using System.Text;
using OpenQA.Selenium;
using boin.Util;

namespace boin;

public class User
{
    // 商户
    public string Merchant { get; set; } = string.Empty;

    // AppId
    public string AppId { get; set; } = string.Empty;

    // 游戏ID
    public string GameId { get; set; } = string.Empty;

    // 昵称
    public string NickName { get; set; } = string.Empty;

    // 等级
    public string Level { get; set; } = string.Empty;

    // 设备
    public string Device { get; set; } = string.Empty;

    // 用户备注(如果包含 冻结/排查 时暂不审单)
    public string Remark { get; set; } = string.Empty;
    
    // 注册时间
    public DateTime Created { get; set; }

    public GameInfo GameInfo { get; set; }
    public Funding Funding { get; set; }

    public Order Order { get; set; }

    public static string[] Heads = new string[]
    {
        "商户", "用户信息", "等级/设备", "余额", "金流", "贵族",
        "在线时长", "注册时间/最后上线", "注册IP/登录IP", "操作"
    };

    public User()
    {
    }

    public bool IsNewUser()
    {
        var day = DateTime.Now.Subtract(this.Created).TotalDays;
        return day < 30;
    }
    

    // 是否玩了指定的游戏
    public static User Create(IWebElement element)
    {
        using (var span = new Span())
        {
            var ts = element.FindElements(By.XPath(".//td"));
            if (ts.Count != Heads.Length)
            {
                throw new ArgumentException("User Create");
            }

            User user = new User();
            user.Merchant = Helper.ReadString(ts[0]); // 商户

            // 用户信息
            var spans = ts[1].FindElements(By.XPath(".//div/div/span"));
            user.AppId = Helper.ReadString(spans[1]);
            user.GameId = Helper.ReadString(spans[2]);

            // 注册时间/最后上线
            var c = ts[7].FindElement(By.XPath(".//div/div/span[1]"));
            user.Created = Helper.ReadShortTime(c);

            span.Msg = "用户:" + user.GameId;
            return user;
        }
    }

    public static User Create(Dictionary<string, string> head, IWebElement element)
    {
        using (var span = new Span())
        {
            var row = Helper.Ele2Dic(element);
            User user = new User();
            user.Merchant = Helper.ReadString(head, "商户", row);

            // 用户信息
            IWebElement userInfo = row[head["用户信息"]];
            var spans = userInfo.FindElements(By.XPath(".//div/div/span"));

            user.AppId = Helper.ReadString(spans[1]);
            user.GameId = Helper.ReadString(spans[2]);

            span.Msg = "用户:" + user.GameId;

            return user;
        }
    }
}

