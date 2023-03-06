
using System;
using StackExchange.Redis;

namespace boin.Util;

public class Cache
{
    static ConnectionMultiplexer redis;
    static IDatabase db;
    static object locker = new object();
    public static string Platform;

    public static void Init(string strConn = "localhost", string platform = "")
    {
        Platform = platform;
        lock (locker)
        {
            redis = ConnectionMultiplexer.Connect(strConn);
            db = redis.GetDatabase();
        }
    }

    public static void SaveBank(string card, string msg)
    {
        var key = "b." + card;
        lock (locker)
        {
            db.StringSet(key, msg, TimeSpan.FromDays(30));
        }
    }

    public static string GetBank(string card)
    {
        var key = "b." + card;
        lock (locker)
        {
            string value = db.StringGet(key);
            return value;
        }
    }

    public static void SaveOrder(string orderId, string msg)
    {
        var key = Platform + ".o." + orderId;
        lock (locker)
        {
            db.StringSet(key, msg, TimeSpan.FromDays(3));
        }
    }

    public static string GetOrder(string orderId)
    {
        var key = Platform + ".o." + orderId;
        lock (locker)
        {
            string value = db.StringGet(key);
            return value;
        }
    }

    public static void SaveRecharge(string card, string msg)
    {
        var key = Platform + ".r." + card;
        lock (locker)
        {
            db.StringSet(key, msg, TimeSpan.FromDays(60));
        }
    }

    public static string GetRecharge(string card)
    {
        var key = Platform + ".r." + card;
        lock (locker)
        {
            string value = db.StringGet(key);
            return value;
        }
    }


    public static void SaveGameBind(string card, string msg)
    {
        var key = Platform + ".gb." + card;
        lock (locker)
        {
            db.StringSet(key, msg, TimeSpan.FromDays(2));
        }
    }

    public static string GetGameBind(string card)
    {
        var key = Platform + ".gb." + card;
        lock (locker)
        {
            string value = db.StringGet(key);
            return value;
        }
    }
}
