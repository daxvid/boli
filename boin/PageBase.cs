using System;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using boin.Review;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    public class PageBase : IDisposable
    {
        protected ChromeDriver driver;
        protected WebDriverWait wait;
        protected int maxPage = 4;

        public static readonly ReadOnlyCollection<IWebElement> EmptyElements = new ReadOnlyCollection<IWebElement>(new List<IWebElement>());

        public PageBase(ChromeDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        protected IWebElement SetTextElementByXPath(string path, string txt)
        {
            return SetTextElement(By.XPath(path), txt);
        }

        protected IWebElement SetTextElement(By by, string txt)
        {
            IWebElement r = null;
            var result = wait.Until(driver =>
            {
                try
                {
                    var es = driver.FindElements(by);
                    if (es.Count == 0)
                    {
                        return false;
                    }
                    if (es.Count == 1)
                    {
                        r = es[0];
                        r.Clear();
                        r.SendKeys(txt);
                        return true;
                    }
                    else
                    {
                        throw new MoreSuchElementException(by, "set text", es);
                    }
                }
                catch (NoSuchElementException) { }
                catch
                {
                    throw;
                }
                return false;
            });
            return r;
        }

        protected IWebElement SetTextElementByXPath(IWebElement e, string path, string txt)
        {
            return SetTextElement(e, By.XPath(path), txt);
        }

        protected IWebElement SetTextElement(IWebElement e, By by, string txt)
        {
            IWebElement r = null;
            var result = wait.Until(driver =>
            {
                try
                {
                    var es = e.FindElements(by);
                    if (es.Count == 0)
                    {
                        return false;
                    }
                    if (es.Count == 1)
                    {
                        r = es[0];
                        r.Clear();
                        r.SendKeys(txt);
                        return true;
                    }
                    else
                    {
                        throw new MoreSuchElementException(by, "set text", es);
                    }
                }
                catch (NoSuchElementException) { }
                catch
                {
                    throw;
                }
                return false;
            });
            return r;
        }

        protected IWebElement FindElementByXPath(string path)
        {
            return FindElement(By.XPath(path));
        }

        protected IWebElement FindElement(By by)
        {
            IWebElement r = null;
            var result = wait.Until(driver =>
            {
                try
                {
                    var es = driver.FindElements(by);
                    if (es.Count == 0)
                    {
                        return false;
                    }
                    if (es.Count == 1)
                    {
                        r = es[0];
                        return true;
                    }
                    else
                    {
                        throw new MoreSuchElementException(by, "find", es);
                    }
                }
                catch (NoSuchElementException) { }
                catch
                {
                    throw;
                }
                return false;
            });
            return r;
        }

        protected IWebElement FindElementByXPath(IWebElement e, string path)
        {
            return FindElement(e, By.XPath(path));
        }

        protected IWebElement FindElement(IWebElement e, By by)
        {
            IWebElement r = null;
            var result = wait.Until(driver =>
            {
                try
                {
                    var es = e.FindElements(by);
                    if (es.Count == 0)
                    {
                        return false;
                    }
                    if (es.Count == 1)
                    {
                        r = es[0];
                        return true;
                    }
                    else
                    {
                        throw new MoreSuchElementException(by, "find", es);
                    }
                }
                catch (NoSuchElementException) { }
                catch
                {
                    throw;
                }
                return false;
            });
            return r;
        }

        protected ReadOnlyCollection<IWebElement> FindElementsByXPath(string path)
        {
            return FindElements(By.XPath(path));
        }

        protected ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            var result = wait.Until(driver =>
            {
                try
                {
                    var es = driver.FindElements(by);
                    return es;
                }
                catch (NoSuchElementException) { }
                return EmptyElements;
            });
            return result;
        }

        protected ReadOnlyCollection<IWebElement> FindElementsByXPath(IWebElement e, string path)
        {
            return FindElements(e, By.XPath(path));
        }

        protected ReadOnlyCollection<IWebElement> FindElements(IWebElement e, By by)
        {
            var result = wait.Until(driver =>
            {
                try
                {
                    var es = e.FindElements(by);
                    return es;
                }
                catch (NoSuchElementException) { }
                return EmptyElements;
            });
            return result;
        }

        protected bool TryClickByXPath(string path, int ms = 1000)
        {
            return TryClick(By.XPath(path), ms);
        }

        protected bool TryClick(By by, int ms = 1000)
        {
            var result = wait.Until(driver =>
            {
                try
                {
                    var es = driver.FindElements(by);
                    if (es.Count == 0)
                    {
                        return false;
                    }
                    if (es.Count == 1)
                    {
                        var c = es[0];
                        c.Click();
                        Thread.Sleep(ms);
                        return true;
                    }
                    else
                    {
                        throw new MoreSuchElementException(by, "click", es);
                    }
                }
                catch (NoSuchElementException) { }
                catch (ElementClickInterceptedException) { }
                catch
                {
                    throw;
                }
                return false;
            });
            return result;
        }

        protected bool TryClickByXPath(IWebElement e, string path, int ms = 1000)
        {
            return TryClick(e, By.XPath(path), ms);
        }

        protected bool TryClick(IWebElement e, By by, int ms = 1000)
        {
            var result = wait.Until(driver =>
            {
                try
                {
                    var es = e.FindElements(by);
                    if (es.Count == 0)
                    {
                        return false;
                    }
                    if (es.Count == 1)
                    {
                        var c = es[0];
                        c.Click();
                        Thread.Sleep(ms);
                        return true;
                    }
                    else
                    {
                        throw new MoreSuchElementException(by, "click", es);
                    }
                }
                catch (NoSuchElementException) { }
                catch (ElementClickInterceptedException) { }
                catch
                {
                    throw;
                }
                return false;
            });
            return result;
        }

        protected bool SafeClick(IWebElement btn, int ms = 0)
        {
            if (btn.Enabled)
            {
                return wait.Until(driver =>
                {
                    try
                    {
                        btn.Click();
                        Thread.Sleep(ms);
                        return true;
                    }
                    catch (ElementClickInterceptedException) { }
                    return false;
                });
            }
            return true;
        }

        protected bool GoToPage(int index, string itme)
        {
            var v1 = TryClick(By.CssSelector("nav li:nth-child(" + index + ")"));
            var v2 = TryClick(By.LinkText(itme));
            return v1 && v2;
        }

        public virtual bool Open()
        {
            return true;
        }

        public virtual bool Close()
        {
            return true;
        }

        public virtual void SendMsg(string msg)
        {
            Console.WriteLine(msg);
            TelegramBot.SendMessage(msg);
        }


        // 读取列表头
        public static List<Head> ReadHeadList(IWebElement table)
        {
            List<Head> listHead = new List<Head>();
            var heads = table.FindElements(By.XPath(".//div[@class='ivu-table-header']/table/thead/tr/th"));
            foreach (var th in heads)
            {
                var tag = th.GetAttribute("class");
                if (!string.IsNullOrEmpty(tag))
                {
                    var value = th.Text;
                    listHead.Add(new Head { Name = value, Tag = tag });
                }
            }
            return listHead;
        }

        public static Dictionary<string, string> ReadHeadDic(IWebElement table)
        {
            //var heads = table.FindElements(By.XPath(".//div[@class='ivu-table-header']/table/thead/tr/th"));
            var dicHead = new Dictionary<string, string>(29);
            //foreach (var th in heads)
            //{
            //    var tag = th.GetAttribute("class");
            //    if (!string.IsNullOrEmpty(tag))
            //    {
            //        var key = th.Text;
            //        dicHead.Add(key, tag);
            //    }
            //}
            return dicHead;
        }

        public void Dispose()
        {
            this.Close();
        }

        protected bool GoToNextPage(IWebElement table, int ms = 500)
        {
            // 检查是否有下一页
            var nextPage = FindElementByXPath(table, ".//button/span/i[@class='ivu-icon ivu-icon-ios-arrow-forward']/../..");
            var next = nextPage.Enabled;
            if (!next)
            {
                return false;
            }
            nextPage.Click();
            //TODO: 检查是否加载完成
            Thread.Sleep(ms);
            return true;
        }
    }
}

