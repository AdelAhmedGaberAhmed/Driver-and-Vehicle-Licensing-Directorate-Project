using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsLicenseData
    {
        public static DataTable GetAllLicense()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT  
                                            P.FirstName + ' '  +
                                            P.SecondName + ' '+ 
                                            P.ThirdName + ' '+
                                            P.LastName AS [Full Driver Name], 
                                            L.LicenseID,
                                            L.DriverID,
                                            D.PersonID,
                                            P.NationalNo,
                                            L.PaidFees,
                                            L.IsActive,
                                            L.Notes,
                                        
                                            LC.ClassName, 
                                            L.IssueDate, 
                                            L.ExpirationDate, 
                                        
                                            CASE 
                                                WHEN L.IssueReason = 1 THEN 'New'
                                                WHEN L.IssueReason = 2 THEN 'Renew'
                                                WHEN L.IssueReason = 3 THEN 'Replacement - Lost'
                                                WHEN L.IssueReason = 4 THEN 'Replacement - Damaged'
                                                ELSE 'Unknown'
                                            END AS IssueReason
                                        
                                        FROM dbo.Licenses AS L
                                        
                                        INNER JOIN dbo.Drivers AS D 
                                            ON L.DriverID = D.DriverID 
                                        
                                        INNER JOIN dbo.LicenseClasses AS LC
                                            ON L.LicenseClass = LC.LicenseClassID
                                        
                                        INNER JOIN dbo.People AS P
                                            ON D.PersonID = P.PersonID
                                          ORDER BY L.IssueDate DESC";
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

        public static int AddNewLicense(int AppID, int DriverID, int LicenseClass, DateTime IssueDate,
            DateTime ExpirationDate, string Notes, decimal PaidFees, bool IsActive, int IssueReason, int CreatedByUserID)
        {
            int LicenseID = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"
                                    INSERT INTO Licenses
                                    (
                                        ApplicationID,
                                        DriverID,
                                        LicenseClass,
                                        IssueDate,
                                        ExpirationDate,
                                        Notes,
                                        PaidFees,
                                        IsActive,
                                        IssueReason,
                                        CreatedByUserID
                                    )
                                    VALUES
                                    (
                                        @AppID,
                                        @DriverID,
                                        @LicenseClass,
                                        @IssueDate,
                                        @ExpirationDate,
                                        @Notes,
                                        @PaidFees,
                                        @IsActive,
                                        @IssueReason,
                                        @CreatedByUserID
                                    );

                                    SELECT SCOPE_IDENTITY();";
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(Query, con))
                    {
                        cmd.Parameters.Add("@AppID", SqlDbType.Int).Value = AppID;
                        cmd.Parameters.Add("@DriverID", SqlDbType.Int).Value = DriverID;
                        cmd.Parameters.Add("@LicenseClass", SqlDbType.Int).Value = LicenseClass;
                        cmd.Parameters.Add("@IssueDate", SqlDbType.DateTime).Value = IssueDate;
                        cmd.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = ExpirationDate;
                        cmd.Parameters.Add("@Notes", SqlDbType.NVarChar, 1000).Value = string.IsNullOrWhiteSpace(Notes) ? (object)DBNull.Value : Notes;
                        cmd.Parameters.Add("@PaidFees", SqlDbType.Decimal).Value = PaidFees;
                        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = IsActive;
                        cmd.Parameters.Add("@IssueReason", SqlDbType.Int).Value = IssueReason;
                        cmd.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;


                        object Result = cmd.ExecuteScalar();

                        if (Result != null && Result != DBNull.Value && int.TryParse(Result.ToString(), out int InsertID))
                        {
                            LicenseID = InsertID;
                            return LicenseID;
                        }

                    }
                }

            }
            catch { return LicenseID; }
            return LicenseID;
        }


        public static bool UpdateLicense(int LicenseID, int LicenseClass, DateTime IssueDate, DateTime ExpirationDate, string Notes,
            bool IsActive, int IssueReason, int CreatedByUserID)
        {
            int RowAffected = 0;

            try
            {

                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"UPDATE Licenses 
                                    SET     LicenseClass = @LicenseClass  , 
                                            IssueDate = @IssueDate , 
                                            ExpirationDate  = @ExpirationDate, 

                                            Notes = @Notes,
                                            IsActive = @IsActive ,
                                            IssueReason = @IssueReason,
                                            CreatedByUserID = @CreatedByUserID

                                         WHERE LicenseID = @LicenseID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {

                        Command.Parameters.Add("@LicenseID", SqlDbType.Int).Value = LicenseID;
                        Command.Parameters.Add("@LicenseClass", SqlDbType.Int).Value = LicenseClass;
                        Command.Parameters.Add("@Notes", SqlDbType.NVarChar, 1000).Value = Notes ?? (object)DBNull.Value;
                        Command.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = ExpirationDate;
                        Command.Parameters.Add("@IssueDate", SqlDbType.DateTime).Value = IssueDate;
                        Command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = IsActive;

                        Command.Parameters.Add("@IssueReason", SqlDbType.Int).Value = IssueReason;

                        Command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;


                        RowAffected = Command.ExecuteNonQuery();
                        return (RowAffected > 0);


                    }
                }
            }
            catch { return false; }
        }


        public static bool HasActiveLicense(int DriverID, int LicenseClassID)
        {

            bool IsFound = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open();
                    string Query = @"SELECT 1
                                FROM Licenses
                                WHERE DriverID = @DriverID
                                AND LicenseClass = @LicenseClassID
                                AND IsActive = 1
                                AND ExpirationDate > GETDATE()";
                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.Add("@DriverID", SqlDbType.Int).Value = DriverID;
                        cmd.Parameters.Add("@LicenseClassID", SqlDbType.Int).Value = LicenseClassID;

                        object result = cmd.ExecuteScalar();

                        IsFound = (result != null);
                        return IsFound;


                    }
                }

            }
            catch { return IsFound; }
        }

        // Should Not Delete the License Ever 
        /* public static bool Delete(int LicenseID)
         {
             int RowsAffected = 0;

             try
             {
                 using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                 {
                     string Query = @"DELETE FROM Licenses
                              WHERE LicenseID = @LicenseID";

                     conn.Open();

                     using (SqlCommand cmd = new SqlCommand(Query, conn))
                     {
                         cmd.Parameters.Add("@LicenseID", SqlDbType.Int).Value = LicenseID;

                         RowsAffected = cmd.ExecuteNonQuery();
                         return (RowsAffected > 0);
                     }
                 }
             }
             catch
             {
                 return false;
             }
         }*/
        public static bool Find(int LicenseID, ref int DriverID, ref int LicenseClassID, ref int ApplicationID,
                ref DateTime IssueDate, ref DateTime ExpirationDate, ref string Notes,
                ref decimal PaidFees, ref bool IsActive, ref int IssueReason, ref int CreatedByUserID)
        {
            bool IsFound = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"SELECT *
                             FROM Licenses
                             WHERE LicenseID = @LicenseID";

                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.Add("@LicenseID", SqlDbType.Int).Value = LicenseID;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                IsFound = true;

                                DriverID = (int)reader["DriverID"];
                                LicenseClassID = (int)reader["LicenseClass"];
                                ApplicationID = (int)reader["ApplicationID"];
                                IssueDate = (DateTime)reader["IssueDate"];
                                ExpirationDate = (DateTime)reader["ExpirationDate"];

                                Notes = reader["Notes"] == DBNull.Value ? "" : reader["Notes"].ToString();

                                PaidFees = reader["PaidFees"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PaidFees"]);

                                IsActive = (bool)reader["IsActive"];

                                IssueReason = (int)reader["IssueReason"];

                                CreatedByUserID = (int)reader["CreatedByUserID"];
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return IsFound;
        }


        //list for one driver
        public static DataTable GetDriverLicenses(int DriverID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open();
                    string Query = @"select 
                                    l.LicenseID,
                                    
                                    p.FirstName +' ' +
                                    p.SecondName + ' '+
                                    p.ThirdName +' '+
                                    p.LastName 
                                     as [Full Driver Name] , 
                                     l.IssueDate ,
                                     l.ExpirationDate ,
                                     l.IssueReason ,
                                     lc.ClassName , 
                                     l.Notes , 
                                     l.PaidFees , 
                                     l.IsActive  ,
                                     u.UserName as [Created By User]
                                    from dbo.Licenses as l
                                    
                                    inner join dbo.LicenseClasses as lc
                                    on l.LicenseClass = lc.LicenseClassID
                                    inner join dbo.Drivers as d
                                    on l.DriverID = d.DriverID
                                    inner join dbo.Users u 
                                    on l.CreatedByUserID = u.UserID 
                                    inner join People as p
                                    on d.PersonID = p.PersonID
                                    where d.DriverID = @DriverID ";


                    using (SqlCommand cmd = new SqlCommand(Query, conn))
                    {
                        cmd.Parameters.Add("@DriverID", SqlDbType.Int).Value = DriverID;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                dt.Load(reader);
                                return dt;
                            }
                        }
                    }
                }

            }
            catch { return dt; }
            return dt;
        }

        public static bool IsLicenseExist(int LicenseID)
        {
            bool isFound = false;
            using (SqlConnection connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
            {
                string query = "SELECT Found=1 FROM Licenses WHERE LicenseID = @LicenseID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LicenseID", LicenseID);
                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null) isFound = true;
                    }
                    catch { isFound = false; }
                }
            }
            return isFound;
        }
        public static bool FindByApplicationID(
                                   int applicationID,
                                   ref int licenseID,
                                   ref int driverID,
                                   ref int LicenseClass,
                                   ref DateTime issueDate,
                                   ref DateTime expirationDate,
                                   ref string notes,
                                   ref decimal paidFees,
                                   ref bool isActive,
                                   ref int issueReason,
                                   ref int createdByUserID)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open();

                    string query = @"SELECT 
                                LicenseID,
                                DriverID,
                                LicenseClass,
                                IssueDate,
                                ExpirationDate,
                                Notes,
                                PaidFees,
                                IsActive,
                                IssueReason,
                                CreatedByUserID
                             FROM Licenses
                             WHERE ApplicationID = @ApplicationID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@ApplicationID", SqlDbType.Int).Value = applicationID;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                isFound = true;

                                licenseID = (int)reader["LicenseID"];
                                driverID = (int)reader["DriverID"];
                                LicenseClass = (int)reader["LicenseClass"];
                                issueDate = (DateTime)reader["IssueDate"];
                                expirationDate = (DateTime)reader["ExpirationDate"];
                                notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : "";
                                paidFees = (decimal)reader["PaidFees"];
                                isActive = (bool)reader["IsActive"];
                                issueReason = (int)reader["IssueReason"];
                                createdByUserID = (int)reader["CreatedByUserID"];
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return isFound;
        }





        public static bool DeleteLicense(int LicenseID)
        {
            int rowsAffected = 0;
            try
            {
              
                using (SqlConnection connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    connection.Open();

                    string query = "DELETE Licenses WHERE LicenseID = @LicenseID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {

                        command.Parameters.Add("@LicenseID", SqlDbType.Int).Value = LicenseID;

                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Log your error here if you have a logging system
                return false;
            }

            return (rowsAffected > 0);
        }

    }
}
