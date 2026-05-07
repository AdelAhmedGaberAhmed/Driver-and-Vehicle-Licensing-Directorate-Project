using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsPersonData
    {
        public static DataTable GetAllPeople()
        {
            DataTable dt = new DataTable();

            try
            {
                string Qeury = @"SELECT  People.PersonID, 
                         People.NationalNo, 
                         People.FirstName, 
                         People.SecondName, 
                         People.ThirdName, 
                         People.LastName, 
                         People.DateOfBirth,
                         CASE 
                             WHEN People.Gender = 0 THEN 'Male'
                             ELSE 'Female' 
                         END AS Gender,
                         People.Address, 
                         People.Phone, 
                         People.Email, 
                         People.NationalityCountryID, 
                         Countries.CountryName, 
                         People.ImagePath 
                FROM     People 
                INNER JOIN Countries 
                        ON People.NationalityCountryID = Countries.CountryID
                ORDER BY People.FirstName";
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    Connection.Open();

                    using (SqlCommand Command = new SqlCommand(Qeury, Connection))
                    {
                        SqlDataReader Reader = Command.ExecuteReader();

                        if (Reader.HasRows)
                        {
                            dt.Load(Reader);
                        }
                    }

                }

            }
            catch
            {
                return dt;
            }
            return dt;

        }

        public static int AddNewPerson(string NationalNo, string FirstName , string SecondName , string ThirdName ,
            string LastName , DateTime DateOfBirth , bool Gender, string Address , string Phone , string Email ,
            int NationalityCountryID, string ImagePath)
        {
            int PersonID = -1;

            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    Connection.Open();

                    string Query = @"INSERT INTO People (NationalNo ,FirstName , SecondName ,  ThirdName , LastName ,
                                   DateOfBirth , Gender , Address , Phone , Email , NationalityCountryID , ImagePath )
                                    VALUES (@NationalNo ,@FirstName , @SecondName ,  @ThirdName , @LastName ,
                                   @DateOfBirth , @Gender , @Address , @Phone , @Email , @NationalityCountryID , @ImagePath)
                                    SELECT SCOPE_IDENTITY();";

                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@NationalNo",         SqlDbType.NVarChar ,30).Value     = NationalNo;
                        Command.Parameters.Add("@FirstName",         SqlDbType.NVarChar, 30).Value      = FirstName;
                        Command.Parameters.Add("@SecondName",        SqlDbType.NVarChar, 30).Value      = SecondName;
                        Command.Parameters.Add("@ThirdName",         SqlDbType.NVarChar, 30).Value      = ThirdName;
                        Command.Parameters.Add("@LastName",          SqlDbType.NVarChar, 30).Value      = LastName;
                        Command.Parameters.Add("@DateOfBirth",       SqlDbType.DateTime).Value          = DateOfBirth;
                        Command.Parameters.Add("@Gender",            SqlDbType.Bit ).Value              = Gender;
                        Command.Parameters.Add("@Address",           SqlDbType.NVarChar, 300).Value     = Address ;
                        Command.Parameters.Add("@Phone",             SqlDbType.NVarChar, 30).Value      = Phone ;
                        Command.Parameters.Add("@Email",             SqlDbType.NVarChar, 100).Value     = Email ?? (object)DBNull.Value;
                        Command.Parameters.Add("@NationalityCountryID", SqlDbType.Int).Value        = NationalityCountryID;
                        Command.Parameters.Add("@ImagePath",         SqlDbType.NVarChar, 200).Value     = ImagePath ?? (object)DBNull.Value;

                        object Result = Command.ExecuteScalar();

                        if (Result != null && Result != DBNull.Value)
                        {
                            PersonID  = Convert.ToInt32(Result);
                            return PersonID;
                        }
                    }
                }
            }
            catch
            {
                return PersonID;
            }
            return PersonID;
        }



        public static bool UpdatePerson( int PersonID, string NationalNo, string FirstName, string SecondName, string ThirdName,
            string LastName, DateTime DateOfBirth, bool Gender, string Address, string Phone, string Email,
            int NationalityCountryID, string ImagePath)
        {

            
            int RowAffected = 0;
            
            try
            {
                
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"UPDATE People 
                                    SET     NationalNo = @NationalNo  , 
                                            FirstName = @FirstName ,  SecondName = @SecondName  , ThirdName = @ThirdName ,
                                            LastName = @LastName, DateOfBirth = @DateOfBirth ,
                                            Gender = @Gender ,Address = @Address , Phone = @Phone , Email = @Email ,
                                            NationalityCountryID = @NationalityCountryID,
                                            ImagePath = @ImagePath

                                         WHERE PersonID = @PersonID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        
                        Command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                        Command.Parameters.Add("@NationalNo", SqlDbType.NVarChar , 30).Value                = NationalNo;
                        Command.Parameters.Add("@FirstName",         SqlDbType.NVarChar, 30).Value          = FirstName;
                        Command.Parameters.Add("@SecondName",        SqlDbType.NVarChar, 30).Value          = SecondName;
                        Command.Parameters.Add("@ThirdName",         SqlDbType.NVarChar, 30).Value          = ThirdName;
                        Command.Parameters.Add("@LastName",          SqlDbType.NVarChar, 30).Value          = LastName;
                        Command.Parameters.Add("@DateOfBirth",       SqlDbType.DateTime).Value              = DateOfBirth;
                        Command.Parameters.Add("@Gender",            SqlDbType.Bit).Value                   = Gender;
                        Command.Parameters.Add("@Address",           SqlDbType.NVarChar, 300).Value         = Address;
                        Command.Parameters.Add("@Phone",             SqlDbType.NVarChar, 20).Value          = Phone;
                        Command.Parameters.Add("@Email",             SqlDbType.NVarChar, 100).Value         = Email ?? (object) DBNull.Value;
                        Command.Parameters.Add("@ImagePath",         SqlDbType.NVarChar, 200).Value         = ImagePath ?? (object)DBNull.Value ;
                        Command.Parameters.Add("@NationalityCountryID", SqlDbType.Int, 30).Value            = NationalityCountryID;


                        RowAffected = Command.ExecuteNonQuery();
                        return (RowAffected > 0);


                    }
                }

            }
            catch
            {
                return false;
            }

        }

        public static bool Find(string NationalNo, ref int PersonID,ref string FirstName, ref string SecondName, ref string ThirdName,
           ref string LastName, ref DateTime DateOfBirth, ref bool Gender, ref string Address, ref string Phone, ref string Email,
           ref int NationalityCountryID, ref string ImagePath)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM People WHERE NationalNo = @NationalNo";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@NationalNo", SqlDbType.NVarChar, 20).Value = NationalNo;
                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                PersonID = (int)Reader["PersonID"];
                                FirstName = Reader["FirstName"].ToString();
                                SecondName = Reader["SecondName"].ToString();
                                ThirdName = Reader["ThirdName"] == DBNull.Value ? "" : Reader["ThirdName"].ToString();  

                                LastName = Reader["LastName"].ToString();
                                DateOfBirth = (DateTime)Reader["DateOfBirth"];
                                Gender = (bool)Reader["Gender"];
                                Address = Reader["Address"].ToString();
                                Phone = Reader["Phone"].ToString();
                                Email = Reader["Email"] == DBNull.Value ? "" : Reader["Email"].ToString();
                                NationalityCountryID = (int)Reader["NationalityCountryID"];
                                ImagePath = Reader["ImagePath"] == DBNull.Value ? "" : Reader["ImagePath"].ToString();
                            }
                        }
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }
        public static bool DeletePerson(int PersonID)
        {
            try
            {
                int RowsAffected = 0;
                using (SqlConnection Connection = new  SqlConnection (ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "DELETE FROM People WHERE PersonID = @PersonID";

                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand (Query, Connection))
                    {
                        Command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                        RowsAffected = Command.ExecuteNonQuery();
                        return (RowsAffected > 0);
                    }
                }
            }
            catch
            {
                return false;
            }

        }


        public static bool Find(int PersonID , ref string NationalNo, ref string FirstName, 
            ref string SecondName, ref string ThirdName,  ref string LastName,
            ref DateTime DateOfBirth, ref bool Gender,   ref string Address,
            ref  string Phone, ref string Email,
            ref int NationalityCountryID, ref string ImagePath)

        {

            bool IsFound = false;

            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM People WHERE PersonID = @PersonID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;

                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;

                                NationalNo              = Reader["NationalNo"].ToString();
                                FirstName               = Reader["FirstName"].ToString();
                                SecondName              = Reader["SecondName"].ToString();
                                ThirdName               = Reader["ThirdName"]  == DBNull.Value ? "" : Reader["ThirdName"].ToString(); 
                                LastName                = Reader["LastName"].ToString();
                                DateOfBirth             = (DateTime)Reader["DateOfBirth"];
                                Gender                  = (bool)Reader["Gender"];
                                Address                 = Reader["Address"].ToString();
                                Phone                   = Reader["Phone"].ToString();
                                Email                   = Reader["Email"] == DBNull.Value ? "" : Reader["Email"].ToString();
                                NationalityCountryID    = (int)Reader["NationalityCountryID"];
                                ImagePath               = Reader["ImagePath"] == DBNull.Value ? "" : Reader["ImagePath"].ToString();

                                return IsFound;
                            }
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
    }
}
