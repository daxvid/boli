using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

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

        public static bool TryClick(WebDriverWait wait, IWebElement btn)
        {
            if (btn.Enabled)
            {
                return wait.Until(driver =>
                {
                    try
                    {
                        btn.Click();
                        return true;
                    }
                    catch (ElementClickInterceptedException) { }
                    return false;
                });
            }
            return true;
        }

    }
}

