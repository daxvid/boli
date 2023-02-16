using System;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace boin.Review
{
    public class TelegramBot
    {
        static BotClient api;
        static long defaultChatId;

        public static void Run(string botToken, long id)
        {
            defaultChatId = id;
            api = new BotClient(botToken);
            var me = api.GetMe();
            ThreadPool.QueueUserWorkItem(state =>
            {
                SendMessage("start:" + DateTime.Now.ToString("yyyy-MM-dd hh::mm:ss"));
                run(botToken);
            });
        }

        static void run(string botToken)
        {
            var updates = api.GetUpdates();
            while (true)
            {
                if (updates.Any())
                {
                    foreach (var update in updates)
                    {
                        long chatId = update.Message.Chat.Id; // Target chat Id
                        api.SendMessage(chatId, "Hello World!" + chatId.ToString()); // Send a message
                                                                                     // Process update
                    }
                    var offset = updates.Last().UpdateId + 1;
                    updates = api.GetUpdates(offset);
                }
                else
                {
                    updates = api.GetUpdates();
                }
            }
        }

        static public void SendMessage(long chatId, string msg)
        {
            api.SendMessage(chatId, msg);
        }

        static public void SendMessage(string msg)
        {
            api.SendMessage(defaultChatId, msg);
        }

    }
}
