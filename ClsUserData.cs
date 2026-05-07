using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.SymbolStore;

namespace DVLD.DataAccess
{
    public class ClsUserData
    {
        public static bool IsUserExists(int UserID)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT Found = 1 FROM Users WHERE UserID = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            IsFound = reader.HasRows;
                        }
                    }
                }
            }
            catch
            {
                return IsFound;
            }
            return IsFound;
        }

        public static bool IsUserExists(string Username)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT Found = 1 FROM Users WHERE Username = @Username";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = Username;
                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            IsFound = Reader.HasRows;
                        }
                        
                        return IsFound;
                    }
                }
            }
            catch { return IsFound; }
        }

        public static bool Find(int UserID, ref int PersonID, ref string Username, ref string Password, ref int Permissions, ref bool IsActive)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM Users WHERE UserID = @UserID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                PersonID = (int)Reader["PersonID"];
                                Username = Reader["Username"].ToString();
                                Password = Reader["Password"].ToString();
                                Permissions = (int)Reader["Permissions"];
                                IsActive = (bool)Reader["IsActive"];
                            }
                        }
                    }
                }
            }
            catch { return IsFound; }

            return IsFound;
        }

        public static bool Find(string Username, ref int UserID, ref int PersonID, ref string Password, ref int Permissions, ref bool IsActive)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM Users WHERE Username = @Username";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = Username;
                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                UserID = (int)Reader["UserID"];
                                PersonID = (int)Reader["PersonID"];
                                Password = Reader["Password"].ToString();
                                Permissions = (int)Reader["Permissions"];
                                IsActive = (bool)Reader["IsActive"];
                            }
                        }
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }

        public static DataTable GetAllUsers()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT  Users.UserID,
                                     Users.PersonID,
                                     People.FirstName + ' ' + People.SecondName + 
                                     ' ' + People.ThirdName + ' ' + People.LastName AS FullName,
                                     Users.Username,
                                     Users.IsActive,
                                     Users.Permissions
                             FROM    Users
                             INNER JOIN People 
                                     ON Users.PersonID = People.PersonID
                             ORDER BY People.FirstName";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();
                        if (Reader.HasRows)
                            dt.Load(Reader);
                    }
                }
            }
            catch { return dt; }
            return dt;
        }

        public static int AddNewUser(int PersonID, string Username,  string Password, int Permissions, bool IsActive)
        {
            int UserID = -1;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"INSERT INTO Users (PersonID, Username, Password, Permissions, IsActive)
                             VALUES (@PersonID, @Username, @Password, @Permissions, @IsActive)
                             SELECT SCOPE_IDENTITY();";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                        Command.Parameters.Add("@Username", SqlDbType.NVarChar, 60).Value = Username;
                        Command.Parameters.Add("@Password", SqlDbType.NVarChar, 200).Value = Password;
                        Command.Parameters.Add("@Permissions", SqlDbType.Int).Value = Permissions;
                        Command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = IsActive;

                        object Result = Command.ExecuteScalar();

                        if (Result != null && Result != DBNull.Value &&
                            int.TryParse(Result.ToString(), out int InsertedID))
                        {
                            UserID = InsertedID;
                        }
                    }
                }
            }
            catch { return UserID; }
            return UserID;
        }

        public static bool UpdateUser(int UserID, int PersonID, string Username, string Password, int Permissions, bool IsActive)
        {
            int RowsAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"UPDATE Users
                             SET    PersonID    = @PersonID,
                                    Username    = @Username,
                                    Password    = @Password,
                                    Permissions = @Permissions,
                                    IsActive    = @IsActive
                             WHERE  UserID      = @UserID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
                        Command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                        Command.Parameters.Add("@Username", SqlDbType.NVarChar, 60).Value = Username;
                        Command.Parameters.Add("@Password", SqlDbType.NVarChar, 200).Value = Password;
                        Command.Parameters.Add("@Permissions", SqlDbType.Int).Value = Permissions;
                        Command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = IsActive;

                        RowsAffected = Command.ExecuteNonQuery();
                        return (RowsAffected > 0);
                    }
                }
            }
            catch { return false; }
        }

        public static bool DeleteUser(int UserID)
        {
            int RowsAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "DELETE FROM Users WHERE UserID = @UserID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
                        RowsAffected = Command.ExecuteNonQuery();
                        return (RowsAffected > 0);
                    }
                }
            }
            catch { return false; }
        }

        public static bool ChangePassword (int UserID , string NewPassword)
        {
            int RowAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"UPDATE Users 
                             SET    Password = @NewPassword
                             WHERE  UserID   = @UserID";

                    Connection.Open();
                    using (SqlCommand Command  = new SqlCommand(Query ,Connection))
                    {
                        Command.Parameters.Add("@UserID" , SqlDbType.Int).Value = UserID;
                        Command.Parameters.Add("@NewPassword", SqlDbType.NVarChar, 200).Value = NewPassword;
                        RowAffected = Command.ExecuteNonQuery();
                        return (RowAffected > 0);
                    }
                }
            }
            catch { return false; }
        }
    }
}
