
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
    bool Review()
    { 
        // 支付商家内容（飞天代付）
        // /html/body/div[7]/div[2]/div/  div/div[3]/div/div[1]/span[2]/div/div[1]/div/span
        var hitPath = "./div[3]/div/div[1]/span[2]/div/div[1]/div/span";
        var hit = Helper.ReadString(FindElementByXPath(mainTable, hitPath));
        if (hit == "请选择")
        {
            // 选择代付商家，选择第一项
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

    void setReason(string reason)
    {
        // 设置备注内容
        // <input autocomplete="off" spellcheck="false" type="text" placeholder="请输入备注" class="ivu-input ivu-input-default">
        // /html/body/div[47]/div[2]/div/  div/div[2]/div/div[2]/div/div[3]/div[5]/div/input
        // /html/body/div[07]/div[2]/div/  div/div[2]/div/div[2]/div/div[3]/div[4]/div/input
         var remarkPath = "./div[2]/div/div[2]/div/div[3]/div/div/input[@type='text' and @placeholder='请输入备注']";
         SetTextElementByXPath(mainTable, remarkPath, reason);
        
        // 修改按钮
        // <span>修改</span>
        // /html/body/div[47]/div[2]/div/div/div[2]/div/div[2]/div/div[3]/div[5]/button/span
        // /html/body/div[07]/div[2]/div/div/div[2]/div/div[2]/div/div[3]/div[4]/button/span
        FindAndClickByXPath(mainTable, "./div[2]/div/div[2]/div/div[3]/div/button/span[text()='修改']",10);

        // 修改确认框
        // /html/body/div[64]/div[2]/div/div/div/div/div[2]/div <div>确定修改备注？</div>
        // /html/body/div[64]/div[2]/div/div/div/div/div[3]/button[2]/span[确定]
        // /html/body/div[64]/div[2]/div/div/div/div/div[3]/button[1]/span[取消]
        var confirmPath = ".//div[@class='ivu-modal-confirm-body']/div[starts-with(text(),'确定修改备注')]/../../" +
                          "div[3]/button[2]/span[text()='确定']";
        FindAndClickByXPath(confirmPath, 100);
    }
    
    bool Reject()
    {
        var reason = order.RejectReason;
        try
        {
            setReason(reason);
        }
        catch(Exception err)
        {
            Log.SaveException(err, driver);
        }

        // 拒绝按钮
        // <span>拒绝</span>
        // /html/body/div[47]/div[2]/div/  div/div[3]/div/div[2]/div[2]/button[1]/span
        var rejectPath = "./div[3]/div/div[2]/div[2]/button[1]/span[text()='拒绝']";
        FindAndClickByXPath(mainTable, rejectPath, 100);
        using (var rp = new RejectPage(driver,cnf))
        {
            return rp.RejectReason(reason);
        }
    }

    public bool Submit()
    {
        var r = Helper.SafeExec(driver,()=>
        {
            switch (order.ReviewMsg)
            {
                case OrderReviewEnum.Pass:
                    return Review();
                case OrderReviewEnum.Reject:
                    return Reject();
                default:
                    return Remark();
            }
            
        },1000,20);
        return r;
    }

    public bool Remark()
    {
        var reason = order.RemarkReason;
        try
        {
            setReason(reason);
            return true;
        }
        catch(Exception err)
        {
            Log.SaveException(err, driver);
        }
        return false;
    }
}


