using System;
using OpenQA.Selenium;

namespace boin
{
	public class Helper
	{

        public static void SetTimeRang(int hour, IWebElement et)
        {
            et.Click();
            et.SendKeys(Keys.Control + "a");
            et.SendKeys(Keys.Delete);
            et.SendKeys(Keys.Command + "a");
            et.SendKeys(Keys.Delete);
            Thread.Sleep(20);

            var now = DateTime.Now;
            string start = now.AddHours(-hour).ToString("yyyy-MM-dd HH:mm:ss");
            string end = now.ToString("yyyy-MM-dd HH:mm:ss");
            et.SendKeys(start + " - " + end);
            Thread.Sleep(20);
        }

    }
}

