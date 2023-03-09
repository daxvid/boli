using Boin.Util;

namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class ClosePage : PageBase
{
    protected readonly string Path;
    protected readonly IWebElement MainTable;
    private readonly IWebElement closeBtn;
    protected IWebElement? CancelBtn;
    private bool closed;

    protected ClosePage(ChromeDriver driver, AppConfig config, string path) : base(driver, config)
    {
        this.Path = path;
        // ivu-modal-content
        MainTable = FindElementByXPath(path);
        closeBtn = FindElementByXPath(MainTable, ".//a[@class='ivu-modal-close']/i");
    }

    public override void Close()
    {
        if (closed)
        {
            return;
        }

        // 关闭窗口
        try
        {
            if (closeBtn.Enabled && closeBtn.Displayed)
            {
                closeBtn.Click();
                closed = true;
                Thread.Sleep(10);
            }
            else if (CancelBtn != null && CancelBtn.Enabled && CancelBtn.Displayed)
            {
                CancelBtn.Click();
                closed = true;
                Thread.Sleep(10);
            }
        }
        catch (Exception err)
        {
            //Log.SaveException(err, Driver);
        }
    }
}
