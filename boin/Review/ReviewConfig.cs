using System;
using YamlDotNet.Serialization;
using boin.Util;

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
    //波音没绑姓名的话不给予通过
    //当日充值和提款总额两者都超过四万的人工审核
    //玩百家乐的会员 波币一天不超过3000提款
    //最近10笔提款内 波币不能超过4笔
    //前一笔是失败的订单机器人不要通过，让我们人工审核
    
    // 针对充值接口显示『客单充值(不能删)』
    // 计算日期:上一笔提款日-最新一笔提款日
    // 金额>2000-人工审核
    // 金额<2000，笔数小于3笔的-可以机器人审核，笔数>三笔的-人工审核
    
    public class AmountConfig
    {
        // 当日充值达多少人工审核
        public decimal DayRecharge { get; set; }

        // 每日最大额度
        public decimal DayMax { get; set; }

        // 每笔最大额度
        public decimal OnceMax { get; set; }

        // 不可以通过的游戏
        public Dictionary<string, List<string>> BanGames { get; set; }
        
        // 玩某个游戏的提款限制， 玩百家乐的会员，波币一天不超过3000提款
        public Dictionary<string, decimal> DayMaxGames{ get; set; }
        
        public string ExistsGame(string platform, string game)
        {
            foreach (var kv in BanGames)
            {
                var k = kv.Key;
                foreach (var ban in kv.Value)
                {
                    if (!string.IsNullOrEmpty(ban))
                    {
                        if (game.Contains(ban))
                        {
                            if (k == "all")
                            {
                                return ban;
                            }
                            else if (k == platform)
                            {
                                return k + ban;
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }

        
        public void Init()
        {
            var newBans = new Dictionary<string, List<string>>();
            foreach (var kv in BanGames)
            {
                var key = kv.Key;
                for (var i = 0; i < kv.Value.Count; i++)
                {
                    kv.Value[i] = StringUtility.TW2ZH(kv.Value[i]);
                }
                newBans.Add(key, kv.Value);
            }
            BanGames = newBans;
        }
    }

    public class ReviewConfig
    {
        public static ReviewConfig Cnf;
        
        // 新会员银行卡检查条件
        public AmountConfig NewBank { get; set; }

        // 老会员银行卡检查条件
        public AmountConfig OldBank { get; set; }

        // 新会员波币检查条件
        public AmountConfig NewBobi { get; set; }

        // 老会员波币检查条件
        public AmountConfig OldBobi { get; set; }

        // 每笔最大额度
        public int OrderAmountMax { get; set; }
        
        // 最近10笔提款内 波币不能超过4笔
        public int NearWithdrawCount { get; set; }
        public int BobiMaxCount { get; set; }
        
        // 针对充值接口显示『客单充值(不能删)』
        // 计算日期:上一笔提款日-最新一笔提款日
        // 金额>2000-人工审核
        // 金额<2000，笔数小于3笔的-可以机器人审核，笔数>三笔的-人工审核
        public Dictionary<string, List<decimal>> RechargeChannel { get; set; }

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
            cnf.NewBank.Init();
            cnf.NewBobi.Init();
            cnf.OldBank.Init();
            cnf.OldBobi.Init();
            Cnf = cnf;
            return cnf;
        }

	}
}

