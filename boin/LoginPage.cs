using boin.Util;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TwoStepsAuthenticator;

namespace boin;

public class LoginPage: PageBase
{
    private readonly AuthConfig _authCnf;
    private readonly TimeAuthenticator _authenticator;
    
    public LoginPage(ChromeDriver driver, AppConfig cnf, AuthConfig authCnf) : base(driver, cnf)
    {
        this._authCnf = authCnf;
        this._authenticator = new TwoStepsAuthenticator.TimeAuthenticator();
    }
    
    // 登录
    public bool Login()
    {
        driver.Navigate().GoToUrl(_authCnf.Home);
        // //*[@id="logins"]/div/form/div[1]/div/div/input
        var namePath = "//div[@id=\"logins\"]/div/form/div[1]/div/div/input[@type='text' and @placeholder='请输入账号']";
        SetTextElementByXPath(namePath, _authCnf.UserName);

        // //*[@id="logins"]/div/form/div[2]/div/div/input
        var pwdPath =
            "//div[@id=\"logins\"]/div/form/div[2]/div/div/input[@type='password' and @placeholder='请输入密码']";
        SetTextElementByXPath(pwdPath, _authCnf.Password);
        for (var i = 1; i < 100; i++)
        {
            if (login(i))
            {
                return true;
            }
        }

        return false;
    }


    private bool login(int i)
    {
        // google认证
        var code = _authenticator.GetCode(_authCnf.GoogleKey);
        // //*[@id="logins"]/div/form/div[3]/div/div/input
        var glPath = "//div[@id=\"logins\"]/div/form/div[3]/div/div/input";
        var googlePwd = SetTextElementByXPath(glPath, code);

        // 登录按钮
        // //*[@id="logins"]/div/form/div[4]/div/button
        FindAndClickByXPath("//div[@id=\"logins\"]/div/form/div[4]/div/button", 1000);

        try
        {
            var e = FindElementByXPath("//*[@id='b_home_notice']/h1");
            var txt = Helper.ReadString(e);
            if (txt.Contains("登入成功"))
            {
                SendMsg("登入成功:" + _authCnf.UserName);
                return true;
            }
        }
        catch (WebDriverTimeoutException)
        {
            SendMsg("登入超时:" + _authCnf.UserName + "_" + i.ToString());
            Thread.Sleep(1000 * i);
        }

        return false;
    }
}