namespace boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using boin.Util;


public class UserPage : LablePage
{

    public UserPage(ChromeDriver driver, AppConfig cnf) : base(driver, cnf, 1, "用户列表")
    {
    }

    public User Select( Order  order)
    {
        string gameId = order.GameId;
        // 设置游戏ID
        var gameIdPath = "//div[@id='LiveGameRoleList']/div/div/div[contains(text(),'游戏ID')]/div/input";
        SetTextElementByXPath(gameIdPath, gameId);

        // 点击查询按钮
        //*[@id="LiveGameRoleList"]/div[1]/div/div[9]/button[1]/span
        var btnPath = "//div[@id='LiveGameRoleList']/div[1]/div/div[9]/button[1]/span[text()='查询']";
        FindAndClickByXPath(btnPath, 1000);

        // 等待查询结果
        var idPath =
            "//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]/div[2]/table/tbody/tr/td[2]/div/div/div/div[2]/div[3]/div/span";
        idPath += "[contains(text(),'" + gameId + "')]";
        var xp = By.XPath(idPath);
        var t = FindElement(xp);
        var gid = Helper.ReadString(t);
        if (gid == gameId)
        {
            var table = FindElementByXPath("//div[@id='LiveGameRoleList']/div[2]/div[2]/div[1]");
            var tbody = FindElementByXPath(table, ".//tbody[@class='ivu-table-tbody']");
            var row = FindElementByXPath(tbody, ".//tr");
            var user = User.Create(row, order);
            readUserInfo(user, 0);
            return user;
        }

        throw new MoreSuchElementException(xp, "selectUser", null);
    }

    private bool readUserInfo(User user, int i)
    {
        Console.WriteLine("AppId:" + user.AppId + "; GameId:" + user.GameId);
        Int64 gid;
        if (Int64.TryParse(user.GameId, out gid))
        {
            // 编辑(读取用户备注)
            readUserEdit(user, i);
            // 注单(游戏)
            readGameLog(user, i);
            // 概况(资金)
            readFunding(user, i);
            return true;
        }
        else
        {
            // 用户不存在
            Console.WriteLine("无效的游戏ID：" + user.GameId);
        }

        return false;
    }

    // 出入金概况
    private void readFunding(User user, int i)
    {
        var xpath = "(//div[@id='timeListBox']/div/div[2]/button[3]/span[text()='概况']/..)[" +
                    (i + 1).ToString() + "]";
        moveToOp(user, i);
        // 点击扩展按钮中的概况
        FindAndClickByXPath(xpath, 2000);
        using FundingPage gl = new FundingPage(driver, cnf, user.GameId);
        var funding = gl.Select();
        user.Funding = funding;
    }

    // 注单(游戏)
    private void readGameLog(User user, int i)
    {
        var xpath = "(//div[@id='timeListBox']/div/div[2]/button[4]/span[text()='注单']/..)[" +
                    (i + 1).ToString() + "]";
        moveToOp(user, i);
        // 点击扩展按钮中的概况
        FindAndClickByXPath(xpath, 2000);
        using GameLogPage gl = new GameLogPage(driver, cnf, user.GameId);
        var gameInfo = gl.Select(cnf.GameLogMaxHour);
        user.GameInfo = gameInfo;
    }

    // 编辑,查看备注
    private void readUserEdit(User user, int i)
    {
        var xpath = "(//div[@id='timeListBox']/div/div[1]/button[1]/span[text()='编辑']/..)[" +
                    (i + 1).ToString() + "]";
        moveToOp(user, i);
        // 点击扩展按钮中的概况
        FindAndClickByXPath(xpath, 2000);
        using UserEditPage ue = new UserEditPage(driver, cnf, user.GameId);
        user.Remark = ue.ReadRemark();
    }


    private bool moveToOp(User user, int i)
    {
        var opBtn = FindElementByXPath(".//button/span[text()='操作 +']");
        // 移动到【操作+】，显示出扩展按钮
        new Actions(driver).MoveToElement(opBtn).Perform();
        Thread.Sleep(500);
        return true;
    }
}

