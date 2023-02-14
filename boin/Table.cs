using System;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin
{
    public class Head
    {
        // 标题名
        public string Name { get; set; }
        // class name
        public string Tag { get; set; }

        // 读取列表头
        public static List<Head> ReadHead(IWebElement table)
        {
            //var thList = table.FindElement(By.XPath(".//div[1]"));
            var thList = table.FindElement(By.ClassName("ivu-table-header"));
            List<Head> listHead = new List<Head>();
            var heads = thList.FindElements(By.TagName("th"));
            foreach (var th in heads)
            {
                var value = string.Empty;
                var tag = th.GetAttribute("class");
                if (!string.IsNullOrEmpty(tag))
                {
                    value = th.Text;
                    if (string.IsNullOrEmpty(value))
                    {
                        Console.WriteLine(tag);
                        //try
                        //{
                        //    var span = th.FindElement(By.XPath(".//div/span"));
                        //    value = span.Text;
                        //    if (string.IsNullOrEmpty(value))
                        //    {
                        //        var childs = th.FindElements(By.XPath(".//*"));
                        //        foreach (var child in childs)
                        //        {
                        //            if (!string.IsNullOrEmpty(child.Text))
                        //            {
                        //                value = child.Text;
                        //                break;
                        //            }
                        //        }
                        //    }
                        //}
                        //catch(Exception err)
                        //{
                        //    Console.WriteLine(err);
                        //}
                    }
                    listHead.Add(new Head { Name = value, Tag = tag });
                }
            }

            return listHead;
        }


        // 读取列表头
        public static List<Head> ReadHead2(IWebElement table)
        {
            List<Head> listHead = new List<Head>();
            var heads = table.FindElements(By.XPath(".//div[@class='ivu-table-header']/table/thead/tr/th"));
            foreach (var th in heads)
            {
                var value = string.Empty;
                var tag = th.GetAttribute("class");
                if (!string.IsNullOrEmpty(tag))
                {
                    value = th.Text;
                    listHead.Add(new Head { Name = value, Tag = tag });
                }
            }
            return listHead;
        }
    }

    public class Table
    {
        IWebElement tb;
        public List<Head> Heards = new List<Head>();
        public Table(IWebElement tb)
        {
            this.tb = tb;
        }

        public static Tuple<IWebElement, List<Head>> FindTable(ChromeDriver driver, IEnumerable<String> headNames)
        {
            var tables = driver.FindElements(By.ClassName("ivu-table"));
            foreach (var table in tables)
            {
                var head = Head.ReadHead(table);
                if (mathHead(head, headNames))
                {
                    return new Tuple<IWebElement, List<Head>>(table, head);
                }
            }
            return null;
        }

        static bool mathHead(IEnumerable<Head> heads, IEnumerable<String> headNames)
        {
            foreach (var name in headNames)
            {
                bool exists = false;
                foreach (var head in heads)
                {
                    if (head.Name == name)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    return false;
                }
            }
            return true;
        }


        public static decimal GetDecimal(Dictionary<string, string> dic, string key)
        {
            string value = GetValue(dic, key);
            decimal d;
            decimal.TryParse(value, out d);
            return d;
        }


        public static string GetValue(Dictionary<string, string> dic, string key)
        {
            string value;
            if (dic.TryGetValue(key, out value))
            {
                return value;
            }
            return string.Empty;
        }

        public static decimal ReadDecimal(Dictionary<string, string> head, string key, Dictionary<string, IWebElement> dicCell)
        {
            string value = ReadString(head, key, dicCell);
            decimal d;
            decimal.TryParse(value, out d);
            return d;
        }

        public static DateTime ReadTime(Dictionary<string, string> head, string key, Dictionary<string, IWebElement> dicCell)
        {
            string value = ReadString(head, key, dicCell);
            DateTime d = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            return d;
        }

        public static string ReadString(Dictionary<string, string> head, string key, Dictionary<string, IWebElement> dicCell)
        {
            string className;
            if (!head.TryGetValue(key, out className))
            {
                return string.Empty;
            }
            IWebElement cell;
            if (!dicCell.TryGetValue(className, out cell))
            {
                return string.Empty;
            }

            var value = cell.Text;
            return value;
        }

        public static Dictionary<string, IWebElement> Ele2Dic(IWebElement element)
        {
            var tdList = element.FindElements(By.XPath(".//td"));
            Dictionary<string, IWebElement> row = new Dictionary<string, IWebElement>(tdList.Count*2);
            foreach (var td in tdList)
            {
                var key = td.GetAttribute("class");
                row.Add(key, td);
            }
            return row;
        }

        public static bool SafeClose(ChromeDriver driver, IWebElement table)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            var result = wait.Until(driver =>
            {
                try
                {
                    table.FindElement(By.XPath(".//a/i[@class='ivu-icon ivu-icon-ios-close']")).Click();
                    return true;
                }
                catch (NoSuchElementException) { }
                catch (ElementNotInteractableException) { }
                catch (InvalidOperationException) { }
                catch
                {
                    throw;
                }
                return false;
            });
            return result;
        }
    }
}
