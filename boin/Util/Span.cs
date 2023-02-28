﻿using System;

namespace boin.Util;

public class Span : IDisposable
{
    long start = DateTime.UtcNow.Ticks;
    public string Msg { get; set; }

    public Span()
    {
    }

    public Span(string msg)
    {
        this.Msg = msg;
    }

    public void Dispose()
    {
        if (Msg != null)
        {
            var s = (DateTime.UtcNow.Ticks - start) / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("span:" + s.ToString() + "ms; " + Msg);
        }
    }
}

