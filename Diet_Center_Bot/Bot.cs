using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Diet_Center_Bot
{
    public class Bot
    {
        static private DbManager db;
        static string PATH;
        public static Bot bot;
        static TelegramBotClient client;
        private Bot()
        {
            PATH = Directory.GetCurrentDirectory() + "\\files\\";
        }

        public static Bot getBot()
        {
            if (client == null)
            {
                client = new TelegramBotClient("652792663:AAFxU3zISJKqaBHGhJ3CvRnNhivIf7VCVng");
                client.OnMessage += eventMessage;
                client.OnCallbackQuery += eventCallBack;
                db = new DbManager(System.IO.Directory.GetCurrentDirectory() + "\\DB.db");
            }

            return new Bot();
        }

        private static void eventCallBack(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                var a = new InputOnlineFile(new MemoryStream(File.ReadAllBytes(PATH + e.CallbackQuery.Data)), e.CallbackQuery.Data);
                client.SendDocumentAsync(e.CallbackQuery.Message.Chat.Id, a);
            }
            catch (Exception ex)
            {

            }

        }

        public void startReceiving()
        {
            do
            {
                client.StartReceiving();
            } while (client.IsReceiving == false);
        }
        private static async void eventMessage(object sender, MessageEventArgs e)
        {

            //        var buttonItem = new[] { "one", "two", "three", "Four" };


            //        string[] allFileNames = Directory.EnumerateFiles(PATH)
            //.Select(System.IO.Path.GetFileName)
            //.ToArray();

            //        int[] lines = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            //        int counter = 4;
            //        var b = allFileNames.GroupBy(_ => counter++ / 4).Select(v => v.ToArray());

            //        // Console.WriteLine(String.Join("\n", b.Select(v => String.Join(" ", v))));
            //        Console.WriteLine(b.Count()) ;

            if (e.Message.Text.Contains("/start"))
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Это внутренний бот компании Диет центр\n Если вы наш раб нажмите /login для входа\n" +
                    "если нет - то ок");
            }
            if (e.Message.Text.Contains("login_"))
            {
                string login = e.Message.Text.Substring(e.Message.Text.IndexOf('_') + 1);

                if (db.GetLogins(login).GetAwaiter().GetResult() > 0)
                {
                    e.Message.Text = "/files";
                }
                else
                {
                    await db.AddUserLogin(login);
                    await client.SendTextMessageAsync(e.Message.Chat.Id, login + ", register! press /files");
                }

            }
            if (e.Message.Text.Contains("/login"))
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Enter telegram login (can start from login_)");

            }
            if (e.Message.Text.Contains("/files"))
            {
                var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboard());
                await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Text, replyMarkup: keyboardMarkup);
            }



            //await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Text);
        }

        private static void RenameAllFiles()
        {

            var di = new DirectoryInfo(PATH).GetFiles();
            string tempExtension, newpath, name;
            int i = 0;
            foreach (var fi in di)
            {
                name = Path.GetFileNameWithoutExtension(fi.FullName);

                if (name.Length > 20)
                {
                    name = name.Substring(0, name.Length - (name.Length - 30));
                    tempExtension = fi.Extension;
                    newpath = fi.DirectoryName + "\\" + name + i.ToString() + tempExtension;
                    File.Move(fi.FullName, newpath);
                    i++;
                }
            }
        }
        private static InlineKeyboardButton[][] GetInlineKeyboard()
        {
            RenameAllFiles();

            string[] allFileNames = Directory.EnumerateFiles(PATH)
    .Select(System.IO.Path.GetFileName)
    .ToArray();

            //for (int i = 0; i < allFileNames.Length; i++)
            //{
            //    Console.WriteLine(allFileNames[i]);
            //}

            int counter = 1;
            var b = allFileNames.GroupBy(_ => counter++ / 1).Select(v => v.ToArray());

            var keyboardInline = new InlineKeyboardButton[b.Count()][];
            Console.WriteLine(b.Count().ToString());
            for (int i = 0; i < b.Count(); i++)
            {
                var keyboardButtons = new InlineKeyboardButton[1];
                //for (var j = 0; j < 1; j++)
                //{
                keyboardButtons[0] = new InlineKeyboardButton
                {
                    Text = allFileNames[i],
                    CallbackData = allFileNames[i].ToString(),
                };
                //    Console.WriteLine();

                //}

                keyboardInline[i] = keyboardButtons;
            }

            return keyboardInline;
        }

    }
}
