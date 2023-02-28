using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace boin;

public class PopPage : PageBase
{
    protected string gameId;
    protected string path;
    protected IWebElement mainTable;
    private IWebElement closeBtn;

    protected PopPage(ChromeDriver driver, AppConfig cnf, string gameId, string path) : base(driver, cnf)
    {
        this.gameId = gameId;
        this.path = path + "[text()='游戏ID：" + gameId + "']/../../../../..";
        mainTable = FindElementByXPath(this.path);
        closeBtn = FindElementByXPath(mainTable,".//a[@class='ivu-modal-close']/i");
        // this.path = "//div[text()='概况' and @class='ivu-modal-header-inner']/../.././/div[text()='游戏ID：" + gameId + "']/../../../../..";
        // this.path = "//div[text()='用户游戏日志' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";
        // this.path = "//div[text()='用户充值详情' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";
        // this.path = "//div[text()='用户提现详情' and @class='ivu-modal-header-inner']/../.././/span[text()='游戏ID：" + gameId + "']/../../../../..";
    }

    protected IWebElement getCurrentTable(int page)
    {
        // var pagePath = ".//div/span[@class='marginRight' and contains(text(),'第" + page.ToString() + "页')]";
        var pagePath = ".//div/span[@class='marginRight' and contains(text(),'第" + page.ToString() + "页')]";
        var table = FindElementByXPath(path); // mainTable
        var pageTag = FindElementByXPath(table, pagePath);
        return mainTable;
    }

    public override bool Close()
    {
        if (closeBtn != null)
        {
            closeBtn.Click();
            closeBtn = null;
        }

        return true;
    }
}
