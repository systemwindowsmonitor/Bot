using System;
using Telegram.Bot;

namespace Diet_Center_Bot
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Bot.getBot().startReceiving();
            string input = String.Empty;
            do
            {
                Console.WriteLine("Input same");
                input = Console.ReadLine();
                if (input.ToLower().Equals("stop"))
                {
                    break;
                }
                Bot.SendMessageToBot(input);
                GC.Collect();
            } while (true);

            Bot.StopBot();
            Console.Read();
        }

        
    }
}
