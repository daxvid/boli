using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;

namespace boin
{
    public class User
    {

        // 商户
        public string Merchant { get; set; } = "";

        // AppId
        public string AppId { get; set; } = "";

        // 游戏ID
        public string GameId { get; set; } = "";

        // 昵称
        public string NickName { get; set; } = "";

        // 等级
        public string Level { get; set; } = "";

        // 设备
        public string Device { get; set; } = "";

        public GameInfo GameInfo { get; set; }
        public Funding Funding { get; set; }

        public Order Order { get; set; }
        public bool  Pass { get; set; }
        public List<Review.ReviewResult> ReviewResult { get; set; } = null;

        public static string[] Heads = new string[] { "商户" , "用户信息", "等级/设备", "余额", "金流", "贵族",
			"在线时长", "注册时间/最后上线", "注册IP/登录IP", "操作"};

		public User()
		{
		}

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
                user.Merchant = ts[0].Text.Trim(); // 商户

                // 用户信息
                IWebElement userInfo = ts[1]; // 用户信息;
                var spans = userInfo.FindElements(By.XPath(".//div/div/span"));

                user.AppId = spans[1].Text;
                user.GameId = spans[2].Text;

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

                user.AppId = spans[1].Text;
                user.GameId = spans[2].Text;

                span.Msg = "用户:" + user.GameId;

                return user;
            }
		}
	}
}

