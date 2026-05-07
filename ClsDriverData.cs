using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsDriverData
    {
        public static  DataTable GetAllDrivers()
        {
            DataTable dt = new DataTable();
            try
            {

                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"select 
                                        P.FirstName  + ' ' +
                                        p.SecondName +' ' + 
                                        P.ThirdName  + ' '+ 
                                        P.LastName AS[Full Driver Name],
                                        
                                        p.DateOfBirth ,
                                        P.Address , 
                                        P.Phone , 
                                        C.CountryName ,
                                        D.DriverID   ,   -- needed to open driver details
                                        D.PersonID    ,  -- needed to link to person
                                        P.NationalNo   , -- needed for search
                                        U.UserName AS [Created By User], 
                                        U.IsActive
                                        
                                        from 
                                        dbo.Drivers AS D
                                        inner join People AS P
                                        On D.PersonID = P.personID
                                        inner join dbo.Users AS U
                                        ON U.UserID = D.CreatedByUserID
                                        inner join Countries AS C
                                        ON P.NationalityCountryID = C.CountryID";

                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                dt.Load(reader);
                            }
                        }
                    }
                }
            }
            catch { return dt; }
            return dt;
        }

        public static bool Find(int DriverID , ref int PersonID , ref int CreatedByUserID , ref DateTime CreatedDate)
        { 
            bool IsFound = false;  
            try
            {
                using (SqlConnection con = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT * FROM Drivers WHERE DriverID = @DriverID";
                    con.Open();

                    using (SqlCommand  command = new SqlCommand(Query, con))
                    {
                        command.Parameters.Add("@DriverID" , SqlDbType.Int).Value = DriverID;
                        using (SqlDataReader reader =  command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                IsFound = true;
                                PersonID = (int)reader["PersonID"];
                                CreatedByUserID = (int)reader["CreatedByUserID"];
                                CreatedDate = (DateTime)reader["CreatedDate"];
                            }
                        }
                    }
                }

            }
            catch { return IsFound;}
            return IsFound;
        }

        public static int GetDriverIDByPersonID(int PersonID)
        {
            int DriverID = -1;

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT DriverID 
                             FROM Drivers 
                             WHERE PersonID = @PersonID";

                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;

                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            DriverID = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch
            {
                return DriverID;
            }

            return DriverID;
        }

        public static int AddNewDriver(int PersonID, int CreatedByUserID, DateTime CreatedDate)
        {
            int DriverID = -1;

            try
            {
                using (SqlConnection con = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                 {
                        string Query = @"
                       INSERT INTO Drivers
                        (PersonID, CreatedByUserID, CreatedDate)
                        VALUES
                        (@PersonID, @CreatedByUserID, @CreatedDate);

                      SELECT CAST(SCOPE_IDENTITY());";
                    con.Open();


                    using (SqlCommand cmd = new SqlCommand(Query, con))
                    {
                        cmd.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                        cmd.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;
                        cmd.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = CreatedDate;


                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            DriverID = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch 
            {
                // Optional: log ex.Message
                return -1;
            }

            return DriverID;
        }
    }
}
