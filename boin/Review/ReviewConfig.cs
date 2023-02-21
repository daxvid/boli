using System;
using YamlDotNet.Serialization;

namespace boin.Review
{
    //新会员(注册时间一个月内)波币当日提款不超过3000
    //老会员(注册时间一个月以上)波币当日提款不超过6000
    //任何波币提款该笔不超过3000
    //新会员提银行卡-卡号无误-可以过
    //新会员提银行卡-卡号有误-不可以过
    //任何提款出现-波币/银行卡姓名不一致-不可以通过
    //最新两笔提款名字不一致-不可以通过
    //新会员首提使用波币提款-游戏炸金花-不可以通过
    //新会员首提使用银行卡提款-游戏炸金花-可以通过
    //会员游戏『环亚棋牌』『炸金花』—提款任何方式-不可以通过
    //会员使用卡卡充值的姓名，与提款姓名不符-不可以通过
    //充值和提款两者都超过四万的

    public class AmountConfig
    {
        // 当日充值达多少不审
        public decimal DayRecharge { get; set; }

        // 每日最大额度
        public decimal DayMax { get; set; }

        // 每笔最大额度
        public decimal OnceMax { get; set; }

        // 不可以通过的游戏
        public List<string> BanGames { get; set; }

        public string ExistsGame(string p, string g)
        {
            string pg = p + g;
            foreach (var item in BanGames)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item == g || item == pg)
                    {
                        return item;
                    }
                }
            }
            return string.Empty;
        }
    }

    public class ReviewConfig
    {
        // 新会员银行卡检查条件
        public AmountConfig NewBank { get; set; }

        // 老会员银行卡检查条件
        public AmountConfig OldBank { get; set; }

        // 新会员波币检查条件
        public AmountConfig NewBobi { get; set; }

        // 老会员波币检查条件
        public AmountConfig OldBobi { get; set; }

        // 每笔最大额度
        public decimal OrderAmountMax { get; set; }

        public ReviewConfig()
        {
        }

        public AmountConfig GetAmountConfig(string way, bool isNew)
        {
            if (way == "银行卡")
            {
                return isNew ? NewBank : OldBank;
            }
            else if (way == "数字钱包")
            {
                return isNew ? NewBobi : OldBobi;
            }
            return null;
        }

        public static ReviewConfig FromYamlFile(string path)
        {
            string yml = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder().Build();
            var cnf = deserializer.Deserialize<ReviewConfig>(yml);
            return cnf;
        }

	}
}

