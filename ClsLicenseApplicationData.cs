using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsLicenseApplicationData
    {
        // List All License Application

        public static DataTable GetAllLicenseApplication()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT  LO.LocalDrivingLicenseApplicationID AS [L.D.L AppID],
                         LO.ApplicationID,
                         LO.LicenseClassID,
                         LicenseClasses.ClassName             AS [Driving Class],
                         Applications.ApplicantPersonID,
                         People.NationalNo,
                         People.FirstName + ' ' +
                         People.SecondName + ' ' +
                         People.ThirdName + ' ' +
                         People.LastName                      AS FullName,
                         Applications.ApplicationDate,
                         Applications.PaidFees,
                         COUNT(CASE WHEN Tests.TestResult = 1 
                               THEN 1 END)                    AS PassedTests,
                         CASE
                             WHEN Applications.ApplicationStatus = 1 THEN 'New'
                             WHEN Applications.ApplicationStatus = 2 THEN 'Cancelled'
                             WHEN Applications.ApplicationStatus = 3 THEN 'Completed'
                         END                                  AS Status
                FROM     LocalDrivingLicenseApplications LO
                INNER JOIN Applications
                        ON LO.ApplicationID = Applications.ApplicationID
                INNER JOIN LicenseClasses
                        ON LO.LicenseClassID = LicenseClasses.LicenseClassID
                INNER JOIN People
                        ON People.PersonID = Applications.ApplicantPersonID
                LEFT JOIN TestAppointments
                        ON LO.LocalDrivingLicenseApplicationID = TestAppointments.LocalDrivingLicenseApplicationID
                LEFT JOIN Tests
                        ON TestAppointments.TestAppointmentID = Tests.TestAppointmentID
                GROUP BY
                        LO.LocalDrivingLicenseApplicationID,
                        LO.ApplicationID,
                        LO.LicenseClassID,
                        LicenseClasses.ClassName,
                        Applications.ApplicantPersonID,
                        People.FirstName,
                        People.SecondName,
                        People.ThirdName,
                        People.LastName,
                        People.NationalNo,
                        Applications.ApplicationDate,
                        Applications.PaidFees,
                        Applications.ApplicationStatus
                ORDER BY Applications.ApplicationDate DESC";


                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand (Query, conn))
                    {
                        using(SqlDataReader reader = cmd.ExecuteReader())
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


        public static bool FindByLicenseApplicationID(int LicenseApplicationID, ref int ApplicationID, 
            ref int LicenseClassID,ref int PersonID,ref int MinimumAllowedAge)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    //-- ✅ Only what you need
                    string Query = @"
                                    SELECT  LO.LocalDrivingLicenseApplicationID,
                                            LO.ApplicationID,
                                            LO.LicenseClassID,
                                            Applications.ApplicantPersonID AS PersonID,
                                            People.DateOfBirth,
                                            LicenseClasses.MinimumAllowedAge
                                    FROM    LocalDrivingLicenseApplications LO
                                    INNER JOIN Applications
                                            ON LO.ApplicationID = Applications.ApplicationID
                                    INNER JOIN LicenseClasses
                                            ON LO.LicenseClassID = LicenseClasses.LicenseClassID
                                    INNER JOIN People
                                            ON Applications.ApplicantPersonID = People.PersonID
                                    WHERE   LO.LocalDrivingLicenseApplicationID = @LicenseApplicationID";
                    
                    conn.Open();

                    using (SqlCommand command = new SqlCommand(Query, conn))
                    {
                        command.Parameters.Add("@LicenseApplicationID", SqlDbType.Int).Value = LicenseApplicationID;
                      

                        using (SqlDataReader Reader = command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                ApplicationID = (int)Reader["ApplicationID"];
                                LicenseClassID = (int)Reader["LicenseClassID"];
                                PersonID = (int)Reader["PersonID"];
                                MinimumAllowedAge = (int)Reader["MinimumAllowedAge"];

                            }
                        }
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }

        public static bool FindByApplicationID(int ApplicationID, ref int LicenseApplicationID,
            ref int LicenseClassID, ref int PersonID,  ref int MinimumAllowedAge)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    // ✅ Better — get everything in one shot
                    string Query = @"
                                    SELECT  LO.LocalDrivingLicenseApplicationID,
                                            LO.ApplicationID,
                                            LO.LicenseClassID,
                                            Applications.ApplicantPersonID AS PersonID,
                                            People.DateOfBirth,
                                            LicenseClasses.MinimumAllowedAge
                                    FROM    LocalDrivingLicenseApplications LO
                                    INNER JOIN Applications
                                            ON LO.ApplicationID = Applications.ApplicationID
                                    INNER JOIN LicenseClasses
                                            ON LO.LicenseClassID = LicenseClasses.LicenseClassID
                                    INNER JOIN People
                                            ON Applications.ApplicantPersonID = People.PersonID
                                    WHERE   LO.ApplicationID = @ApplicationID";
                                        conn.Open();

                    using (SqlCommand command = new SqlCommand(Query, conn))
                    {
                        command.Parameters.Add("@ApplicationID", SqlDbType.Int).Value = ApplicationID;
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                IsFound = true;
                                LicenseApplicationID = (int)reader["LocalDrivingLicenseApplicationID"];
                                LicenseClassID = (int)reader["LicenseClassID"];
                                PersonID = (int)reader["PersonID"];
                                MinimumAllowedAge = (int)reader["MinimumAllowedAge"];

                            }
                        }
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }


        // Checks if person has OPEN application for same class
        // Different from DoesPersonHaveLicenseClass
        // which checks ISSUED licenses


        public static bool DoesPersonHaveActiveLicenseApplication( int PersonID, int LicenseClassID)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT Found = 1
                             FROM   LocalDrivingLicenseApplications LO
                             INNER JOIN Applications
                                     ON LO.ApplicationID = Applications.ApplicationID
                             WHERE  Applications.ApplicantPersonID = @PersonID
                             AND    LO.LicenseClassID              = @LicenseClassID
                             AND    Applications.ApplicationStatus  = 1"; // New
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                        Command.Parameters.Add("@LicenseClassID", SqlDbType.Int).Value = LicenseClassID;

                        object Result = Command.ExecuteScalar();
                        IsFound = (Result != null);
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }
        public static bool DoesPersonHaveLicenseClass(int PersonID, int LicenseClassID)
        {
            bool HaveLicenseClass = false;

            try
            {
                using (SqlConnection conn = new SqlConnection  (ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT Found = 1
                                        FROM   Licenses
                                        INNER JOIN Drivers
                                                ON Licenses.DriverID = Drivers.DriverID
                                        WHERE  Drivers.PersonID    = @PersonID
                                        AND    Licenses.LicenseClassID = @LicenseClassID
                                        AND    Licenses.IsActive       = 1";

                    conn.Open();

                    using (SqlCommand command = new SqlCommand(Query,conn))
                    {
                        command.Parameters.Add("@PersonID" , SqlDbType.Int).Value = PersonID;
                        command.Parameters.Add("@LicenseClassID" ,SqlDbType.Int).Value = LicenseClassID;

                        object result = command.ExecuteScalar();
                        HaveLicenseClass = Convert.ToBoolean(result);
                    }
                }
            }
            catch { return HaveLicenseClass; }
            return HaveLicenseClass;

        }


        public static int AddNew(int ApplicationID, int LicenseClassID)
        {
            int LocalDrivingLicenseApplicationID = -1;
            try
            {
                using (SqlConnection Connection = new SqlConnection( ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"INSERT INTO LocalDrivingLicenseApplications
                            (ApplicationID, LicenseClassID)
                             VALUES
                            (@ApplicationID, @LicenseClassID)
                             SELECT SCOPE_IDENTITY();";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@ApplicationID" ,SqlDbType.Int).Value = ApplicationID;
                        Command.Parameters.Add("@LicenseClassID",SqlDbType.Int).Value = LicenseClassID;

                        object Result = Command.ExecuteScalar();

                        if (Result != null && Result != DBNull.Value &&int.TryParse(Result.ToString(), out int InsertedID))
                            LocalDrivingLicenseApplicationID = InsertedID;
                    }
                }
            }
            catch { return LocalDrivingLicenseApplicationID; }
            return LocalDrivingLicenseApplicationID;
        }


        public static bool Delete(int LocalDrivingLicenseApplicationID)
        {
            int RowsAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"DELETE FROM LocalDrivingLicenseApplications
                             WHERE LocalDrivingLicenseApplicationID = 
                             @LocalDrivingLicenseApplicationID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add( "@LocalDrivingLicenseApplicationID",SqlDbType.Int).Value = LocalDrivingLicenseApplicationID;

                        RowsAffected = Command.ExecuteNonQuery();
                        return (RowsAffected > 0);
                    }
                }
            }
            catch { return false; }
            
        }

    }
}
