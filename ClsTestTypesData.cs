using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsTestTypesData
    {
        public static  DataTable GetAllTypes()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection  conn = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM TestTypes";
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(Query , conn))
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

        
        public static bool Update(int ID, string title, string Description, decimal fees)
        {
            int  RowsAffected = 0;
            try
            {
              
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    Connection.Open();
                    string Query = @"UPDATE TestTypes SET  TestTypeTitle = @title ,  TestTypeDescription =  @Description , TestTypeFees = @fees WHERE TestTypeID = @ID ";

                    using (SqlCommand cmd = new SqlCommand(Query , Connection))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                        cmd.Parameters.Add("@title", SqlDbType.NVarChar, 200).Value = title;
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = Description;
                        cmd.Parameters.Add("@fees", SqlDbType.Decimal).Value = fees;


                        RowsAffected = cmd.ExecuteNonQuery();
                        return (RowsAffected > 0);
                        
                    }

                }

            }
            catch { return (RowsAffected > 0); }
        }

        public static bool Find(int ID, ref string Title,  ref string Description, ref decimal Fees)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM TestTypes WHERE TestTypeID = @ID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                Title = Reader["TestTypeTitle"].ToString();
                                Description = Reader["TestTypeDescription"] == DBNull.Value ? "" :  Reader["TestTypeDescription"].ToString();
                                Fees = (decimal)Reader["TestTypeFees"];
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
