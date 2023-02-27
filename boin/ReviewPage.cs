
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Util;

namespace boin
{
    public class ReviewPage : PageBase
    {
        private Order order;
        private string path;
        private IWebElement mainTable;
        private IWebElement closeBtn;

        public ReviewPage(ChromeDriver driver, AppConfig cnf, Order order) : base(driver, cnf)
        {
            this.order = order;
            this.path =
                ".//div[@class='ivu-modal-content']/div[@class='ivu-modal-header']/div/h3[text()='提现审核']/../../..";

            // ivu-modal-content[ivu-modal-header,ivu-modal-body,ivu-modal-footer]
            mainTable = FindElementByXPath(this.path);
            closeBtn = FindElementByXPath(mainTable, ".//a/i[@class='ivu-icon ivu-icon-ios-close']");
        }

        public override bool Close()
        {
            if (closeBtn != null)
            {
                try
                {
                    if (closeBtn.Enabled)
                    {
                        closeBtn.Click();
                    }
                }catch{}

                closeBtn = null;
            }
            return true;
        }

        public bool Review()
        {
            // 代付提现
            // /html/body/div[7]/div[2]/div/   div/div[3]/div/div[2]/div[2]/button[2]/span
            string dfPath = ".//div/div[2]/div[2]/button[2]/span[text()='代付提现']";
            FindAndClickByXPath(mainTable, dfPath, 10);
            using (var hint = new ReviewHintPage(driver, cnf, order))
            {
                if (hint.Confirm())
                {
                    return true;
                }
                // TODO：选择这代付商家
                // /html/body/div[7]/div[2]/div/   div/div[3]/div/div[2]/div[2]/button[2]/span
                // /html/body/div[7]/div[2]/div/   div/div[3]/div/div[1]/span[2]/div/div[1]/div/span
                
                
                return false;
            }
            
        }

    }
}

