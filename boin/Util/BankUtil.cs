using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;
using RestSharp;

namespace boin.Util
{

    public class BankUtil
    {
        private static string[] bankBin = {
            "CDB","ICBC","ABC","BOC","CCB","PSBC","COMM","CMB","SPDB","CIB","HXBANK","GDB", "CMBC","CITIC","CEB","EGBANK",
            "CZBANK","BOHAIB","SPABANK","SHRCB","YXCCB","YDRCB","BJBANK","SHBANK","JSBANK","HZCB","NJCB","NBBANK","HSBANK",
            "CSCB","CDCB","CQBANK","DLB","NCB","FJHXBC","HKB","WZCB","QDCCB","TZCB","JXBANK","CSRCB","NHB","CZRCB","H3CB","SXCB","SDEB","WJRCB","ZBCB",
            "GYCB","ZYCBANK","HZCCB","DAQINGB","JINCHB","ZJTLCB","GDRCC","DRCBCL","MTBANK","GCB","LYCB",
            "JSRCU","LANGFB","CZCB","DYCB","JZBANK","BOSZ","GLBANK","URMQCCB","CDRCB","ZRCBANK","BOD","LSBANK",
            "BJRCB","TRCB","SRBANK","FDB","CRCBANK","ASCB","NXBANK","BHB","HRXJB","ZGCCB","YNRCC","JLBANK",
            "DYCCB","KLB","ORBANK","XTB","JSB","TCCB","BOYK","JLRCU","SDRCU","XABANK","HBRCU","NXRCU","GZRCU",
            "FXCB","HBHSBANK","ZJNX","XXBANK","HBYCBANK","LSCCB","TCRCB","BZMD","GZB","WRCB","BGB",
            "GRCB","JRCB","BOP","TACCB","CGNB","CCQTGB","XLBANK","HDBANK","KORLABANK","BOJZ","QLBANK","BOQH",
            "YQCCB","SJBANK","FSCB","ZZBANK","SRCB","BANKWF","JJBANK","JXRCU","HNRCU","GSRCU","SCRCU","GXRCU","SXRCCU",
            "WHRCB","YBCCB","KSRB","SZSBK","HSBK","XYBANK","NBYZ","ZJKCCB","XCYH","JNBANK","CBKF","WHCCB","HBC",
            "BOCD","BODD","JHBANK","BOCY","LSBC","BSB","LZYH","BOZK","DZBANK","SCCB","AYCB","ARCU","HURCB","HNRCC","NYNB","LYBANK","NHQS","CBBQS"
    };

        private static string[] bankName = {
            "国家开发银行","中国工商银行","中国农业银行","中国银行","中国建设银行","中国邮政储蓄银行","交通银行","招商银行","上海浦东发展银行","兴业银行","华夏银行","广东发展银行",
            "中国民生银行","中信银行","中国光大银行","恒丰银行","浙商银行","渤海银行","平安银行","上海农村商业银行","玉溪市商业银行","尧都农商行","北京银行","上海银行",
            "江苏银行","杭州银行","南京银行","宁波银行","徽商银行","长沙银行","成都银行","重庆银行","大连银行","南昌银行","福建海峡银行","汉口银行","温州银行","青岛银行","台州银行",
            "嘉兴银行","常熟农村商业银行","南海农村信用联社","常州农村信用联社","内蒙古银行","绍兴银行","顺德农商银行","吴江农商银行","齐商银行","贵阳市商业银行","遵义市商业银行","湖州市商业银行","龙江银行",
            "晋城银行JCBANK","浙江泰隆商业银行","广东省农村信用社联合社","东莞农村商业银行","浙江民泰商业银行","广州银行","辽阳市商业银行","江苏省农村信用联合社","廊坊银行","浙江稠州商业银行","德阳商业银行",
            "晋中市商业银行","苏州银行","桂林银行","乌鲁木齐市商业银行","成都农商银行","张家港农村商业银行","东莞银行","莱商银行","北京农村商业银行","天津农商银行","上饶银行","富滇银行",
            "重庆农村商业银行","鞍山银行","宁夏银行","河北银行","华融湘江银行","自贡市商业银行","云南省农村信用社","吉林银行","东营市商业银行","昆仑银行","鄂尔多斯银行","邢台银行","晋商银行",
            "天津银行","营口银行","吉林农信","山东农信","西安银行","河北省农村信用社","宁夏黄河农村商业银行","贵州省农村信用社","阜新银行","湖北银行黄石分行","浙江省农村信用社联合社","新乡银行",
            "湖北银行宜昌分行","乐山市商业银行","江苏太仓农村商业银行","驻马店银行","赣州银行","无锡农村商业银行","广西北部湾银行","广州农商银行","江苏江阴农村商业银行","平顶山银行","泰安市商业银行",
            "南充市商业银行","重庆三峡银行","中山小榄村镇银行","邯郸银行","库尔勒市商业银行","锦州银行","齐鲁银行","青海银行","阳泉银行","盛京银行","抚顺银行","郑州银行","深圳农村商业银行",
            "潍坊银行","九江银行","江西省农村信用","河南省农村信用","甘肃省农村信用","四川省农村信用","广西省农村信用","陕西信合","武汉农村商业银行","宜宾市商业银行","昆山农村商业银行","石嘴山银行",
            "衡水银行","信阳银行","鄞州银行","张家口市商业银行","许昌银行","济宁银行","开封市商业银行","威海市商业银行","湖北银行","承德银行","丹东银行","金华银行","朝阳银行","临商银行",
            "包商银行","兰州银行","周口银行","德州银行","三门峡银行","安阳银行","安徽省农村信用社","湖北省农村信用社","湖南省农村信用社","广东南粤银行","洛阳银行","农信银清算中心","城市商业银行资金清算中心"
    };

