namespace boin.Util;

using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class Log
{
    public static void Debug()
    {
        
    }
    
    public static void Info(Exception err)
    {
        Console.WriteLine(err.Message);
        Console.WriteLine(err.StackTrace);
    }
    
    public static void SaveException(Exception e, ChromeDriver driver= null)
    {
        var msg = e.ToString();
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
                Console.WriteLine(msg);
                File.WriteAllText(Path.Join(dir, t + ".txt"), msg);
            }
            else
            {
                var st = e.StackTrace;
                Console.WriteLine(msg);
                Console.WriteLine(st);
                File.WriteAllLines(Path.Join(dir, t + ".txt"), new string[] { msg, st});
            }
        }

        TakeScreenshot(driver, dir, t);
    }

    private static void TakeScreenshot(ChromeDriver driver, string dir, string t)
    {
        if (driver != null)
        {
            ITakesScreenshot ssdriver = driver as ITakesScreenshot;
            Screenshot screenshot = ssdriver.GetScreenshot();
            screenshot.SaveAsFile(Path.Join(dir, t + ".png"), ScreenshotImageFormat.Png);
        }
    }

}