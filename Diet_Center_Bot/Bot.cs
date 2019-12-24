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
                client = new TelegramBotClient("987477165:AAGJmiDWuSgz-nkiZitBk8qE_PLTjTDUUN8");
                client.OnMessage += eventMessage;
                client.OnCallbackQuery += eventCallBack;
                db = new DbManager(System.IO.Directory.GetCurrentDirectory() + "\\DB.db");
                ////373162664
                //foreach (var item in db.getUsersChats())
                //{
                //    client.SendTextMessageAsync(item, "ASD");
                //}

            }

            return new Bot();
        }

        private static void eventCallBack(object sender, CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Data.Contains("question_"))
            {
                string s = db.Questionary[e.CallbackQuery.Data.Substring("question_".Length)];
                client.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, s);
                return;
            }
            try
            {
                var a = new InputOnlineFile(new MemoryStream(File.ReadAllBytes(PATH + e.CallbackQuery.Data)), e.CallbackQuery.Data);
                client.SendDocumentAsync(e.CallbackQuery.Message.Chat.Id, a);
            }
            catch (Exception ex)
            {

            }

        }

        public static void StopBot()
        {
            if (client != null)
                client.StopReceiving();
            if (db != null)
                db.Dispose();
            if (PATH != null)
                PATH = String.Empty;
            GC.Collect();
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


            if (e.Message.Text.Contains("/start"))
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Здравствуйте " + e.Message.Chat.FirstName + ", Вас приветствует DietCenter.\nДля регистрации необходимо нажать /login" +
                                                                     "\n\nHello " + e.Message.Chat.FirstName + ", DietCenter welcomes you.\nTo register, click /login");
                Console.WriteLine(e.Message.Chat.Id);
            }

            if (e.Message.Text.Contains("/help"))
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Список команд бота:\n/files - файлы\n/questions - вопросы и ответы" +
                    "\n\nList of bot commands: \n/files - files \n/questions - questions and answers");
            }

            if (e.Message.Text.ToString().ToLower().Contains("login_"))
            {
                if (await db.getUserLastActAsync(e.Message.Chat.Username) == "sign_in")
                {
                    string l = e.Message.Text.Substring(e.Message.Text.IndexOf('_') + 1);
                    await db.AddUserLogin(l, e.Message.Chat.Username, e.Message.Chat.Id);
                    await client.SendTextMessageAsync(e.Message.Chat.Id, $"{e.Message.Chat.FirstName}, Вы зарегистрировались под именем пользователя telegram {e.Message.Chat.Username}! Пожалуйста нажмите /help " +
                        $"для просмотра команд бота" +
                        $"\n\n{e.Message.Chat.FirstName}, You are registered under the telegram username {e.Message.Chat.Username}! Please click /help to view bot commands");
                }
                else
                {
                    await client.SendTextMessageAsync(e.Message.Chat.Id, $"{e.Message.Chat.FirstName}, Вы уже авторизованы! Пожалуйста нажмите /help для просмотра команд бота" +
                                                                            $"\n\n{e.Message.Chat.FirstName}, You are already logged in! Please click /help to view bot commands");
                }
                string s = await db.getUserLastActAsync(e.Message.Chat.Username.ToString());
            }

            //if (e.Message.Text.Contains("Login_"))
            //{
            //    if (await db.getUserLastActAsync(e.Message.Chat.Username) == "sign_in")
            //    {
            //        string l = e.Message.Text.Substring(e.Message.Text.IndexOf('_') + 1);
            //        await db.AddUserLogin(l, e.Message.Chat.Username);
            //        await client.SendTextMessageAsync(e.Message.Chat.Id, $"{e.Message.Chat.FirstName}, Вы зарегистрировались под именем пользователя telegram {e.Message.Chat.Username}! Пожалуйста нажмите /help " +
            //            $"для просмотра команд бота" +
            //            $"\n\n{e.Message.Chat.FirstName}, You are registered under the telegram username {e.Message.Chat.Username}! Please click /help to view bot commands");
            //    }
            //    else
            //    {
            //        await client.SendTextMessageAsync(e.Message.Chat.Id, $"{e.Message.Chat.FirstName}, Вы уже авторизованы! Пожалуйста нажмите /help для просмотра команд бота" +
            //                                                                $"\n\n{e.Message.Chat.FirstName}, You are already logged in! Please click /help to view bot commands");
            //    }
            //    string s = await db.getUserLastActAsync(e.Message.Chat.Username.ToString());
            //}

            if (e.Message.Text.Contains("/login"))
            {
                try
                {
                    if (await db.getUserLastActAsync(e.Message.Chat.Username) == "-1")
                        await db.AddUser(e.Message.Chat.Username.ToString());
                    await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Chat.FirstName + ", введите имя пользователя telegram (нужно начать с login_ или Login_ )" +
                        "\n\n" + e.Message.Chat.FirstName + ", enter telegram username (can start from  login_ or Login_ )");
                }
                catch (System.Exception ex)
                {
                    await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Chat.FirstName + ", у Вас не указано имя пользователя telegram. Для регистрации пожалуйста укажите имя пользователя в " +
                                                                                                                                                    "вашем профиле телеграмм и нажмите /login" +
                                                        "\n\n" + e.Message.Chat.FirstName + ", You do not have a telegram username. To register, please enter the username in your telegram profile and click /login");
                }

            }
            if (e.Message.Text.Contains("/files"))
            {
                if (await db.getUserLastActAsync(e.Message.Chat.Username) == "register")
                {
                    if (Directory.Exists(PATH) && (new DirectoryInfo(PATH).GetFiles().Length > 0))
                    {
                        var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboardFiles());
                        await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Text, replyMarkup: keyboardMarkup);
                    }
                    else
                    {
                        await client.SendTextMessageAsync(e.Message.Chat.Id, "Пока файлов нет...");
                    }
                }
                else
                {
                    await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Chat.FirstName + ", для использования команды /files необходимо зарегистрироваться. Для регистрации нажмите на команду /login" +
                                                        "\n\n" + e.Message.Chat.FirstName + ", you must register to use the /files command. To register, click on the command /login");
                }
            }
            if (e.Message.Text.Contains("/questions"))
            {
                if (await db.getUserLastActAsync(e.Message.Chat.Username) == "register")
                {
                    var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboardQuestionary());
                    await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Chat.FirstName + ", для Вас представлен ряд вопросов на которые Вы можете получить ответы"
                        + "\n\n" + e.Message.Chat.FirstName + ", a number of questions are presented for you to which you can get answers", replyMarkup: keyboardMarkup);
                }
                else
                {
                    await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Chat.FirstName + ", для использования команды /questions необходимо зарегистрироваться. Для регистрации нажмите на команду /login" +
                                                        "\n\n" + e.Message.Chat.FirstName + ", you must register to use the /questions command. To register, click on the command /login");
                }
            }
            //await client.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Text);
        }
        public static async void SendMessageToBot(string message)
        {
            if (message.Length > 0)
                foreach (var item in db.getUsersChats())
                {
                    await client.SendTextMessageAsync(item, message);
                }
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
        private static InlineKeyboardButton[][] GetInlineKeyboardQuestionary()
        {
            var keyboardInline = new InlineKeyboardButton[db.Questionary.Count][];
            for (int i = 0; i < db.Questionary.Count; i++)
            {
                var keyboardButtons = new InlineKeyboardButton[1];
                keyboardButtons[0] = new InlineKeyboardButton
                {
                    Text = db.Questionary.Keys.ElementAt(i),
                    CallbackData = ("question_" + db.Questionary.Keys.ElementAt(i))
                };
                keyboardInline[i] = keyboardButtons;
            }

            return keyboardInline;
        }
        private static InlineKeyboardButton[][] GetInlineKeyboardFiles()
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
