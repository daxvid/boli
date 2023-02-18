using System;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace boin.Bot
{
    public class TelegramBot
    {
        private static readonly TelegramBot instance = new TelegramBot();

        /// <summary>
        /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
        /// </summary>
        static TelegramBot() { }

        private TelegramBot() { }

        public static TelegramBot Instance
        {
            get
            {
                return instance;
            }
        }

        AppConfig cnf;
        BotClient api;

        public void Run(AppConfig cnf)
        {
            this.cnf = cnf;
            var client = new BotClient(cnf.BotToken);
            this.api = client;
            ThreadPool.QueueUserWorkItem(state =>
            {
                SendMessage("start tg:" + DateTime.Now.ToString("yyyy-MM-dd hh::mm:ss"));
                while (true)
                {
                    try
                    {
                        run(api);
                    }
                    catch
                    {
                        client = new BotClient(cnf.BotToken);
                        this.api = client;
                        SendMessage("restart tg:" + DateTime.Now.ToString("yyyy-MM-dd hh::mm:ss"));
                        run(client);
                    }
                }
            });
        }

        void run(BotClient client)
        {
            var updates = client.GetUpdates();
            while (true)
            {
                if (updates.Any())
                {
                    foreach (var update in updates)
                    {
                        long chatId = update.Message.Chat.Id; // Target chat Id
                        client.SendMessage(chatId, "ok" + chatId.ToString());
                    }
                    var offset = updates.Last().UpdateId + 1;
                    updates = client.GetUpdates(offset);
                }
                else
                {
                    updates = client.GetUpdates();
                }
            }
        }

        public void SendMessage(string msg)
        {
            foreach (var charId in cnf.ChatIds)
            {
                api.SendMessage(charId, msg);
            }
        }

        public static void SendMsg(string msg)
        {
            instance.SendMessage(msg);
        }

    }
}
