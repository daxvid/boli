namespace Boin;

using OpenQA.Selenium.Chrome;
using Boin.Util;

public class ReviewPage : ClosePage
{
    private static readonly string path =
        ".//div/div[@class='ivu-modal-content']/div[@class='ivu-modal-header']/div/h3[text()='提现审核']/../../..";

    private readonly Order order;

    public ReviewPage(ChromeDriver driver, AppConfig config, Order order) : base(driver, config, path)
    {
        this.order = order;
    }

    private bool Review()
    {
        // 支付商家内容（飞天代付）
        // /html/body/div[7]/div[2]/div/  div/div[3]/div/div[1]/span[2]/div/div[1]/div/span
        var hitPath = "./div[3]/div/div[1]/span[2]/div/div[1]/div/span";
        var hit = Helper.ReadString(FindElementByXPath(MainTable, hitPath));
        if (hit == "请选择")
        {
            // 选择代付商家，选择第一项
            // 支付商家下拉
            // /html/body/div[7]/div[2]/div/div/div[3]/div/div[1]/span[2]/div/div[1]/div/i
            FindAndClickByXPath(MainTable, "./div[3]/div/div[1]/span[2]/div/div[1]/div/i", 10);
            // 下拉选项(飞天代付)
            // /html/body/div[7]/div[2]/div/div/div[3]/div/div[1]/span[2]/div/div[2]/ul[2]/li
            FindAndClickByXPath(MainTable, "./div[3]/div/div[1]/span[2]/div/div[2]/ul[2]/li", 10);
        }

        hit = Helper.ReadString(FindElementByXPath(MainTable, hitPath));
        if (string.IsNullOrEmpty(hit) || hit == "请选择")
        {
            return false;
        }

        // 代付提现
        // /html/body/div[7]/div[2]/div/   div/div[3]/div/div[2]/div[2]/button[2]/span
        string dfPath = "./div[3]/div/div[2]/div[2]/button[2]/span[text()='代付提现']";
        FindAndClickByXPath(MainTable, dfPath, 10);
        using var hint = new ReviewHintPage(Driver, Config, order);
        if (!hint.Confirm())
        {
            return false;
        }

        this.Closed = true;
        return true;
    }

    private bool SetReason(string reason)
    {
        try
        {
            // 设置备注内容
            // <input autocomplete="off" spellcheck="false" type="text" placeholder="请输入备注" class="ivu-input ivu-input-default">
            // /html/body/div[47]/div[2]/div/  div/div[2]/div/div[2]/div/div[3]/div[5]/div/input
            // /html/body/div[07]/div[2]/div/  div/div[2]/div/div[2]/div/div[3]/div[4]/div/input
            var remarkPath = "./div[2]/div/div[2]/div/div[3]/div/div/input[@type='text' and @placeholder='请输入备注']";
            SetTextElementByXPath(MainTable, remarkPath, reason);

            // 修改按钮
            // <span>修改</span>
            // /html/body/div[47]/div[2]/div/div/div[2]/div/div[2]/div/div[3]/div[5]/button/span
            // /html/body/div[07]/div[2]/div/div/div[2]/div/div[2]/div/div[3]/div[4]/button/span
            FindAndClickByXPath(MainTable, "./div[2]/div/div[2]/div/div[3]/div/button/span[text()='修改']", 10);

            // 修改确认框
            // /html/body/div[64]/div[2]/div/div/div/div/div[2]/div <div>确定修改备注？</div>
            // /html/body/div[64]/div[2]/div/div/div/div/div[3]/button[2]/span[确定]
            // /html/body/div[64]/div[2]/div/div/div/div/div[3]/button[1]/span[取消]
            var confirmPath = ".//div[@class='ivu-modal-confirm-body']/div[starts-with(text(),'确定修改备注')]/../../" +
                              "div[3]/button[2]/span[text()='确定']";
            FindAndClickByXPath(confirmPath, 100);
            return true;
        }
        catch (Exception err)
        {
            var msg = order.ReviewNote();
            Log.SaveException(new Exception(msg, err), Driver, "reason_");
        }

        return false;
    }

    private bool Reject()
    {
        var reason = order.RejectReason;
        if (!SetReason(reason))
        {
            return false;
        }

        // 拒绝按钮
        // <span>拒绝</span>
        // /html/body/div[47]/div[2]/div/  div/div[3]/div/div[2]/div[2]/button[1]/span
        var rejectPath = "./div[3]/div/div[2]/div[2]/button[1]/span[text()='拒绝']";
        FindAndClickByXPath(MainTable, rejectPath, 100);
        using var rp = new RejectPage(Driver, Config);
        if (!rp.RejectReason(reason))
        {
            return false;
        }

        this.Closed = true;
        return true;
    }

    public bool Submit()
    {
        var r = Helper.SafeExec(Driver, () =>
        {
            switch (order.ReviewMsg)
            {
                case OrderReviewEnum.Pass:
                    return Review();
                case OrderReviewEnum.Reject:
                    return Reject();
                default:
                    return Doubt();
            }

        }, 2000, 5);
        return r;
    }

    // 有疑问的订单设置备注即可
    private bool Doubt()
    {
        var reason = order.DoubtReason;
        return SetReason(reason);
    }
}