        static Dictionary<string, string> bankDic = new Dictionary<string, string>();
        static BankUtil()
        {
            for (var i = 0; i < bankBin.Length; i++)
            {
                bankDic.Add(bankBin[i], bankName[i]);
            }
        }


        // 通过银行简称获取银行卡所属银行全名 如没有查到全名则返回银行简称
        public static string GetNameOfBank(string bankAbbreviation)
        {
            var n = bankAbbreviation ?? string.Empty;
            return bankDic.GetValueOrDefault(n, n);
        }

        // https://ccdcapi.alipay.com/validateAndCacheCardInfo.json?_input_charset=utf-8&cardBinCheck=true&cardNo=6222801251011210972
        // {"cardType":"DC","bank":"CCB","key":"6222801251011210972","messages":[],"validated":true,"stat":"ok"}
        // https://ccdcapi.alipay.com/validateAndCacheCardInfo.json?_input_charset=utf-8&cardBinCheck=true&cardNo=621669750004140425
        // {"messages":[{"errorCodes":"CARD_BIN_NOT_MATCH","name":"cardNo"}],"validated":false,"stat":"ok","key":"621669750004140425"}
        public static BankCardInfo GetBankInfo(string cardNo)
        {
            string url = "https://ccdcapi.alipay.com/validateAndCacheCardInfo.json?_input_charset=utf-8&cardBinCheck=true&cardNo=" + cardNo;
            string content = string.Empty;
            try
            {
                content = Cache.GetBank(cardNo);
                if (string.IsNullOrEmpty(content))
                {
                    for (var i = 0; i < 4; i++)
                    {
                        // 创建HttpClient实例
                        var client = new RestClient(url);
                        var request = new RestRequest();
                        request.Method = Method.Get;
                        request.AddHeader("Accept", "application/json");
                        var response = client.Execute(request);
                        Thread.Sleep(1);
                        content = response.Content; // raw content as string
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }
                var bankInfo = JsonConvert.DeserializeObject<BankCardInfo>(content);
                Cache.SaveBank(cardNo, content);
                return bankInfo;
            }
            catch (Exception err)
            {
                Dictionary<string, string> msg = new Dictionary<string, string>();
                msg.Add("Message", err.Message);
                msg.Add("StackTrace", err.StackTrace);
                msg.Add("content", content ?? string.Empty);
                var bankInfo = new BankCardInfo()
                {
                    stat = "ok",
                    validated = true,
                    key = err.Message,
                    messages = new List<Dictionary<string, string>>() { msg }
                };
                return bankInfo;
            }
        }


        // @param cardNo 银行卡卡号
        public static string GetCardDetail(string cardNo)
        {
            var bankInfo = GetBankInfo(cardNo);

            //银行卡状态。值：ok，no。
            if (!bankInfo.stat.Equals("ok"))
            {
                return "银行卡不可使用~";
            }
            //有效性，是否正确有效。值：true为是，false为否。
            if (!bankInfo.validated)
            {
                return "银行卡号不正确~";
            }
            //判断是否包含 bank简称
            //所属行。值：所属行简称，如：CMB 为招商银行
            string nameOfBank = GetNameOfBank(bankInfo.bank);
            return nameOfBank;
        }

        //6222801251011210972
        public static bool Chenk(string bankCard)
        {
            //如果小于15位或大于19位为假
            if (bankCard.Length < 15 || bankCard.Length > 19)
            {
                return false;
            }
            //声明一个bit 接收银行卡截取出来的字符串
            char bit = getBank(bankCard.Substring(0, bankCard.Length - 1));
            if (bit == 'F')
            {
                return false;//不是数据返回false
            }
            return bankCard[bankCard.Length - 1] == bit;
        }

        static char getBank(string non)
        {
            char[] cs = non.ToArray();
            foreach (var c in cs)
            {
                if (c < '0' || c > '9')
                {
                    return 'F'; //如果传的不是数据返回F
                }
            }

            int sum = 0;
            for (int i = cs.Length - 1, j = 0; i >= 0; i--, j++)
            {
                int k = cs[i] - '0';
                if (j % 2 == 0)
                {
                    k *= 2;
                    k = k / 10 + k % 10;
                }
                sum += k;
            }
            return (sum % 10 == 0) ? '0' : (char)((10 - sum) % 10 + '0');
        }
    }
}