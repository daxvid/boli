namespace Boin;

using OpenQA.Selenium.Chrome;

// 拒绝页面
public class RejectPage : ClosePage
{
    private static readonly string path =
        ".//div/div[@class='ivu-modal-content']/div[@class='ivu-modal-body']/div[1]/div[text()='拒绝本次提现申请？']/../../..";

    public RejectPage(ChromeDriver driver, AppConfig config) : base(driver, config, path)
    {
    }

    public bool RejectReason(string msg)
    {
        // /html/body/div[55]/div[2]/div/div/  div[2]/div[1]/form/div/div/div/input
        SetTextElementByXPath(MainTable, "./div[2]/div[1]/form/div/div/div/input[@type='text']", msg);
        // /html/body/div[55]/div[2]/div/div/  div[2]/div[1]/form/div/div/p/label/span/input
        var checkBox =
            FindElementByXPath(MainTable, "./div[2]/div[1]/form/div/div/p/label/span/input[@type='checkbox']");
        var selected = checkBox.Selected;
        if (!selected)
        {
            checkBox.Click();
        }

        // 确定
        // /html/body/div[55]/div[2]/div/div/ div[2]/div[2]/button/span/span
        var btn = FindElementByXPath(MainTable, "./div[2]/div[2]/button/span/span[text()='确定']");
        if (!btn.Enabled)
        {
            return false;
        }

        btn.Click();
        this.Closed = true;
        return true;
    }
}