namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using Boin.Util;

public class UserPage : LabelPage
{

    public UserPage(ChromeDriver driver, AppConfig config) : base(driver, config, 1, "用户列表")
    {
    }

    public User Select(Order order)
    {
        var gameId = order.GameId;
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
            ReadUserInfo(user, 0);
            return user;
        }

        throw new MoreSuchElementException(xp, "selectUser", null);
    }

    private bool ReadUserInfo(User user, int i)
    {
        Console.WriteLine("AppId:" + user.AppId + "; GameId:" + user.GameId);
        if (Int64.TryParse(user.GameId, out var _))
        {
            // 编辑(读取用户备注)
            ReadUserEdit(user, i);
            // 注单(游戏)
            ReadGameLog(user, i);
            // 概况(资金)
            ReadFunding(user, i);
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
    private void ReadFunding(User user, int i)
    {
        var xpath = "(//div[@id='timeListBox']/div/div[2]/button[3]/span[text()='概况']/..)[" +
                    (i + 1).ToString() + "]";
        MoveToOp(user, i);
        // 点击扩展按钮中的概况
        FindAndClickByXPath(xpath, 2000);
        using FundingPage gl = new FundingPage(Driver, Config, user.GameId);
        var funding = gl.Select();
        user.Funding = funding;
    }

    // 注单(游戏)
    private void ReadGameLog(User user, int i)
    {
        var xpath = "(//div[@id='timeListBox']/div/div[2]/button[4]/span[text()='注单']/..)[" +
                    (i + 1).ToString() + "]";
        MoveToOp(user, i);
        // 点击扩展按钮中的概况
        FindAndClickByXPath(xpath, 2000);
        using GameLogPage gl = new GameLogPage(Driver, Config, user.GameId);
        var gameInfo = gl.Select(Config.GameLogMaxHour);
        user.GameInfo = gameInfo;
    }

    // 编辑,查看备注
    private void ReadUserEdit(User user, int i)
    {
        var xpath = "(//div[@id='timeListBox']/div/div[1]/button[1]/span[text()='编辑']/..)[" +
                    (i + 1).ToString() + "]";
        MoveToOp(user, i);
        // 点击扩展按钮中的概况
        FindAndClickByXPath(xpath, 2000);
        using UserEditPage ue = new UserEditPage(Driver, Config, user.GameId);
        user.Remark = ue.ReadRemark();
    }


    private void MoveToOp(User user, int i)
    {
        var opBtn = FindElementByXPath(".//button/span[text()='操作 +']");
        // 移动到【操作+】，显示出扩展按钮
        new Actions(Driver).MoveToElement(opBtn).Perform();
        Thread.Sleep(500);
    }
}

