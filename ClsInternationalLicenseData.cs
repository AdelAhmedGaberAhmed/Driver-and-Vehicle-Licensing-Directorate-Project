using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Policy;

namespace DVLD.DataAccess
{
    public class ClsInternationalLicenseData
    {
        public static DataTable GetAllInternationalLicenses()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open();
                    string query = @" select
                                        i.InternationalLicenseID as [Int.Licese ID] , 
                                        a.ApplicationID , 
                                        d.DriverID , 
                                        l.LicenseID as [L.License ID], 
                                        i.IssueDate , 
                                        i.ExpirationDate , 
                                        i.IsActive , 
                                        u.UserName as [Created By User]

                                        from dbo.InternationalLicenses as i
                                        inner join dbo.Applications as a
                                        on i.ApplicationID = a.ApplicationID

                                        inner join dbo.ApplicationTypes as Ate
                                        on a.ApplicationTypeID = ate.ApplicationTypeID

                                        inner join dbo.Licenses as l
                                        on i.IssuedUsingLocalLicenseID = l.LicenseID

                                        inner join dbo.Drivers as d
                                        on i.DriverID = d.DriverID

                                        inner join dbo.Users as u
                                        on i.CreatedByUserID = u.UserID ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
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

        public static DataTable GetDriverIntlLicenses(int DriverID)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open();
                    string query = @" select
                                        i.InternationalLicenseID as [Int.Licese ID] , 
                                        a.ApplicationID , 
                                        l.LicenseID as [L.License ID], 
                                        i.IssueDate , 
                                        i.ExpirationDate , 
                                        i.IsActive , 
                                        u.UserName as [Created By User]

                                        from dbo.InternationalLicenses as i
                                        inner join dbo.Applications as a
                                        on i.ApplicationID = a.ApplicationID

                                        inner join dbo.ApplicationTypes as ate
                                        on a.ApplicationTypeID = ate.ApplicationTypeID

                                        inner join dbo.Licenses as l
                                        on i.IssuedUsingLocalLicenseID = l.LicenseID

                                        inner join dbo.Drivers as d
                                        on i.DriverID = d.DriverID

                                        inner join dbo.Users as u
                                        on i.CreatedByUserID = u.UserID 
                                         where i.DriverID = @DriverID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@DriverID", SqlDbType.Int).Value = DriverID;
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

