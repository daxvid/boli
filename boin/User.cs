using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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


        public static string[] Heads = new string[] { "商户" , "用户信息", "等级/设备", "余额", "金流", "贵族",
			"在线时长", "注册时间/最后上线", "注册IP/登录IP", "操作"};

		public User()
		{
		}


		public static User Create(Dictionary<string, string> head, IWebElement element)
		{
            Dictionary<string, IWebElement> row = new Dictionary<string, IWebElement>(head.Count * 2);
            var tdList = element.FindElements(By.XPath(".//td"));
            foreach (var td in tdList)
            {
                var key = td.GetAttribute("class");
                row.Add(key, td);
            }

            User user = new User();
            
            user.Merchant = Table.ReadString(head, "商户", row);

            // 用户信息
            IWebElement userInfo = row[head["用户信息"]];
            var spans = userInfo.FindElements(By.XPath(".//div/div/span"));

            user.AppId = spans[1].Text;
            user.GameId = spans[2].Text;
            return user;
		}
	}
}

