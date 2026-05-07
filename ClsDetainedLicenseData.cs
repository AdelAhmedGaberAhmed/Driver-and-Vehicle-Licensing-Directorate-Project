using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsDetainedLicenseData
    {
            public static DataTable GetAllDetainedLicenses()
            {
                DataTable dt = new DataTable();
                try
                {
                    using (SqlConnection conn = new SqlConnection(
                        ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"SELECT  DL.DetainID,
                                             DL.LicenseID,
                                             DL.DetainDate,
                                             DL.FineFees,
                                             DL.IsReleased,
                                             DL.ReleaseDate,
                                             DL.ReleasedByUserID,
                                             DL.ReleaseApplicationID,
                                             DL.CreatedByUserID,
                                             P.NationalNo,
                                             P.FirstName + ' ' +
                                             P.SecondName + ' ' +
                                             P.ThirdName + ' ' +
                                             P.LastName              AS FullName,
                                             LC.ClassName            AS LicenseClass,
                                             U1.UserName             AS DetainedByUser,
                                             U2.UserName             AS ReleasedByUser
                                    FROM     DetainedLicenses DL
                                    INNER JOIN Licenses L
                                            ON DL.LicenseID = L.LicenseID
                                    INNER JOIN Drivers D
                                            ON L.DriverID = D.DriverID
                                    INNER JOIN People P
                                            ON D.PersonID = P.PersonID
                                    INNER JOIN LicenseClasses LC
                                            ON L.LicenseClass = LC.LicenseClassID
                                    INNER JOIN Users U1
                                            ON DL.CreatedByUserID = U1.UserID
                                    LEFT JOIN Users U2
                                            ON DL.ReleasedByUserID = U2.UserID
                                    ORDER BY DL.DetainDate DESC";
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
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

            public static bool Find(int DetainID, ref int LicenseID,
                ref DateTime DetainDate, ref decimal FineFees,
                ref int CreatedByUserID, ref bool IsReleased,
                ref DateTime? ReleaseDate, ref int? ReleasedByUserID,
                ref int? ReleaseApplicationID)
            {
                bool IsFound = false;
                try
                {
                    using (SqlConnection conn = new SqlConnection(
                        ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"SELECT * FROM DetainedLicenses
                                     WHERE DetainID = @DetainID";
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.Add("@DetainID",SqlDbType.Int).Value = DetainID;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    IsFound = true;
                                    LicenseID = (int)reader["LicenseID"];
                                    DetainDate = (DateTime)reader["DetainDate"];
                                    FineFees = (decimal)reader["FineFees"];
                                    CreatedByUserID = (int)reader["CreatedByUserID"];
                                    IsReleased = (bool)reader["IsReleased"];

                                    ReleaseDate = reader["ReleaseDate"] == DBNull.Value  ? (DateTime?)null: (DateTime)reader["ReleaseDate"];

                                    ReleasedByUserID = reader["ReleasedByUserID"] == DBNull.Value ? (int?)null  : (int)reader["ReleasedByUserID"];

                                    ReleaseApplicationID = reader["ReleaseApplicationID"] == DBNull.Value? (int?)null  : (int)reader["ReleaseApplicationID"];
                                }
                            }
                        }
                    }
                }
                catch { return IsFound; }
                return IsFound;
            }

            public static bool FindByLicenseID(int LicenseID, ref int DetainID,
                ref DateTime DetainDate, ref decimal FineFees,
                ref int CreatedByUserID, ref bool IsReleased,
                ref DateTime? ReleaseDate, ref int? ReleasedByUserID,
                ref int? ReleaseApplicationID)
            {
                bool IsFound = false;
                try
                {
                    using (SqlConnection conn = new SqlConnection(
                        ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"SELECT * FROM DetainedLicenses
                                     WHERE LicenseID  = @LicenseID
                                     AND   IsReleased = 0";
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.Add("@LicenseID", SqlDbType.Int).Value = LicenseID;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    IsFound = true;
                                    DetainID = (int)reader["DetainID"];
                                    DetainDate = (DateTime)reader["DetainDate"];
                                    FineFees = (decimal)reader["FineFees"];
                                    CreatedByUserID = (int)reader["CreatedByUserID"];
                                    IsReleased = (bool)reader["IsReleased"];

                                    ReleaseDate = reader["ReleaseDate"] == DBNull.Value ? (DateTime?)null  : (DateTime)reader["ReleaseDate"];

                                    ReleasedByUserID = reader["ReleasedByUserID"] == DBNull.Value ? (int?)null : (int)reader["ReleasedByUserID"];

                                    ReleaseApplicationID = reader["ReleaseApplicationID"] == DBNull.Value ? (int?)null: (int)reader["ReleaseApplicationID"];
                                }
                            }
                        }
                    }
                }
                catch { return IsFound; }
                return IsFound;
            }

            public static int AddNew(int LicenseID, DateTime DetainDate, decimal FineFees, int CreatedByUserID)
            {
                int DetainID = -1;
                try
                {
                    using (SqlConnection conn = new SqlConnection(
                        ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"INSERT INTO DetainedLicenses
                                    (LicenseID, DetainDate, FineFees,
                                     CreatedByUserID, IsReleased)
                                     VALUES
                                    (@LicenseID, @DetainDate, @FineFees,
                                     @CreatedByUserID, 0)
                                     SELECT SCOPE_IDENTITY();";
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.Add("@LicenseID",    SqlDbType.Int).Value = LicenseID;
                            cmd.Parameters.Add("@DetainDate",  SqlDbType.DateTime).Value = DetainDate;
                            cmd.Parameters.Add("@FineFees",SqlDbType.Decimal).Value = FineFees;
                            cmd.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;

                            object Result = cmd.ExecuteScalar();
                            if (Result != null && Result != DBNull.Value )
                                DetainID = Convert.ToInt32(Result);
                        }
                    }
                }
                catch { return DetainID; }
                return DetainID;
            }

            public static bool ReleaseLicense(int DetainID, DateTime ReleaseDate,int ReleasedByUserID, int ReleaseApplicationID)
            {
                int RowsAffected = 0;
                try
                {
                    using (SqlConnection conn = new SqlConnection(
                        ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"UPDATE DetainedLicenses
                                     SET    IsReleased           = 1,
                                            ReleaseDate          = @ReleaseDate,
                                            ReleasedByUserID     = @ReleasedByUserID,
                                            ReleaseApplicationID = @ReleaseApplicationID
                                     WHERE  DetainID             = @DetainID
                                     AND    IsReleased           = 0";
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.Add("@DetainID",  SqlDbType.Int).Value = DetainID;
                            cmd.Parameters.Add("@ReleaseDate",SqlDbType.DateTime).Value = ReleaseDate;
                            cmd.Parameters.Add("@ReleasedByUserID", SqlDbType.Int).Value = ReleasedByUserID;
                            cmd.Parameters.Add("@ReleaseApplicationID", SqlDbType.Int).Value = ReleaseApplicationID;

                            RowsAffected = cmd.ExecuteNonQuery();
                            return (RowsAffected > 0);
                        }
                    }
                }
                catch { return false; }
            }

            public static bool IsLicenseDetained(int LicenseID)
            {
                bool IsFound = false;
                try
                {
                    using (SqlConnection conn = new SqlConnection(
                        ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"SELECT   1
                                     FROM   DetainedLicenses
                                     WHERE  LicenseID  = @LicenseID
                                     AND    IsReleased = 0";
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.Add("@LicenseID",
                                SqlDbType.Int).Value = LicenseID;

                            object Result = cmd.ExecuteScalar();
                            IsFound = (Result != null);
                        }
                    }
                }
                catch { return IsFound; }
                return IsFound;
            }

            public static bool Delete(int DetainID)
            {
                int RowsAffected = 0;
                try
                {
                    using (SqlConnection conn = new SqlConnection( ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"DELETE FROM DetainedLicenses
                                     WHERE DetainID = @DetainID";
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(Query, conn))
                        {
                            cmd.Parameters.Add("@DetainID", SqlDbType.Int).Value = DetainID;
                            RowsAffected = cmd.ExecuteNonQuery();
                            return (RowsAffected > 0);
                        }
                    }
                }
                catch { return false; }
            }
        }
    }

