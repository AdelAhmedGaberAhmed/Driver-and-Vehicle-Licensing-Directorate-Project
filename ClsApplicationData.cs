using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsApplicationData
    {
        // List All Applications Info
        public static  DataTable GetAllApplications()
        { 
            DataTable dt = new DataTable();
            

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT   Applications.ApplicationID,Applications.ApplicationDate,Applications.ApplicantPersonID, 
                            People.FirstName + ' ' + People.SecondName + ' ' +  People.ThirdName + ' ' + People.LastName AS FullName, 
                            Applications.ApplicationTypeID, 
                            ApplicationTypes.ApplicationTypeTitle,Applications.ApplicationStatus,  Applications.LastStatusDate ,
                            Applications.PaidFees,      
                            Applications.CreatedByUserID  FROM     Applications INNER JOIN People   
                            ON Applications.ApplicantPersonID = People.PersonID INNER JOIN ApplicationTypes 
                            ON Applications.ApplicationTypeID = ApplicationTypes.ApplicationTypeID ORDER BY Applications.ApplicationDate DESC ;";

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

        // List Info For One Person 

        public static DataTable GetApplicationsForPerson(int PersonID)
        {
            DataTable dt = new DataTable();
      

            try 
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open ();
                    string Query = @"SELECT   Applications.ApplicationID,Applications.ApplicationDate,
                                        Applications.ApplicantPersonID,Applications.ApplicationTypeID,
                                        ApplicationTypes.ApplicationTypeTitle,Applications.ApplicationStatus,
                                         Applications.LastStatusDate,Applications.PaidFees,
                                        Applications.CreatedByUserID
                                        FROM     Applications INNER JOIN ApplicationTypes ON Applications.ApplicationTypeID = ApplicationTypes.ApplicationTypeID 
                                        WHERE    Applications.ApplicantPersonID = @PersonID ORDER BY Applications.ApplicationDate DESC";

                    using (SqlCommand cmd = new SqlCommand(Query,conn))
                    {
                        cmd.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;

                        using (SqlDataReader reader = cmd.ExecuteReader ())
                        {

                            if( reader.HasRows)
                            {
                                dt.Load (reader);
                             
                            }
                        }
                    }
                }
            
            
            }
            catch { return dt; }
            return dt;

        }

        // Find One App info

        public static bool Find(int ApplicationID, ref int PersonID, ref DateTime ApplicationDate,
          ref int ApplicationTypeID, ref int ApplicationStatus, ref DateTime LastStatusDate,
          ref decimal PaidFees, ref int CreatedByUserID)
        {
            bool IsFound = false;

            try
            {
                using (SqlConnection conn = new SqlConnection (ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM dbo.Applications WHERE ApplicationID = @ApplicationID ";
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand (Query , conn))
                    {
                        cmd.Parameters.Add("@ApplicationID", SqlDbType.Int).Value = ApplicationID;

                        using (SqlDataReader reader = cmd.ExecuteReader ())
                        {
                            if (reader.Read())
                            {
                                IsFound = true;
                                PersonID = (int)reader["ApplicantPersonID"];
                                ApplicationDate = (DateTime)reader["ApplicationDate"];
                                ApplicationTypeID = (int)reader["ApplicationTypeID"];
                                ApplicationStatus = (int)reader["ApplicationStatus"];
                                LastStatusDate = (DateTime)reader["LastStatusDate"];
                                PaidFees = (decimal)reader["PaidFees"];
                                CreatedByUserID = (int)reader["CreatedByUserID"];
                            }
                        }

                    }
                }

            }
            catch
            { return IsFound; }
            return IsFound;
        }


        public static int AddNewApplication(int PersonID, DateTime ApplicationDate, int ApplicationTypeID,
            int ApplicationStatus,DateTime LastStatusDate , decimal PaidFees, int CreatedByUserID)
        {
            int ApplicationID = -1;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"INSERT INTO Applications 
                            (ApplicantPersonID, ApplicationDate, ApplicationTypeID, 
                             ApplicationStatus, LastStatusDate , PaidFees, CreatedByUserID)
                             VALUES
                            (@PersonID, @ApplicationDate, @ApplicationTypeID,
                             @ApplicationStatus, @LastStatusDate ,@PaidFees, @CreatedByUserID)
                             SELECT SCOPE_IDENTITY();";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                        Command.Parameters.Add("@ApplicationDate", SqlDbType.DateTime).Value = ApplicationDate;
                        Command.Parameters.Add("@ApplicationTypeID", SqlDbType.Int).Value = ApplicationTypeID;
                        Command.Parameters.Add("@ApplicationStatus", SqlDbType.Int).Value = ApplicationStatus;
                        Command.Parameters.Add("@LastStatusDate", SqlDbType.DateTime).Value = LastStatusDate;
                        Command.Parameters.Add("@PaidFees", SqlDbType.Decimal).Value = PaidFees;
                        Command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;

                        object Result = Command.ExecuteScalar();

                        if (Result != null && Result != DBNull.Value &&  int.TryParse(Result.ToString(), out int InsertedID))
                            ApplicationID = InsertedID;
                    }
                }
            }
            catch { return ApplicationID; }
            return ApplicationID;
        }

        public static bool UpdateApplication(int ApplicationID,  DateTime ApplicationDate
            , int ApplicationStatus, DateTime LastStatusDate, decimal PaidFees)
        {
            int RowsAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"UPDATE Applications
                             SET    
                                    ApplicationDate   = @ApplicationDate,
                                    
                                    ApplicationStatus = @ApplicationStatus,
                                    LastStatusDate    = @LastStatusDate ,
                                    PaidFees          = @PaidFees
                                   
                             WHERE  ApplicationID     = @ApplicationID";

                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@ApplicationID", SqlDbType.Int).Value = ApplicationID;
                        Command.Parameters.Add("@ApplicationDate", SqlDbType.DateTime).Value = ApplicationDate;
                        Command.Parameters.Add("@ApplicationStatus", SqlDbType.Int).Value = ApplicationStatus;
                        Command.Parameters.Add("@LastStatusDate", SqlDbType.DateTime).Value = LastStatusDate;
                        Command.Parameters.Add("@PaidFees", SqlDbType.Decimal).Value = PaidFees;

                        RowsAffected = Command.ExecuteNonQuery();
                        return (RowsAffected > 0);
                    }
                }
            }
            catch { return false; }
        }

        public static bool DeleteApplication(int ApplicationID)
        {
            int RowsAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "DELETE FROM Applications WHERE ApplicationID = @ApplicationID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@ApplicationID", SqlDbType.Int).Value = ApplicationID;
                        RowsAffected = Command.ExecuteNonQuery();
                        return (RowsAffected > 0);
                    }
                }
            }
            catch { return false; }
        }

       
       public static bool DoesPersonHaveActiveApplication(int PersonID, int ApplicationTypeID )
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                   string Query = @"SELECT Found = 1 
                                FROM   Applications
                                INNER JOIN LicenseApplications
                                        ON Applications.ApplicationID = LicenseApplications.ApplicationID
                                WHERE  Applications.ApplicantPersonID = @PersonID
                                AND    Applications.ApplicationTypeID = @ApplicationTypeID
                                AND    Applications.ApplicationStatus = 1"; // 1 = New
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                        Command.Parameters.Add("@ApplicationTypeID", SqlDbType.Int).Value = ApplicationTypeID;

                        object Result = Command.ExecuteScalar();
                        IsFound = (Result != null);
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }


    }
}
