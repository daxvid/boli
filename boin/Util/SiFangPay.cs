namespace Boin.Util;

// 四方查询
public class SiFangPay
{
    public static string Host = string.Empty;
    
    // test: OR1677472338813641
    public static string? GetPayer(string orderId)
    {
        var url = Host + orderId;
        HttpClient client = new HttpClient();
        try
        {
            var task = client.GetStringAsync(url);
            var content = task.Result;
            //content = "{ \"userId\": \"1591389834\", \"name\":\"df\", \"orderId\": \"AD7evE7ANDpuXzL2\", \"orderNo\": \"OR1676436469554825\", \"passageNo\": \"\", \"amount\": 300000, \"realAmount\": 300000, \"status\": 2, \"duplicate\": 0, \"createTime\": 1676436469, \"finishTime\": 1676437144, \"callbackStatus\": 5, \"callbackTime\": 1676437144, \"passageName\": \"\\u5361\\u5361\", \"orderTypeName\": \"\\u94f6\\u8054\"}";
            var name = Helper.GetJsonValue("name", content);
            if (!string.IsNullOrEmpty(name))
            {
                name = System.Text.RegularExpressions.Regex.Unescape(name);
            }
            return name;
        }
        catch(Exception err)
        {
            Console.WriteLine(err);
        }
        return null;
    }

    // public static string GetName(string content)
    // {
    //     string name = string.Empty;
    //     var index = content.IndexOf("\"name\"");
    //     if (index > 0)
    //     {
    //         var start = content.IndexOf("\"", index + 6, 16);
    //         var end = content.IndexOf("\"", start + 1, 64);
    //         name = content.Substring(start + 1, end - start - 1);
    //         if (!string.IsNullOrEmpty(name))
    //         {
    //             name = System.Text.RegularExpressions.Regex.Unescape(name);
    //         }
    //         return name;
    //     }
    //     return name;
    // }
    
}