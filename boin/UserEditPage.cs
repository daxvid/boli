namespace Boin;

using OpenQA.Selenium.Chrome;

// 资金概况
public class UserEditPage : ClosePage
{
   private static readonly string path =
        ".//div/div[@class='ivu-modal-content']/div[@class='ivu-modal-header']/div[@class='ivu-modal-header-inner' and text()='编辑']/../..";

    private string gameId;

    public UserEditPage(ChromeDriver driver, AppConfig config, string gameId) : base(driver, config, path)
    {
        this.gameId = gameId;
        // /html/body/div[18]/div[2]/div/div/div[3]/div/button[2]/span
        CancelBtn = FindElementByXPath(MainTable, "./div[3]/div/button[2]/span[text()='取消']");
        // /html/body/div[5]/div[2]/div/div/a/i
        // /html/body/div[5]/div[2]/div/div/div[1]/div
    }

    public string ReadRemark()
    {
        // /html/body/div[16]/div[2]/div/div/div[2]/div/table/tr[10]/td[2]/div/textarea
        var remarkPath = ".//table/tr[10]/td[2]/div/textarea";
        var txt = FindElementByXPath(MainTable, remarkPath);
        var remark = txt.GetAttribute("value") ?? string.Empty;
        return remark;
    }
}

