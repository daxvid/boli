namespace boin.Util;

public class Log
{
    public static void Info(Exception err)
    {
        Console.WriteLine(err.Message);
        Console.WriteLine(err.StackTrace);
    }
}