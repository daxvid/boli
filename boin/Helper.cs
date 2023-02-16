using System;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace boin
{
	public class Helper
	{
        public static decimal GetDecimal(Dictionary<string, string> dic, string key)
        {
            string value = GetValue(dic, key);
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            decimal d = decimal.Parse(value);
            //decimal.TryParse(value, out d);
            return d;
        }


        public static string GetValue(Dictionary<string, string> dic, string key)
        {
            return dic.GetValueOrDefault(key, string.Empty);
        }

        public static decimal ReadDecimal(Dictionary<string, string> head, string key, Dictionary<string, IWebElement> dicCell)
        {
            string value = ReadString(head, key, dicCell);
            decimal d;
            decimal.TryParse(value, out d);
            return d;
        }

        public static DateTime ReadTime(Dictionary<string, string> head, string key, Dictionary<string, IWebElement> dicCell)
        {
            string value = ReadString(head, key, dicCell);
            DateTime d = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            return d;
        }

        public static string ReadString(Dictionary<string, string> head, string key, Dictionary<string, IWebElement> dicCell)
        {
            string className;
            if (!head.TryGetValue(key, out className))
            {
                return string.Empty;
            }
            IWebElement cell;
            if (!dicCell.TryGetValue(className, out cell))
            {
                return string.Empty;
            }

            var value = cell.Text;
            value = value.Trim();
            return value;
        }

        public static Dictionary<string, IWebElement> Ele2Dic(IWebElement element)
        {
            var tdList = element.FindElements(By.XPath(".//td"));
            Dictionary<string, IWebElement> row = new Dictionary<string, IWebElement>(tdList.Count * 2);
            foreach (var td in tdList)
            {
                var key = td.GetAttribute("class");
                row.Add(key, td);
            }
            return row;
        }

        public static void SetTimeRang(int hour, IWebElement et)
        {
            et.Click();
            et.SendKeys(Keys.Control + "a");
            et.SendKeys(Keys.Delete);
            et.SendKeys(Keys.Command + "a");
            et.SendKeys(Keys.Delete);
            Thread.Sleep(10);

            var now = DateTime.Now;
            string start = now.AddHours(-hour).ToString("yyyy-MM-dd HH:mm:ss");
            string end = now.ToString("yyyy-MM-dd HH:mm:ss");
            et.SendKeys(start + " - " + end);
            Thread.Sleep(10);
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

        public static bool SafeClose(ChromeDriver driver, IWebElement table)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            var result = wait.Until(driver =>
            {
                try
                {
                    table.FindElement(By.XPath(".//a/i[@class='ivu-icon ivu-icon-ios-close']")).Click();
                    return true;
                }
                catch (NoSuchElementException) { }
                catch (ElementClickInterceptedException) { }
                //catch (ElementNotInteractableException) { }
                //catch (InvalidOperationException) { }
                return false;
            });
            return result;
        }
    }
}

