
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using boin.Util;

namespace boin
{
    public class ReviewHintPage : PageBase
    {
        private Order order;
        private string path;
        private IWebElement mainTable;
        private IWebElement closeBtn;

        public ReviewHintPage(ChromeDriver driver, AppConfig cnf, Order order) : base(driver, cnf)
        {
            this.order = order;
            this.path =
                ".//div[@class='ivu-modal-content ivu-modal-content-no-mask']/div[1]/div[text()='提示' and @class='modal_header']/../..";
            
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

            var amount = Helper.ReadDecimal(FindElementByXPath(mainTable, "./div[2]/div[@class='modal_main']/span[1]"));
            var cardNo = Helper.ReadString(FindElementByXPath(mainTable, "./div[2]/div[@class='modal_main']/span[2]"));
            if (order.CardNo != cardNo || order.Amount != amount)
            {
                return false;
            }
            
            // 点击确认按钮
            FindAndClickByXPath(mainTable, "./div[2]/div[2]/button/span/span", 1000);
            
            // 等待另外一个确认框然后关闭
            // 审核通过/审核成功/确定
            // /html/body/div[64]/div[2]/div/div/div/div/div[1]/div[2][text()='审核通过']
            // /html/body/div[64]/div[2]/div/div/div/div/div[2]/div[text()='审核成功']
            // /html/body/div[64]/div[2]/div/div/div/div/div[3]/button/span[text()='确定']

            var txt = FindElementByXPath(".//div[@class='ivu-modal-confirm']/div[@class='ivu-modal-confirm-body']/div").Text;
            if (txt == "审核成功")
            {
                var lastConfirmPath =
                    ".//div[@class='ivu-modal-confirm']/div[@class='ivu-modal-confirm-body']/div[text()='审核成功']/../../div[@class='ivu-modal-confirm-footer']/button/span[text()='确定']";
                FindAndClickByXPath(lastConfirmPath, 100);
                return true;
            }
            else
            {     
                var lastConfirmPath =
                    ".//div[@class='ivu-modal-confirm']/div[@class='ivu-modal-confirm-body']/div/../../div[@class='ivu-modal-confirm-footer']/button/span[text()='确定']";
                FindAndClickByXPath(lastConfirmPath, 100);
            }

            return false;

        }
    }
}