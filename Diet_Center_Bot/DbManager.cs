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
        public bool CheckPassword(string crypt_password)
        {
            return GetPasswords(crypt_password).GetAwaiter().GetResult() > 0 ? true : false; ;
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
        public async Task<List<string>> GetPasswords()
        {
            List<string> data = new List<string>();
            using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
            {
                await conn.OpenAsync();
                SQLiteCommand command = new SQLiteCommand("SELECT password FROM AuthorizationData;", conn);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    foreach (DbDataRecord record in reader)
                    {
                        data.Add(record["password"].ToString());
                    }
                }
            }
            return data;
        }
        private async Task<int> GetPasswords(string key)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
                {
                    await conn.OpenAsync();
                    SQLiteCommand command = new SQLiteCommand("SELECT id FROM AuthorizationData where password = @password;", conn);
                    command.Parameters.Add(new SQLiteParameter("@password", key));
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

        public async Task<bool> AddUserLogin(string login)
        {
            SQLiteConnection conn;
            try
            {
                using (conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
                {
                    await conn.OpenAsync();
                    SQLiteCommand command = new SQLiteCommand("INSERT INTO AuthorizationData (login)  VALUES (@login);", conn);
                    command.Parameters.Add(new SQLiteParameter("@login", login));
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

        public async Task<bool> AddUserPassword(string login, string pass)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
                {
                    await conn.OpenAsync();
                    SQLiteCommand command = new SQLiteCommand("UPDATE AuthorizationData SET password = @password WHERE login = @login ;", conn);
                    command.Parameters.Add(new SQLiteParameter("@login", login));
                    command.Parameters.Add(new SQLiteParameter("@password", pass));
                    command.CreateParameter();

                    command.ExecuteNonQueryAsync().GetAwaiter().GetResult();
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
           
        }

        /// <summary>
        /// ДОБАВЛЯЕТ НОВЫЕ ДАННЫЕ АВТОРИЗАЦИИ И ВОЗРАЩАЕТ ID ЗАПИСИ
        /// </summary>
        /// <param name="login"></param>
        /// <param name="pass"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<int> AddAutorizationDataAsync(string login, string pass, string role)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(string.Format($"Data Source={dataBaseName};")))
                {
                    await conn.OpenAsync();
                    SQLiteCommand command = new SQLiteCommand("INSERT INTO AuthorizationData (login,password,role)VALUES(@login,@password,@role);", conn);
                    command.Parameters.Add(new SQLiteParameter("@login", login));
                    command.Parameters.Add(new SQLiteParameter("@password", pass));
                    command.Parameters.Add(new SQLiteParameter("@role", role));
                    command.CreateParameter();
                    if (command.ExecuteNonQueryAsync().GetAwaiter().GetResult() > 0)
                    {
                        command = new SQLiteCommand("SELECT MAX(Id) From AuthorizationData", conn);

                        var i = command.ExecuteReaderAsync();
                        if (await i.GetAwaiter().GetResult().ReadAsync())
                        {
                            return int.Parse(i.GetAwaiter().GetResult().GetValue(0).ToString());
                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                return -1;
            }
            return -1;
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
