using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Management.Instrumentation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;
using MySql;
using MySql.Data.MySqlClient;

namespace HookappServer
{
    public class Database
    {
        private MySqlConnection _connection;

        class Config
        {
            public static string Server { get; set; } = "localhost";

            public static int Port { get; set; } = 3306;

            public static string Username { get; set; } = "root";

            public static string Password { get; set; } = "";

            public static string Database { get; set; } = "mygym";
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(string.Format("Server={0};Port={1};Username={2};Password={3};Database={4};SslMode=none",
                    Config.Server, Config.Port, Config.Username, Config.Password, Config.Database));
        }

        public bool Connect()
        {
            try
            {
                _connection = GetConnection();

                _connection.Open();
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                    return true;
                }
                else
                {
                    Print.Error($"Database connection state: {_connection.State}");
                }
            }
            catch (Exception ex)
            {
                Print.Error($"Unable to connect to the database. Error: {ex.Message}");
            }

            return false;
        }

        public ConfirmCodeUserObject GetConfirmedUser(string code)
        {
            ConfirmCodeUserObject toReturn = null;
            long userID = -1;

            using (var conn = GetConnection())
            {
                conn.Open();

                using (var com = new MySqlCommand("SELECT * FROM unconfirmed_pool WHERE code = @code", conn))
                {
                    com.Parameters.Add("@code", MySqlDbType.VarChar).Value = code;

                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime expiryDate = reader.GetDateTime(3);

                            bool expired = DateTime.Now >= expiryDate;

                            if (!expired)
                            {
                                userID = reader.GetInt64(4);
                                break;
                            }
                        }
                    }
                }

                if(userID >= 0)
                {
                    using (var com = new MySqlCommand("SELECT * FROM users WHERE user_id = @userID", conn))
                    {
                        com.Parameters.Add("@userID", MySqlDbType.Int64).Value = userID;

                        using (var reader = com.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                toReturn = new ConfirmCodeUserObject { Name = reader.GetString(1), UserID = reader.GetInt64(0), Email = reader.GetString(6) };
                            }
                        }
                    }
                }
            }

            return toReturn;
        }

        public bool ConfirmUser(ConfirmUserPayload confirm)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var com = new MySqlCommand("INSERT INTO registered_users (user_id,google_id) VALUES (@userID,@googleID)", conn))
                {
                    com.Parameters.Add("@userID", MySqlDbType.Int64).Value = confirm.UserID;
                    com.Parameters.Add("@googleID", MySqlDbType.VarChar).Value = confirm.GoogleToken;

                    return com.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CanRegisterUser(string jmbg)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var com = new MySqlCommand("SELECT COUNT(*) FROM users WHERE jmbg = @jmbg", conn))
                {
                    com.Parameters.Add("@jmbg", MySqlDbType.VarChar).Value = jmbg;

                    long affected = (long)com.ExecuteScalar();

                    return affected <= 0;
                }
            }
        }

        public long RegisterUser(RegisterUserObject user)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var com = new MySqlCommand("INSERT INTO users (name,surname,address,jmbg,mobile,email,gender,picture_url,id_country_code,phone_extension) VALUES (@name,@surname,@address,@jmbg,@mobile,@email,@gender,@pic,@idcc,@ext)",conn))
                {
                    com.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.Name;
                    com.Parameters.Add("@pic", MySqlDbType.VarChar).Value = "NOT SET";
                    com.Parameters.Add("@surname", MySqlDbType.VarChar).Value = user.Surname;
                    com.Parameters.Add("@address", MySqlDbType.VarChar).Value = user.Address;
                    com.Parameters.Add("@jmbg", MySqlDbType.VarChar).Value = user.JMBG;
                    com.Parameters.Add("@mobile", MySqlDbType.VarChar).Value = user.Mobile;
                    com.Parameters.Add("@email", MySqlDbType.VarChar).Value = user.Email;
                    com.Parameters.Add("@gender", MySqlDbType.Byte).Value = user.Gender;
                    com.Parameters.Add("@idcc", MySqlDbType.VarChar).Value = user.CountryCode;
                    com.Parameters.Add("@ext", MySqlDbType.VarChar).Value = user.PhoneExtension;

                    com.ExecuteNonQuery();
                    return com.LastInsertedId;
                }
            }
        }

        public bool AddToUnconfirmedPool(string code,string mobile,DateTime validUntil, long userID)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var com = new MySqlCommand("INSERT INTO unconfirmed_pool (code,mobile,valid_until,user_id) VALUES (@code,@mobile,@valid_until,@user_id)", conn))
                {
                    com.Parameters.Add("@mobile", MySqlDbType.VarChar).Value = mobile;
                    com.Parameters.Add("@user_id", MySqlDbType.Int64).Value = userID;
                    com.Parameters.Add("@code", MySqlDbType.VarChar).Value = code;
                    com.Parameters.Add("@valid_until", MySqlDbType.DateTime).Value = validUntil;

                    return com.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
