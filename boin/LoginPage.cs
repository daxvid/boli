namespace Boin;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TwoStepsAuthenticator;
using Boin.Util;

public class LoginPage: PageBase
{
    private readonly AuthConfig authConfig;
    private readonly TimeAuthenticator authenticator;
    
    public LoginPage(ChromeDriver driver, AppConfig config, AuthConfig authConfig) : base(driver, config)
    {
        this.authConfig = authConfig;
        this.authenticator = new TimeAuthenticator();
    }
    
    // 登录
    public bool Login()
    {
        Driver.Navigate().GoToUrl(authConfig.Home);
        // //*[@id="logins"]/div/form/div[1]/div/div/input
        var namePath = "//div[@id=\"logins\"]/div/form/div[1]/div/div/input[@type='text' and @placeholder='请输入账号']";
        SetTextElementByXPath(namePath, authConfig.UserName);

        // //*[@id="logins"]/div/form/div[2]/div/div/input
        var pwdPath =
            "//div[@id=\"logins\"]/div/form/div[2]/div/div/input[@type='password' and @placeholder='请输入密码']";
        SetTextElementByXPath(pwdPath, authConfig.Password);
        for (var i = 1; i < 1000; i++)
        {
            if (Login(i))
            {
                return true;
            }
        }

        return false;
    }


    private bool Login(int i)
    {
        // google认证
        var code = authenticator.GetCode(authConfig.GoogleKey);
        // //*[@id="logins"]/div/form/div[3]/div/div/input
        var glPath = "//div[@id=\"logins\"]/div/form/div[3]/div/div/input";
        SetTextElementByXPath(glPath, code);

        // 登录按钮
        // //*[@id="logins"]/div/form/div[4]/div/button
        FindAndClickByXPath("//div[@id=\"logins\"]/div/form/div[4]/div/button", 1000);

        try
        {
            var e = FindElementByXPath("//*[@id='b_home_notice']/h1");
            var txt = Helper.ReadString(e);
            if (txt.Contains("登入成功"))
            {
                SendMsg("登入成功:" + authConfig.UserName);
                return true;
            }
        }
        catch (WebDriverTimeoutException)
        {
            SendMsg("登入超时:" + authConfig.UserName + "_" + i.ToString());
            Thread.Sleep( (i>60?60:i)*1000);
        }

        return false;
    }
}