
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Util;

namespace boin;

public class ReviewHintPage : PageBase
{
    private readonly Order order;
    private readonly string path;
    private readonly IWebElement mainTable;
    private readonly IWebElement closeBtn;
    private bool closed = false;

    public ReviewHintPage(ChromeDriver driver, AppConfig cnf, Order order) : base(driver, cnf)
    {
        this.order = order;
        this.path =
            ".//div[@class='ivu-modal-content ivu-modal-content-no-mask']/div[1]/div[text()='提示' and @class='modal_header']/../..";

        // <div class="ivu-modal-content ivu-modal-content-no-mask">
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

    public bool Confirm()
    {
        // 提示
        // 提示确定， 确认转账 2200元至 6214835768356451（提现账号）？

        // 金额/提现账号（2200元至 6214835768356451）
        // /html/body/div[32]/div/div/div/div[2]/div[1]/span[1]
        // /html/body/div[32]/div/div/div/div[2]/div[1]/span[2]
        // 确定按钮
        // /html/body/div[32]/div/div/div/div[2]/div[2]/button/span/span
        // /html/body/div[80]/div/div/div/div[2]/div[2]/button/span/span 

        try
        {
            var amount = Helper.ReadDecimal(FindElementByXPath(mainTable, "./div[2]/div[@class='modal_main']/span[1]"));
            var cardNo = Helper.ReadString(FindElementByXPath(mainTable, "./div[2]/div[@class='modal_main']/span[2]"));
            if (order.CardNo != cardNo || order.Amount != amount)
            {
                return false;
            }
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            return false;
        } 

        // 点击确认按钮
        FindAndClickByXPath(mainTable, "./div[2]/div[2]/button/span/span", 1000);

        // 等待另外一个确认框然后关闭
        // 审核通过/审核成功/确定
        // /html/body/div[64]/div[2]/div/div/div/div/div[1]/div[2][text()='审核通过']
        // /html/body/div[64]/div[2]/div/div/div/div/div[2]/div[text()='审核成功']
        // /html/body/div[64]/div[2]/div/div/div/div/div[3]/button/span[text()='确定']
        var confirmPage = FindElementByXPath(".//div[@class='ivu-modal-body']/div[@class='ivu-modal-confirm']");
        var txt = FindElementByXPath(confirmPage, "./div[@class='ivu-modal-confirm-body']/div").Text;
        var lastConfirmPath = "./div[@class='ivu-modal-confirm-footer']/button/span[text()='确定']";
        FindAndClickByXPath(confirmPage, lastConfirmPath, 100);
        if (txt == "审核成功")
        {
            return true;
        }

        return false;
    }
}