using System;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace boin;

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
        var heads = thList.FindElements(By.XPath(".//th"));
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
}
