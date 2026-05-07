using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    
        public class ClsTestResultData
        {
            public static bool Find( int TestID , ref int TestAppointmentID,ref bool TestResult, ref string Notes, ref int CreatedByUserID)
            {
                bool IsFound = false;
                try
                {
                    using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"SELECT * FROM Tests
                                     WHERE TestID = @TestID";
                        Connection.Open();
                        using (SqlCommand Command = new SqlCommand(Query, Connection))
                        {
                            Command.Parameters.Add("@TestID", SqlDbType.Int).Value = TestID;

                            using (SqlDataReader Reader = Command.ExecuteReader())
                            {
                                if (Reader.Read())
                                {
                                    IsFound = true;
                                    TestResult = (bool)Reader["TestResult"];
                                     TestAppointmentID = (int)Reader["TestAppointmentID"];
                                    Notes = Reader["Notes"] == DBNull.Value ? ""  : Reader["Notes"].ToString();
                                    CreatedByUserID = (int)Reader["CreatedByUserID"];
                                }
                            }
                        }
                    }
                }
                catch { return IsFound; }
                return IsFound;
            }

            public static int AddNew(int TestAppointmentID,  bool TestResult, string Notes, int CreatedByUserID)
            {
                int TestID = -1;
                try
                {
                    using (SqlConnection Connection = new SqlConnection(  ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"INSERT INTO Tests
                                    (TestAppointmentID, TestResult,
                                     Notes, CreatedByUserID)
                                     VALUES
                                    (@TestAppointmentID, @TestResult,
                                     @Notes, @CreatedByUserID)
                                     SELECT SCOPE_IDENTITY();";
                        Connection.Open();
                        using (SqlCommand Command = new SqlCommand(Query, Connection))
                        {
                            Command.Parameters.Add("@TestAppointmentID", SqlDbType.Int).Value = TestAppointmentID;
                            Command.Parameters.Add("@TestResult",SqlDbType.Bit).Value = TestResult;
                            Command.Parameters.Add("@Notes",SqlDbType.NVarChar, 1000).Value = string.IsNullOrWhiteSpace(Notes) ? (object)DBNull.Value : Notes;
                            Command.Parameters.Add("@CreatedByUserID",  SqlDbType.Int).Value = CreatedByUserID;

                            object Result = Command.ExecuteScalar();
                            if (Result != null && Result != DBNull.Value &&  int.TryParse(Result.ToString(), out int InsertedID))
                            TestID = InsertedID;
                        }
                    }
                }
                catch { return TestID; }
                return TestID;
            }

            public static bool Update(int TestID,  bool TestResult, string Notes, int CreatedByUserID)
            {
                int RowsAffected = 0;
                try
                {
                    using (SqlConnection Connection = new SqlConnection(
                        ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"UPDATE Tests
                                     SET    TestResult      = @TestResult,
                                            Notes           = @Notes,
                                            CreatedByUserID = @CreatedByUserID
                                     WHERE  TestID = @TestID";
                        Connection.Open();
                        using (SqlCommand Command = new SqlCommand(Query, Connection))
                        {
                            Command.Parameters.Add("@TestID", SqlDbType.Int).Value = TestID;
                            Command.Parameters.Add("@TestResult", SqlDbType.Bit).Value = TestResult;
                            Command.Parameters.Add("@Notes",SqlDbType.NVarChar, 1000).Value = string.IsNullOrWhiteSpace(Notes)? (object)DBNull.Value : Notes;
                            Command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;

                            RowsAffected = Command.ExecuteNonQuery();
                            return (RowsAffected > 0);
                        }
                    }
                }
                catch { return false; }
            }

            public static bool Delete(int TestID)
            {
                int RowsAffected = 0;
                try
                {
                    using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"DELETE FROM Tests
                                     WHERE TestID = @TestID";
                        Connection.Open();
                        using (SqlCommand Command = new SqlCommand(Query, Connection))
                        {
                            Command.Parameters.Add("@TestID", SqlDbType.Int).Value = TestID;
                            RowsAffected = Command.ExecuteNonQuery();
                            return (RowsAffected > 0);
                        }
                    }
                }
                catch { return false; }
            }

        public static bool FindByAppointmentID(int TestAppointmentID, ref int TestID, 
            ref bool TestResult, ref string Notes, ref int CreatedByUserID)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(
                    ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT * FROM Tests
                             WHERE TestAppointmentID = @TestAppointmentID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@TestAppointmentID",
                            SqlDbType.Int).Value = TestAppointmentID;

                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                TestID = (int)Reader["TestID"];
                                TestResult = (bool)Reader["TestResult"];
                                Notes = Reader["Notes"] == DBNull.Value ? "" : Reader["Notes"].ToString();
                                CreatedByUserID = (int)Reader["CreatedByUserID"];
                            }
                        }
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }
    }
    
}
