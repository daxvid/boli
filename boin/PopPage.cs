namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class PopPage : ClosePage
{
    protected readonly string GameId;

    protected PopPage(ChromeDriver driver, AppConfig config, string gameId, string path) :
        base(driver, config, path + "[text()='游戏ID：" + gameId + "']/../../../../..")
    {
        this.GameId = gameId;
        // this.path = "//div[text()='概况'        and @class='ivu-modal-header-inner']/../..//div[text()='游戏ID：" + gameId + "']/../../../../..";
        // this.path = "//div[text()='用户游戏日志' and @class='ivu-modal-header-inner']/../..//span[text()='游戏ID：" + gameId + "']/../../../../..";
        // this.path = "//div[text()='用户充值详情' and @class='ivu-modal-header-inner']/../..//span[text()='游戏ID：" + gameId + "']/../../../../..";
        // this.path = "//div[text()='用户提现详情' and @class='ivu-modal-header-inner']/../..//span[text()='游戏ID：" + gameId + "']/../../../../..";
    }

    protected IWebElement GetCurrentTable(int page)
    {
        var table = FindElementByXPath(Path); // mainTable
        var pagePath = ".//div/span[@class='marginRight' and text()='第" + page.ToString() + "页']";
        var pageTag = FindElementByXPath(table, pagePath);
        return MainTable;
    }
}
