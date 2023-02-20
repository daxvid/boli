
using System;
using StackExchange.Redis;

namespace boin
{
    public class OrderCache
    {
        ConnectionMultiplexer redis;
        IDatabase db ;


        public OrderCache(string  strConn = "localhost")
        {
            redis = ConnectionMultiplexer.Connect(strConn);
            db = redis.GetDatabase();
        }

        public void Save(string orderId, string msg )
        {
            db.StringSet(orderId, msg, TimeSpan.FromDays(1));
        }

        public string GetOrderInfo(string orderId)
        {
            string value = db.StringGet(orderId);
            return value;
        }
    }
}
