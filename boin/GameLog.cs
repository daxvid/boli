using System;
using OpenQA.Selenium;

namespace boin
{
    // 游戏日志
    public class GameLog
    {
        // 编号ID
        public string LogId { get; set; } = "";

        // 游戏ID	
        public string GameId { get; set; } = "";

        // 游戏平台	
        public string GamePlatform { get; set; } = "";

        // 单号
        public string No { get; set; } = "";

        // 游戏名
        public string GameName { get; set; } = "";

        // 操作时间
        public string OpTime { get; set; } = "";

        // 下注总金额
        public Decimal TotalBet { get; set; } 

        // 中奖金额
        public Decimal TotalWin { get; set; } 

        // 有效下注
        public Decimal ValidBet { get; set; }

        // 小费
        public Decimal Tip { get; set; }

        // 取消下注
        public string CancelBet { get; set; } = "";

        public GameLog()
        {
        }


        public static GameLog Create(Dictionary<string, string> head, IWebElement element)
        {
            var row = Table.Ele2Dic(element);

            GameLog log = new GameLog();
            log.LogId = Table.ReadString(head, "编号ID", row);
            log.GameId = Table.ReadString(head, "游戏ID", row);
            log.GamePlatform = Table.ReadString(head, "游戏平台", row);
            log.No = Table.ReadString(head, "单号", row);
            log.GameName = Table.ReadString(head, "游戏名", row);
            log.OpTime = Table.ReadString(head, "操作时间", row);
            log.TotalBet = Table.ReadDecimal(head, "下注总金额", row);
            log.TotalWin = Table.ReadDecimal(head, "中奖金额", row);
            log.ValidBet = Table.ReadDecimal(head, "有效下注", row);
            log.Tip = Table.ReadDecimal(head, "小费", row);
            log.CancelBet = Table.ReadString(head, "取消下注", row);

            return log;
        }
    }
}

