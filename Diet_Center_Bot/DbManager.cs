using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Diet_Center_Bot
{
    class DbManager : System.IDisposable
    {
        string dataBaseName;
        public Dictionary<string, string> Questionary { private set; get; }

        public DbManager(string dataBaseName)
        {
            if (File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\DB.db"))
                this.dataBaseName = dataBaseName;
            else
                throw new FileNotFoundException("No such database! Check path to it!");
            if (!CheckDbTablesAsync().GetAwaiter().GetResult())
                throw new SQLiteException("No needed tables in dataBase! Please, update it!");

            Questionary = getQuestionaryFromDb();
        }

        private Dictionary<string, string> getQuestionaryFromDb()
        {
            var tmp = new Dictionary<string, string>();


            using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Questionary;", conn);
                using (var reader = command.ExecuteReader())
                {
                    foreach (DbDataRecord record in reader)
                    {
                        tmp.Add(record.GetValue(1).ToString(), record.GetValue(2).ToString());
                    }
                }
            }

            return tmp;
        }

        private async Task<bool> CheckDbTablesAsync()
        {
            using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
            {
                await conn.OpenAsync();
                SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE TYPE = 'table'; ", conn);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    foreach (DbDataRecord record in reader)
                    {
                        if (record.GetValue(0).ToString().Contains("AuthorizationData"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public async Task<string> getUserLastActAsync(string login_telegram)
        {
            string act = "-1";
            using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
            {
                await conn.OpenAsync();
                //SQLiteCommand command = new SQLiteCommand("SELECT last_act  FROM AuthorizationData WHERE telegram_login = @login_telegram;", conn);
                //command.Parameters.Add(new SQLiteParameter("@login_telegram", login_telegram));
                SQLiteCommand command = new SQLiteCommand("SELECT last_act FROM AuthorizationData where telegram_login = @login;", conn);
                command.Parameters.Add(new SQLiteParameter("@login", login_telegram));
                using (var reader = await command.ExecuteReaderAsync())
                {
                    foreach (DbDataRecord record in reader)
                    {
                        act = record["last_act"].ToString();
                    }
                }
            }
            return act;
        }
        #region добавление пользователей

        public async Task<bool> AddUser(string telegram_login)
        {
            SQLiteConnection conn;
            bool isOk = false;
            try
            {
                using (conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
                {
                    await conn.OpenAsync();
                    SQLiteCommand command = new SQLiteCommand("INSERT INTO AuthorizationData (telegram_login, last_act)  VALUES (@telegram_login, 'sign_in');", conn);
                    command.Parameters.Add(new SQLiteParameter("@telegram_login", telegram_login));
                    command.CreateParameter();

                    await command.ExecuteNonQueryAsync();
                }
                isOk = !isOk;
            }
            catch (System.Exception ex)
            { }
            return isOk;
        }
        public async Task<bool> AddUserLogin(string login, string telegram_login)
        {
            SQLiteConnection conn;
            bool isOk = false;
            try
            {
                using (conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
                {
                    await conn.OpenAsync();
                    SQLiteCommand command = new SQLiteCommand("UPDATE AuthorizationData SET login = @log, last_act = 'register' WHERE telegram_login = @telegram_login", conn);
                    command.Parameters.Add(new SQLiteParameter("@telegram_login", telegram_login));
                    command.Parameters.Add(new SQLiteParameter("@log", login));
                    command.CreateParameter();
                    command.ExecuteNonQueryAsync().GetAwaiter().GetResult();
                    isOk = !isOk;
                }
            }
            catch (System.Exception ex)
            { }
            return isOk;
        }

        #endregion

        public void Dispose()
        {
            try
            {

            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }
    }
}
