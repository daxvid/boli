
using System;
using StackExchange.Redis;

namespace boin.Util;

public class Cache
{
    static ConnectionMultiplexer redis;
    static IDatabase db;
    static object locker = new object();

    public static void Init(string strConn = "localhost")
    {
        lock (locker)
        {
            redis = ConnectionMultiplexer.Connect(strConn);
            db = redis.GetDatabase();
        }
    }

    public static void SaveOrder(string orderId, string msg)
    {
        lock (locker)
        {
            db.StringSet("o." + orderId, msg, TimeSpan.FromDays(1));
        }
    }

    public static string GetOrder(string orderId)
    {
        lock (locker)
        {
            string value = db.StringGet("o." + orderId);
            return value;
        }
    }

    public static void SaveBank(string card, string msg)
    {
        lock (locker)
        {
            db.StringSet("b." + card, msg, TimeSpan.FromDays(30));
        }
    }

    public static string GetBank(string card)
    {
        lock (locker)
        {
            string value = db.StringGet("b." + card);
            return value;
        }
    }

    public static void SaveRecharge(string card, string msg)
    {
        lock (locker)
        {
            db.StringSet("r." + card, msg, TimeSpan.FromDays(60));
        }
    }

    public static string GetRecharge(string card)
    {
        lock (locker)
        {
            string value = db.StringGet("r." + card);
            return value;
        }
    }


    public static void SaveGameBind(string card, string msg)
    {
        lock (locker)
        {
            db.StringSet("gb." + card, msg, TimeSpan.FromDays(2));
        }
    }

    public static string GetGameBind(string card)
    {
        lock (locker)
        {
            string value = db.StringGet("gb." + card);
            return value;
        }
    }
}
