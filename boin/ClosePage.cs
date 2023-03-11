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
    protected bool Closed;

    protected ClosePage(ChromeDriver driver, AppConfig config, string path) : base(driver, config)
    {
        this.Path = path;
        // ivu-modal-content
        MainTable = FindElementByXPath(path);
        closeBtn = FindElementByXPath(MainTable, "./a[@class='ivu-modal-close']/i");
    }

    // 关闭窗口
    public override void Close()
    {
        if (Closed)
        {
            return;
        }

        try
        {
            try
            {
                closeBtn.Click();
            }
            catch
            {
                if (CancelBtn != null && CancelBtn.Enabled && CancelBtn.Displayed)
                {
                    CancelBtn.Click();
                }
                else
                {
                    throw;
                }
            }

            Closed = true;
            Thread.Sleep(10);
        }
        catch (Exception err)
        {
            Log.SaveException(new Exception(Path, err), Driver, "close_");
        }
    }
}
