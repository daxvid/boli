
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Util;

namespace boin;

public class ReviewPage : PageBase
{
    private readonly Order order;
    private readonly string path;
    private readonly IWebElement mainTable;
    private readonly IWebElement closeBtn;
    private bool closed = false;

    public ReviewPage(ChromeDriver driver, AppConfig cnf, Order order) : base(driver, cnf)
    {
        this.order = order;
        this.path =
            ".//div[@class='ivu-modal-content']/div[@class='ivu-modal-header']/div/h3[text()='提现审核']/../../..";

        // ivu-modal-content[ivu-modal-header,ivu-modal-body,ivu-modal-footer]
        mainTable = FindElementByXPath(this.path);
        closeBtn = FindElementByXPath(mainTable, "./a[@class='ivu-modal-close']/i");
    }

    public override bool Close()
    {
        if (closed)
        {
            return false;
        }

        closed = true;
        try
        {
            if (closeBtn.Enabled)
            {
                closeBtn.Click();
            }
        }
        catch
        {
        }
        return true;
    }

    public bool Review()
    {
        // 支付商家内容（飞天代付）
        // /html/body/div[7]/div[2]/div/  div/div[3]/div/div[1]/span[2]/div/div[1]/div/span
        var hitPath = "./div[3]/div/div[1]/span[2]/div/div[1]/div/span";
        var hit = Helper.ReadString(FindElementByXPath(mainTable, hitPath));
        if (hit == "请选择")
        {
            // TODO：选择代付商家
            // 选择提现方式，选择第一项
            // 支付商家下拉
            // /html/body/div[7]/div[2]/div/div/div[3]/div/div[1]/span[2]/div/div[1]/div/i
            FindAndClickByXPath(mainTable, "./div[3]/div/div[1]/span[2]/div/div[1]/div/i", 10);
            // 下拉选项(飞天代付)
            // /html/body/div[7]/div[2]/div/div/div[3]/div/div[1]/span[2]/div/div[2]/ul[2]/li
            FindAndClickByXPath(mainTable, "./div[3]/div/div[1]/span[2]/div/div[2]/ul[2]/li", 10);
        }

        hit = Helper.ReadString(FindElementByXPath(mainTable, hitPath));
        if (string.IsNullOrEmpty(hit) || hit == "请选择")
        {
            return false;
        }

        // 代付提现
        // /html/body/div[7]/div[2]/div/   div/div[3]/div/div[2]/div[2]/button[2]/span
        string dfPath = "./div[3]/div/div[2]/div[2]/button[2]/span[text()='代付提现']";
        FindAndClickByXPath(mainTable, dfPath, 10);
        using (var hint = new ReviewHintPage(driver, cnf, order))
        {
            if (hint.Confirm())
            {
                return true;
            }

            return false;
        }

    }
}


