using System;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace boin.Bot;

public class TelegramBot
{
    private static readonly TelegramBot instance = new TelegramBot();

    /// <summary>
    /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
    /// </summary>
    static TelegramBot()
    {
    }

    private TelegramBot()
    {
    }

    public static TelegramBot Instance
    {
        get { return instance; }
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
            SendMessage("start bot:" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
            while (true)
            {
                try
                {
                    update(client);
                }
                catch
                {
                    try
                    {
                        client = new BotClient(cnf.BotToken);
                        this.api = client;
                        SendMessage("restart bot:" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
                    }
                    catch
                    {
                    }
                }
            }
        });
    }

    static void update(BotClient client)
    {
        var updates = client.GetUpdates();
        while (true)
        {
            if (updates.Any())
            {
                foreach (var update in updates)
                {
                    if (update.Message != null && update.Message.Chat != null)
                    {
                        long chatId = update.Message.Chat.Id; // Target chat Id
                        try
                        {
                            client.SendMessage(chatId, "ok" + chatId.ToString());
                        }
                        catch
                        {
                        }
                    }
                }

                var offset = updates.Last().UpdateId + 1;
                updates = client.GetUpdates(offset);
            }
            else
            {
                updates = client.GetUpdates();
                Thread.Sleep(10);
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
        lock (instance)
        {
            try
            {
                instance.SendMessage(msg);
            }
            catch
            {

            }
        }
    }
}
