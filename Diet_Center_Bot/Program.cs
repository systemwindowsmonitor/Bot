using System;
using Telegram.Bot;

namespace Diet_Center_Bot
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Bot.getBot().startReceiving();
            Console.Read();
        }

        
    }
}
