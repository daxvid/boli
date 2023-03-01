using System;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using boin.Bot;

namespace boin.Util;

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

    public static decimal ReadDecimal(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
    {
        string value = ReadString(head, key, dicCell);
        decimal d;
        decimal.TryParse(value, out d);
        return d;
    }

    public static DateTime ReadTime(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
    {
        string value = ReadString(head, key, dicCell);
        DateTime d = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
        return d;
    }

    public static string ReadString(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
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

        var value = cell.Text.Trim();
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

    // 设置查询的时间范围
    public static void SetTimeRang(IWebElement et, int hour)
    {
        et.Click();
        et.SendKeys(Keys.Control + "a");
        et.SendKeys(Keys.Delete);
        et.SendKeys(Keys.Command + "a");
        et.SendKeys(Keys.Delete);

        var now = DateTime.Now;
        string start = now.AddHours(-hour).ToString("yyyy-MM-dd HH:mm:ss");
        string end = now.ToString("yyyy-MM-dd 23:59:59");
        et.SendKeys(start + " - " + end);
        Thread.Sleep(10);
    }

    // 设置查询的日期范围
    public static void SetDayRang(IWebElement et, int day)
    {
        et.Click();
        et.SendKeys(Keys.Control + "a");
        et.SendKeys(Keys.Delete);
        et.SendKeys(Keys.Command + "a");
        et.SendKeys(Keys.Delete);

        var now = DateTime.Now;
        string start = now.AddDays(-(day - 1)).ToString("yyyy-MM-dd");
        string end = now.ToString("yyyy-MM-dd");
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
                catch (WebDriverException)
                {
                }

                return false;
            });
        }

        return true;
    }



    public static decimal ReadBetDecimal(IWebElement e)
    {
        var txt = e.Text;
        var index = txt.IndexOf('：');
        txt = txt.Substring(index + 1);
        decimal r;
        decimal.TryParse(txt, out r);
        return r;
    }

    public static decimal ReadDecimal(IWebElement e)
    {
        var txt = ReadString(e);
        decimal r = decimal.Parse(txt);
        return r;
    }

    public static string ReadString(IWebElement e)
    {
        var txt = e.Text.Trim();
        if (txt == "--")
        {
            return string.Empty;
        }

        return txt;
    }

    public static DateTime ReadDateTime(IWebElement e)
    {
        var txt = ReadString(e);
        var r = DateTime.ParseExact(txt, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
        return r;
    }

    public static DateTime ReadShortTime(IWebElement e)
    {
        var str = Helper.ReadString(e);
        if (str.IndexOf('/') >= 0)
        {
            str = str.Replace("/", "-");
        }

        var now = DateTime.Now;
        for (var year = now.Year; year >= 2022; year--)
        {
            DateTime d = DateTime.ParseExact(year + "-" + str, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            if (d < now)
            {
                return d;
            }
        }

        return now;
    }

    public static void SendMsg(string msg)
    {
        Console.WriteLine(msg);
        TelegramBot.SendMsg(msg);
    }

    public static void SendMsg(Exception err)
    {
        var s = err.ToString();
        var t = err.StackTrace;
        Console.WriteLine(s);
        Console.WriteLine(t);
        TelegramBot.SendMsg(s);
        TelegramBot.SendMsg(t);
    }
    
    public static void TakeScreenshot(ChromeDriver driver, Exception e)
    {
        string dir = Path.Join(Environment.CurrentDirectory, "log");
        if (!Path.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string t = DateTime.Now.ToString("yyMMddHHmmssfff");
        if (e != null)
        {
            if (e is WebDriverException)
            {
                File.WriteAllText(Path.Join(dir, t + ".txt"), e.ToString());
            }
            else
            {
                File.WriteAllLines(Path.Join(dir, t + ".txt"),  new string[]{ e.ToString(), e.StackTrace });
            }
        }
        ITakesScreenshot ssdriver = driver as ITakesScreenshot;
        Screenshot screenshot = ssdriver.GetScreenshot();
        screenshot.SaveAsFile(Path.Join(dir, t + ".png"), ScreenshotImageFormat.Png);
    }
    

    public static T SafeExec<T>(ChromeDriver driver, Func<T> fun, int sleep = 1000, int tryCount = int.MaxValue)
    {
        Exception ex = null;
        for (var i = 0; i < tryCount; i++)
        {
            try
            {
                return fun();
            }
            catch (WebDriverException e)
            {
                ex = e;
                TakeScreenshot(driver, e);
                if (e is InvalidElementStateException ||
                    e is NotFoundException ||
                    e is WebDriverTimeoutException)
                {
                    Log.Info(e);
                }
                else
                {
                    SendMsg(e);
                    throw;
                }
            }
            catch (SystemException e)
            {
                ex = e;
                TakeScreenshot(driver,e);
                Log.Info(e);
            }
            catch (Exception e)
            {
                ex = e;
                TakeScreenshot(driver,e);
                SendMsg(e);
                throw;
            }

            Thread.Sleep(sleep);
        }

        throw ex;
    }
}

