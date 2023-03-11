namespace Boin.Util;

using System.Text;
using OpenCCNET;


public class StringUtility
{
    static StringUtility()
    {
        ZhConverter.Initialize();
        // var input = "为我的电脑换了新的内存，开启电脑后感觉看网络视频更加流畅了";
        //
        // // 爲我的電腦換了新的內存，開啓電腦後感覺看網絡視頻更加流暢了
        // Console.WriteLine(ZhConverter.HansToHant(input));
        //
        // // 為我的電腦換了新的內存，開啟電腦後感覺看網絡視頻更加流暢了
        // Console.WriteLine(ZhConverter.HansToTW(input));
        //
        // // 為我的電腦換了新的記憶體，開啟電腦後感覺看網路影片更加流暢了
        // Console.WriteLine(ZhConverter.HansToTW(input, true));
        //
        // // 為我的電腦換了新的內存，開啓電腦後感覺看網絡視頻更加流暢了
        // Console.WriteLine(ZhConverter.HansToHK(input));
        //
        // // 沖繩縣內の學校
        // Console.WriteLine(ZhConverter.ShinToKyuu("沖縄県内の学校"));
    }

    public static string TW2ZH(string str)
    {
        return ZhConverter.HantToHans(str);
    }

    public static string DecodeString(string unicode)
    {
        if (string.IsNullOrEmpty(unicode))
        {
            return string.Empty;
        }

        //string[] ls = unicode.Replace("\\", "").Split(new char[]{'u'},StringSplitOptions.RemoveEmptyEntries);
        string[] ls = unicode.Split(new string[] { "\\u" }, StringSplitOptions.RemoveEmptyEntries);
        StringBuilder builder = new StringBuilder();
        var len = ls.Length;
        for (var i = 0; i < len; i++)
        {
            //builder.Append((Char)ushort.Parse(ls[i], System.Globalization.NumberStyles.HexNumber));
            builder.Append(Convert.ToChar(ushort.Parse(ls[i], System.Globalization.NumberStyles.HexNumber)));

        }

        return builder.ToString();
    }
}