        public static bool Find(int InternationalLicenseID  ,ref int DriverID , ref int ApplicationID , ref int CreatedByUserID , ref int IssuedUsingLocalLicenseID,
           ref DateTime IssueDate , ref DateTime ExpirationDate , ref bool IsActive )
        {
            bool IsFound = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open();
                    string query = @"
                                SELECT
                                i.InternationalLicenseID as [Int.Licese ID] , 
                                a.ApplicationID , 
                                d.DriverID , 
                                l.LicenseID as [L.License ID], 
                                i.IssueDate , 
                                i.ExpirationDate , 
                                i.IsActive , 
                                i.CreatedByUserID,
                                
                                u.UserName as [Created By User]
                                
                                from dbo.InternationalLicenses as i
                                inner join dbo.Applications as a
                                on i.ApplicationID = a.ApplicationID
                                
                                inner join dbo.ApplicationTypes as ate
                                on a.ApplicationTypeID = ate.ApplicationTypeID
                                
                                inner join dbo.Licenses as l
                                on i.IssuedUsingLocalLicenseID = l.LicenseID
                                
                                inner join dbo.Drivers as d
                                on i.DriverID = d.DriverID
                                
                                inner join dbo.Users as u
                                on i.CreatedByUserID = u.UserID
                                
                                where i.InternationalLicenseID = @InternationalLicenseID";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.Add("@InternationalLicenseID" , SqlDbType.Int).Value= InternationalLicenseID;
                       

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                IsFound = true;
                                DriverID = reader["DriverID"] != DBNull.Value ? (int)reader["DriverID"] : -1;
                                ApplicationID = (int)reader["ApplicationID"];
                                IssuedUsingLocalLicenseID = (int)reader["IssuedUsingLocalLicenseID"];
                                CreatedByUserID = (int)reader["CreatedByUserID"];
                                IssueDate = (DateTime)reader["IssueDate"];
                                ExpirationDate = (DateTime)reader["ExpirationDate"];
                                IsActive = (bool)reader["IsActive"];
                                return IsFound;
                            }
                        }
                    }
                }

            }
            catch { return IsFound; }
            return IsFound;
        }

        public static bool HasActiveIntlLicense(int DriverID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT HasActiveIntLicense = 1 FROM  InternationalLicenses as i
                                WHERE  i.DriverID = @DriverID
                                 AND i.IsActive = 1
                                 AND i.ExpirationDate > GETDATE()";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@DriverID", SqlDbType.Int).Value = DriverID;
                       
                        object reasult = cmd.ExecuteScalar();
                        if (reasult != null) return true;
                        
                    }
                }

            }
            catch { return false; }
            return false;
    

        }


        public static DataTable GetAppInfoByLocalLicenseID(int LocalLicenseID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open();
                    string query = @"select 
                                    p.ApplicationID ,
                                    p.ApplicationDate , 
                                   
                                    p.PaidFees as[Fees], 
                                    i.IssueDate,
                                    i.ExpirationDate,
                                    u.UserName as [Created By User]
                                    
                                    from InternationalLicenses as i
                                    inner join Applications as p
                                    on i.ApplicationID = p.ApplicationID
                                    
                                    inner join ApplicationTypes as ate
                                    on p.ApplicationTypeID = ate.ApplicationTypeID
                                    
                                    inner join dbo.Users as u
                                    on i.CreatedByUserID = u.UserID
                                    
                                    where i.IssuedUsingLocalLicenseID = @LocalLicenseID";

                    using (SqlCommand cmd = new SqlCommand(query , conn))
                    {
                        cmd.Parameters.Add("@LocalLicenseID", SqlDbType.Int).Value = LocalLicenseID;

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
            catch { return  dt; }
            return dt;

        }


        public static int AddNew(int ApplicationID , int DriverID , int CreatedByUserID, int IssuedUsingLocalLicenseID,
            DateTime IssueDate,  DateTime ExpirationDate,  bool IsActive)
        {

            int INTLicenseID = -1;

            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    Connection.Open();
                    string query = @"
                        INSERT INTO InternationalLicenses (ApplicationID ,DriverID , IssuedUsingLocalLicenseID ,CreatedByUserID ,  IssueDate , ExpirationDate ,IsActive)
                         VALUES (@ApplicationID ,@DriverID , @IssuedUsingLocalLicenseID, @CreatedByUserID , @IssueDate , @ExpirationDate ,@IsActive )

                           SELECT SCOPE_IDENTITY()";


                    using (SqlCommand Command = new SqlCommand(query, Connection))
                    {
                        Command.Parameters.Add("@ApplicationID" , SqlDbType.Int).Value = ApplicationID;
                        Command.Parameters.Add("@DriverID", SqlDbType.Int).Value = DriverID;
                        Command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;
                        Command.Parameters.Add("@IssuedUsingLocalLicenseID", SqlDbType.Int).Value = IssuedUsingLocalLicenseID;

                        Command.Parameters.Add("@IssueDate", SqlDbType.DateTime).Value = IssueDate;
                        Command.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = ExpirationDate;
                        Command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = IsActive;


                        object Result = Command.ExecuteScalar();

                        if (Result != null && Result != DBNull.Value )
                        {
                            INTLicenseID = Convert.ToInt32(Result);
                            return INTLicenseID;
                        }
                    }
                }
            }
            catch
            {
                return INTLicenseID;
            }
            return INTLicenseID;
        }

        public static bool FindByApplicationID(
                  int applicationID,
                  ref int internationalLicenseID,
                  ref int driverID,
                  ref int issuedUsingLocalLicenseID,
                  ref int createdByUserID,
                  ref DateTime issueDate,
                  ref DateTime expirationDate,
                  ref bool isActive)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    conn.Open();

                    string query = @"SELECT 
                                InternationalLicenseID,
                                DriverID,
                                IssuedUsingLocalLicenseID,
                                CreatedByUserID,
                                IssueDate,
                                ExpirationDate,
                                IsActive
                             FROM InternationalLicenses
                             WHERE ApplicationID = @ApplicationID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@ApplicationID", SqlDbType.Int).Value = applicationID;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                isFound = true;

                                internationalLicenseID = (int)reader["InternationalLicenseID"];
                                driverID = (int)reader["DriverID"];
                                issuedUsingLocalLicenseID = (int)reader["IssuedUsingLocalLicenseID"];
                                createdByUserID = (int)reader["CreatedByUserID"];
                                issueDate = (DateTime)reader["IssueDate"];
                                expirationDate = (DateTime)reader["ExpirationDate"];
                                isActive = (bool)reader["IsActive"];
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
    }
}
