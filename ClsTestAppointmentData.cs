using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsTestAppointmentData
    {
        public static DataTable GetAllTestAppointments()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT  
                                        TA.TestAppointmentID,
                                        TA.LocalDrivingLicenseApplicationID AS [L.D.L AppID],
                                        TA.AppointmentDate,
                                        TA.PaidFees,
                                          -- Person info
                                        People.PersonID,
                                        People.NationalNo,
                                        People.FirstName + ' ' + 
                                        People.SecondName + ' ' + 
                                        People.ThirdName + ' ' + 
                                        People.LastName AS FullName,
                                        -- Application Status
                                        CASE 
                                            WHEN APP.ApplicationStatus = 1 THEN 'New'
                                            WHEN APP.ApplicationStatus = 2 THEN 'Cancelled'
                                            WHEN APP.ApplicationStatus = 3 THEN 'Completed'
                                        END AS ApplicationStatus,
                                    
                                        -- License Class
                                        LC.ClassName AS [License Class Name]
                                    
                                    FROM TestAppointments TA
                                    
                                    INNER JOIN LocalDrivingLicenseApplications LO
                                        ON TA.LocalDrivingLicenseApplicationID = LO.LocalDrivingLicenseApplicationID
                                    
                                    INNER JOIN Applications APP
                                        ON LO.ApplicationID = APP.ApplicationID
                                    
                                    INNER JOIN People
                                        ON APP.ApplicantPersonID = People.PersonID
                                    
                                    INNER JOIN LicenseClasses LC
                                        ON LO.LicenseClassID = LC.LicenseClassID
                                    
                                    ORDER BY TA.AppointmentDate DESC;";

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

        public static DataTable GetAppointmentsForLicApp(int LocalDrivingLicenseApplicationID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(
                    ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT  TA.TestAppointmentID,
                                             TA.AppointmentDate,
                                             TA.PaidFees,
                                             TA.IsLocked,
                                             TA.RetakeTestApplicationID,
                                             TA.TestTypeID,
                                             TT.TestTypeTitle,
                                             T.TestResult,
                                             T.Notes AS TestNotes
                                    FROM     TestAppointments TA
                                    INNER JOIN TestTypes TT
                                            ON TA.TestTypeID = TT.TestTypeID
                                    LEFT JOIN Tests T
                                            ON TA.TestAppointmentID = T.TestAppointmentID
                                    WHERE    TA.LocalDrivingLicenseApplicationID
                                                 = @LocalDrivingLicenseApplicationID
                                    ORDER BY TA.AppointmentDate ASC";

                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.Add(
                            "@LocalDrivingLicenseApplicationID",
                            SqlDbType.Int).Value = LocalDrivingLicenseApplicationID;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                                dt.Load(reader);
                        }
                    }
                }
            }
            catch { return dt; }
            return dt;
        }
        public static bool Find(int TestAppointmentID, ref int TestTypeID, ref int LicenseAppID, ref DateTime AppointmentDate,
            ref decimal PaidFees, ref int CreatedByUserID, ref bool IsLocked, ref int? RetakeTestAppID)
        {
            bool IsFound = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @" select * from TestAppointments  where TestAppointmentID = @TestAppointmentID";
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.Add("@TestAppointmentID", SqlDbType.Int).Value = TestAppointmentID;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                IsFound = true;
                                TestTypeID = (int)reader["TestTypeID"];
                                LicenseAppID = (int)reader["LocalDrivingLicenseApplicationID"];
                                AppointmentDate = (DateTime)reader["AppointmentDate"];
                                PaidFees = Convert.ToDecimal( reader["PaidFees"]);
                                CreatedByUserID = (int)reader["CreatedByUserID"];
                                IsLocked = (bool)reader["IsLocked"];
                                RetakeTestAppID = reader["RetakeTestApplicationID"] == DBNull.Value ? (int?)null : (int)reader["RetakeTestApplicationID"];

                            }
                        }
                    }
                }

            }
            catch { return IsFound; }
            return IsFound;
        }


        public static int AddNew(int LocalDrivingLicenseApplicationID, int TestTypeID, 
            DateTime AppointmentDate, decimal PaidFees, int CreatedByUserID,  int? RetakeTestApplicationID)
        {
            int TestAppointmentID = -1;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"INSERT INTO TestAppointments
                                    (LocalDrivingLicenseApplicationID,
                                     TestTypeID, AppointmentDate,
                                     PaidFees, CreatedByUserID,
                                     IsLocked, RetakeTestApplicationID)
                                     VALUES
                                    (@LocalDrivingLicenseApplicationID,
                                     @TestTypeID, @AppointmentDate,
                                     @PaidFees, @CreatedByUserID,
                                     0, @RetakeTestApplicationID)
                                     SELECT SCOPE_IDENTITY();";
                    Connection.Open();

                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add( "@LocalDrivingLicenseApplicationID", SqlDbType.Int).Value = LocalDrivingLicenseApplicationID;
                        Command.Parameters.Add("@TestTypeID", SqlDbType.Int).Value = TestTypeID;
                        Command.Parameters.Add(  "@AppointmentDate", SqlDbType.DateTime).Value = AppointmentDate;
                        Command.Parameters.Add("@PaidFees",   SqlDbType.Decimal).Value = PaidFees;
                        Command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;
                        Command.Parameters.Add("@RetakeTestApplicationID", SqlDbType.Int).Value =RetakeTestApplicationID.HasValue ? (object)RetakeTestApplicationID.Value: DBNull.Value;

                        object Result = Command.ExecuteScalar();

                        if (Result != null && Result != DBNull.Value &&  int.TryParse(Result.ToString(), out int InsertedID))
                            TestAppointmentID = InsertedID;
                    }
                }
            }
            catch { return TestAppointmentID; }
            return TestAppointmentID;
        }

        public static bool Update(int TestAppointmentID, 
            int TestTypeID, DateTime AppointmentDate,  decimal PaidFees, int CreatedByUserID, bool IsLocked, int? RetakeTestApplicationID)
        {
            int RowsAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(
                    ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"UPDATE TestAppointments
                                     SET    
                                            TestTypeID                       = @TestTypeID,
                                            AppointmentDate                  = @AppointmentDate,
                                            PaidFees                         = @PaidFees,
                                            CreatedByUserID                  = @CreatedByUserID,
                                            IsLocked                         = @IsLocked,
                                            RetakeTestApplicationID          = @RetakeTestApplicationID
                                     WHERE  TestAppointmentID                = @TestAppointmentID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@TestAppointmentID",  SqlDbType.Int).Value = TestAppointmentID;
                        Command.Parameters.Add( "@TestTypeID",    SqlDbType.Int).Value = TestTypeID;
                        Command.Parameters.Add( "@AppointmentDate",SqlDbType.DateTime).Value = AppointmentDate;
                        Command.Parameters.Add( "@PaidFees",SqlDbType.Decimal).Value = PaidFees;
                        Command.Parameters.Add( "@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;
                        Command.Parameters.Add( "@IsLocked",   SqlDbType.Bit).Value = IsLocked;
                        Command.Parameters.Add("@RetakeTestApplicationID", SqlDbType.Int).Value = RetakeTestApplicationID.HasValue  ? (object)RetakeTestApplicationID.Value  : DBNull.Value;

                        RowsAffected = Command.ExecuteNonQuery();

                        return (RowsAffected > 0);
                    }
                }
            }
            catch { return false; }
        }

        public static bool Delete(int TestAppointmentID)
        {
            int RowsAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(
                    ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"DELETE FROM TestAppointments
                                     WHERE TestAppointmentID = @TestAppointmentID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@TestAppointmentID", SqlDbType.Int).Value = TestAppointmentID;
                        RowsAffected = Command.ExecuteNonQuery();
                        return (RowsAffected > 0);
                    }
                }
            }
            catch { return false; }
        }

        public static bool HasPassedTest( int LocalDrivingLicenseApplicationID, int TestTypeID)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection( ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT Found = 1
                                     FROM   TestAppointments TA
                                     INNER JOIN Tests T
                                             ON TA.TestAppointmentID = T.TestAppointmentID
                                     WHERE  TA.LocalDrivingLicenseApplicationID = @LocalDrivingLicenseApplicationID
                                     AND    TA.TestTypeID  = @TestTypeID
                                     AND    T.TestResult   = 1";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add( "@LocalDrivingLicenseApplicationID", SqlDbType.Int).Value = LocalDrivingLicenseApplicationID;
                        Command.Parameters.Add("@TestTypeID", SqlDbType.Int).Value = TestTypeID;

                        object Result = Command.ExecuteScalar();
                        IsFound = (Result != null);
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }

        public static bool HasActiveAppointmentForTest(int LocalDrivingLicenseApplicationID, int TestTypeID)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(
                    ClsDataAccessSettings.ConnectionString))
                {
                    //-- ✅ Check for appointment with no test result
                    string Query = @"SELECT 1
                                        FROM TestAppointments TA
                                        WHERE TA.LocalDrivingLicenseApplicationID = @LocalDrivingLicenseApplicationID
                                        AND TA.TestTypeID = @TestTypeID
                                        AND TA.IsLocked = 0";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add(  "@LocalDrivingLicenseApplicationID",SqlDbType.Int).Value = LocalDrivingLicenseApplicationID;
                        Command.Parameters.Add("@TestTypeID",SqlDbType.Int).Value = TestTypeID;

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

