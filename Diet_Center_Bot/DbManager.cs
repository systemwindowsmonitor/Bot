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
        public DbManager(string dataBaseName)
        {
            if (File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\DB.db"))
                this.dataBaseName = dataBaseName;
            else
                throw new FileNotFoundException("No such database! Check path to it!");
            if (!CheckDbTablesAsync().GetAwaiter().GetResult())
                throw new SQLiteException("No needed tables in dataBase! Please, update it!");
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

        #region взаимодействие с логином/паролем
        public bool CheckLogin(string login)
        {
            return GetLogins(login).GetAwaiter().GetResult() > 0 ? true : false;
        }

        #endregion

        #region методы выборки с таблиц
        public async Task<Dictionary<int, string>> getRegionsAsync()
        {
            Dictionary<int, string> data = new Dictionary<int, string>();
            using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
            {
                await conn.OpenAsync();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Regions;", conn);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    foreach (DbDataRecord record in reader)
                    {
                        data.Add(int.Parse(record.GetValue(0).ToString()), record.GetValue(1).ToString());
                    }
                }
            }
            return data;
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

        public async Task<List<User>> getUsers()
        {
            List<User> data = new List<User>();
            using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
            {
                await conn.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("select Сlient.Id,Сlient.account_name, Сlient.ip, Regions.name from Сlient" +
                    " left join Regions on Regions.id = Сlient.Region", conn);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    foreach (DbDataRecord record in reader)
                    {
                        data.Add(new User(
                            record["id"].ToString(),
                            record["account_name"].ToString(),
                            record["ip"].ToString(),
                            record["name"].ToString()
                        ));
                    }
                }
            }
            return data;
        }



        public async Task<List<string>> GetLogins()
        {
            List<string> data = new List<string>();
            using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
            {
                await conn.OpenAsync();
                SQLiteCommand command = new SQLiteCommand("SELECT login FROM AuthorizationData;", conn);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    foreach (DbDataRecord record in reader)
                    {
                        data.Add(record["login"].ToString());
                    }
                }
            }
            return data;
        }
        public async Task<int> GetLogins(string key)
        {
            try
            {
                List<string> data = new List<string>();
                using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
                {
                    await conn.OpenAsync();
                    SQLiteCommand command = new SQLiteCommand("SELECT id FROM AuthorizationData where login = @login;", conn);
                    command.Parameters.Add(new SQLiteParameter("@login", key));
                    SQLiteDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        return reader.FieldCount;
                }
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
            return 0;
        }
        #endregion

        #region добавление пользователей

        public async Task<bool> AddUser(string telegram_login)
        {
            SQLiteConnection conn;
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
                return true;
            }
            catch (System.Exception ex)
            {
                return false;

            }

            return true;


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
                    SQLiteCommand command = new SQLiteCommand("UPDATE AuthorizationData SET login = 'rus', last_act = 'register' WHERE telegram_login = @telegram_login", conn);
                    command.Parameters.Add(new SQLiteParameter("@login", login));
                    //command.Parameters.Add(new SQLiteParameter("@telegram_login", telegram_login));
                    command.CreateParameter();

                    command.ExecuteNonQueryAsync().GetAwaiter().GetResult();

                    //await conn.OpenAsync();
                    //SQLiteCommand command = new SQLiteCommand("UPDATE AuthorizationData SET login = @login WHERE telegram_login = @telegram_login", conn);
                    //command.Parameters.Add(new SQLiteParameter("@login", login));
                    //command.Parameters.Add(new SQLiteParameter("@telegram_login", telegram_login));
                    //command.CreateParameter();
                    //int a = await command.ExecuteNonQueryAsync();
                   
                        isOk = !isOk;
                }
            }
            catch (System.Exception ex)
            {}
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
