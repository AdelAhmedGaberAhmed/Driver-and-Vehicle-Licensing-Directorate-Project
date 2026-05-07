using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsApplicationTypesData
    {
        
        public static DataTable GetAllApplicationTypes()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM ApplicationTypes ORDER BY ApplicationTypeID";
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

        public static bool UpdateApplicationType(int ApplicationTypeID, string Title, decimal Fees)
        {
            int RowsAffected = 0;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = @"UPDATE ApplicationTypes
                             SET    ApplicationTypeTitle = @Title,
                                    ApplicationFees      = @Fees
                             WHERE  ApplicationTypeID    = @ApplicationTypeID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@ApplicationTypeID", SqlDbType.Int).Value = ApplicationTypeID;
                        Command.Parameters.Add("@Title", SqlDbType.NVarChar, 150).Value = Title;
                        Command.Parameters.Add("@Fees", SqlDbType.Decimal).Value = Fees;

                        RowsAffected = Command.ExecuteNonQuery();
                        return (RowsAffected > 0);
                    }
                }
            }
            catch { return false; }
        }


        public static bool Find(int ApplicationTypeID, ref string Title, ref decimal Fees)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM ApplicationTypes WHERE ApplicationTypeID = @ApplicationTypeID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@ApplicationTypeID", SqlDbType.Int).Value = ApplicationTypeID;
                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                Title = Reader["ApplicationTypeTitle"].ToString();
                                Fees = (decimal)Reader["ApplicationFees"];
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




