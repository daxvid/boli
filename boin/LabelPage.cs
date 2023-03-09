using OpenQA.Selenium;

namespace Boin;

using OpenQA.Selenium.Chrome;

public class LabelPage : PageBase
{
    private readonly int labIndex;
    private readonly string labName;

    protected LabelPage(ChromeDriver driver, AppConfig config, int index, string name) : base(driver, config)
    {
        this.labIndex = index;
        this.labName = name;
    }

    public override bool Open()
    {
        return  GoToPage(labIndex, labName);
    }
    
    public override void Close()
    {
        var path =
            "//div[@id='layout']/div/div[2]/div[2]/div/div/div/div[1]/div[1]/div/div[1]/div/div/div/div/div[contains(text(),'" +
            labName + "')]/i";
        // 关闭窗口
        try
        {     
            wait.Until(drv =>
            {
                var btn = drv.FindElement(By.XPath(path));
                if (btn.Enabled && btn.Displayed)
                {
                    btn.Click();
                    Thread.Sleep(10);
                    return true;
                }
                return false;
            });
        }
        catch
        {
        }
    }
}