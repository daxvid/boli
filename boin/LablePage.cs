using OpenQA.Selenium.Chrome;

namespace boin;

public class LablePage : PageBase
{
    private int labIndex;
    private string labName;

    private bool closed = false;

    protected LablePage(ChromeDriver driver, AppConfig cnf, int index, string name) : base(driver, cnf)
    {
        this.labIndex = index;
        this.labName = name;
    }

    public override bool Open()
    {
        return  GoToPage(labIndex, labName);
    }
    
    public override bool Close()
    {
        // var path1 = "//div[@id='layout']/div/div[2]/div[2]/div/div/div/div[1]/div[1]/div/div[1]/div/div/div/div/div[contains(text(),'用户列表')]/i";
        // var path2 = "//div[@id='layout']/div/div[2]/div[2]/div/div/div/div[1]/div[1]/div/div[1]/div/div/div/div/div[contains(text(),'绑定管理')]/i";
        // var path3 = "//div[@id='layout']/div/div[2]/div[2]/div/div/div/div[1]/div[1]/div/div[1]/div/div/div/div/div[contains(text(),'提现管理')]/i";

        if (closed)
        {
            return false;
        }

        closed = true;
        var path =
            "//div[@id='layout']/div/div[2]/div[2]/div/div/div/div[1]/div[1]/div/div[1]/div/div/div/div/div[contains(text(),'" +
            labName + "')]/i";
        // 关闭窗口
        FindAndClickByXPath(path, 100);
        return base.Close();
    }
}