using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Util;

namespace boin;

    // 资金概况
public class UserEditPage : PageBase
{
    private string gameId;
    private string path;
    private IWebElement mainTable;
    private IWebElement closeBtn;
    private IWebElement closeBtn2;

    public UserEditPage(ChromeDriver driver, AppConfig cnf, string gameId) : base(driver, cnf)
    {
        this.gameId = gameId;
        this.path =
            "//div[@class='ivu-modal-content']/div[@class='ivu-modal-header']/div[@class='ivu-modal-header-inner' and text()='编辑']/../..";
        // ivu-modal-content
        mainTable = FindElementByXPath(this.path);
        
        // /html/body/div[18]/div[2]/div/div/div[3]/div/button[2]/span
        
        closeBtn2 = FindElementByXPath(mainTable, "./div[3]/div/button[2]/span[text()='取消']");
        closeBtn = FindElementByXPath(mainTable, "./a[@class='ivu-modal-close']/i");
    }

    public override bool Close()
    {
        if (closeBtn != null)
        {
            try
            {
                closeBtn2.Click();
            }
            catch
            {
                closeBtn.Click();
            }
            closeBtn = null;
        }
        return true;
    }

    public string ReadRemark()
    {
        // /html/body/div[16]/div[2]/div/div/div[2]/div/table/tr[10]/td[2]/div/textarea
        var remarkPath = ".//table/tr[10]/td[2]/div/textarea";
        var txt = FindElementByXPath(mainTable, remarkPath);
        var remark = txt.GetAttribute("value") ?? string.Empty;
        return remark;
    }
}

