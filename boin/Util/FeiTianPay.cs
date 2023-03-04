using System.Net.Http.Headers;

namespace boin.Util;

// 飞天接口订单查询
// 1. 创建订单
// 请求地址: https://api.xxx.com /order/query
// 请求方式:POST 表单
// 请求 Header: {'Content-Type': 'application/x-www-form-urlencoded;charset=utf-8'}
// 请求参数:
// 字段 必填定义
// merchantId   商户号
// orderNo      商户订单号
// orderType    订单类型，1 支付 2 兑换
// timestamp    时间戳
// sign         32 位的全小写签名
// 签名方法，顺序固定:
// sign = util.md5('{0}{1}{2}{3}{4}'.format(merchant_id, order_no, order_type,
//     timestamp, token)).lower()
//
// 请求返回 JSON，code = 200 表示查询成功
// 支付: {"code": 200, "msg": "", "data": [{"orderId": 1561, "orderNo":
//
//     "TEST00000000001", "userId": "24025679", "realName": null, "orderType": 1,
//     "amount": 100000, "realAmount": null, "status": 3, "duplicate": false,
//     "createTime": "2023-03-01 14:19:32", "finishTime": null, "callbackStatus":
//     null, "callbackTime": null}]}
//
// 兑换: {"code": 200, "msg": "", "data": [{"orderId": 10000, "orderNo":
//     "TEST00000000001", "orderType": 1, "amount": 50000, "realAmount": null,
//     "extra": {"name": "张三", "bank_no": "6230521380039999999", "bank_code":
//         "ABC", "bank_name": "农业银行", "bank_branch": "北京支行"}, "status": 7,
//     "createTime": "2022-09-03 21:15:09", "finishTime": "2023-02-22 18:10:49",
//     "callbackStatus": null, "callbackTime": null}]}

// 飞天查询
public class FeiTianPay
{
    public static FeiTianConfig Cnf;

    public static string GetPayer(string orderId)
    {
        if (string.IsNullOrEmpty(Cnf.Merchant) || string.IsNullOrEmpty(Cnf.Token))
        {
            return null;
        }
        try
        {
            using (var client = new HttpClient())
            {
                // client.DefaultRequestHeaders.Accept.Add(
                //     new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded; charset=UTF-8")); 
                // client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                // client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                // client.DefaultRequestHeaders.Add("X-MicrosoftAjax", "Delta=true");
                // client.DefaultRequestHeaders.Add("Accept", "*/*");
                // client.Timeout = TimeSpan.FromMilliseconds(10000);
                
                client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded;charset=utf-8");

                const string orderType = "1";
                var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

                var signSource = Cnf.Merchant + orderId + orderType + timeStamp + Cnf.Token;
                var sign = Helper.EncryptMD5(signSource).ToLower();

                //add parameters on request
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("merchantId", Cnf.Merchant),
                    new KeyValuePair<string, string>("orderNo", orderId),
                    new KeyValuePair<string, string>("orderType", orderType),
                    new KeyValuePair<string, string>("timestamp", timeStamp),
                    new KeyValuePair<string, string>("sign", sign)
                };
                var res = client.PostAsync(Cnf.Host, new FormUrlEncodedContent(body)).Result;
                if (res.IsSuccessStatusCode)
                {
                    var content = res.Content.ReadAsStringAsync().Result;
                    var name = Helper.GetJsonValue("name", content);
                    return name;
                }
            }
        }
        catch
        {
        }
        return null;
    }
}