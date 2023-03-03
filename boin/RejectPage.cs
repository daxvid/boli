namespace boin;

using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Util;

// 拒绝页面
public class RejectPage : PageBase
{
    private IWebElement mainTable;
    private IWebElement closeBtn;

    public RejectPage(ChromeDriver driver, AppConfig cnf) : base(driver, cnf)
    {
        // /html/body/div[55]/div[2]/div/div/div[2]/div[1]/div
        var path = ".//div/div[@class='ivu-modal-content']/div[@class='ivu-modal-body']/div[1]/div[text()='拒绝本次提现申请？']/../../..";

        // ivu-modal-content
        mainTable = FindElementByXPath(path);
        
        // /html/body/div[18]/div[2]/div/div/div[3]/div/button[2]/span
        closeBtn = FindElementByXPath(mainTable, "./a[@class='ivu-modal-close']/i");
    }

    public override bool Close()
    {
        if (closeBtn != null)
        {
            try
            {
                closeBtn.Click();
            }
            catch
            {
            }
            closeBtn = null;
        }
        return true;
    }

    public bool RejectReason(string msg)
    {
        // /html/body/div[55]/div[2]/div/div/  div[2]/div[1]/form/div/div/div/input
        SetTextElementByXPath(mainTable, "./div[2]/div[1]/form/div/div/div/input[@type='text']", msg);
        // /html/body/div[55]/div[2]/div/div/  div[2]/div[1]/form/div/div/p/label/span/input
        var checkBox = FindElementByXPath(mainTable, "./div[2]/div[1]/form/div/div/p/label/span/input[@type='checkbox']");
        var selected = checkBox.Selected;
        if (!selected)
        {
            checkBox.Click();
        }
        
        // 确定
        // /html/body/div[55]/div[2]/div/div/ div[2]/div[2]/button/span/span
        var btn = FindElementByXPath(mainTable, "./div[2]/div[2]/button/span/span[text()='确定']");
        if (btn.Enabled)
        {
            return false;
            btn.Click();
        }

        return true;
    }
}