namespace boin.Util;

using System;
using StackExchange.Redis;

public class Cache
{
    static ConnectionMultiplexer redis;
    static IDatabase db;
    static object _locker = new object();
    static string Platform = string.Empty;

    public static void Init(string strConn = "localhost", string platform = "")
    {
        Platform = platform;
        lock (_locker)
        {
            redis = ConnectionMultiplexer.Connect(strConn);
            db = redis.GetDatabase();
        }
    }

    public static void SaveBank(string card, string msg)
    {
        var key = "b." + card;
        lock (_locker)
        {
            db.StringSet(key, msg, TimeSpan.FromDays(30));
        }
    }

    public static string? GetBank(string card)
    {
        var key = "b." + card;
        lock (_locker)
        {
            string? value = db.StringGet(key);
            return value;
        }
    }

    public static void SaveOrder(string orderId, string msg)
    {
        var key = Platform + ".o." + orderId;
        lock (_locker)
        {
            db.StringSet(key, msg, TimeSpan.FromDays(3));
        }
    }

    public static string? GetOrder(string orderId)
    {
        var key = Platform + ".o." + orderId;
        lock (_locker)
        {
            string? value = db.StringGet(key);
            return value;
        }
    }

    public static void SaveRecharge(string card, string msg)
    {
        var key = Platform + ".r." + card;
        lock (_locker)
        {
            db.StringSet(key, msg, TimeSpan.FromDays(60));
        }
    }

    public static string? GetRecharge(string card)
    {
        var key = Platform + ".r." + card;
        lock (_locker)
        {
            string? value = db.StringGet(key);
            return value;
        }
    }


    public static void SaveGameBind(string card, string msg)
    {
        var key = Platform + ".gb." + card;
        lock (_locker)
        {
            db.StringSet(key, msg, TimeSpan.FromDays(2));
        }
    }

    public static string? GetGameBind(string card)
    {
        var key = Platform + ".gb." + card;
        lock (_locker)
        {
            string? value = db.StringGet(key);
            return value;
        }
    }
}
