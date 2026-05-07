using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace DVLD.DataAccess
{
    public class ClsCountryData
    {
        public static DataTable GetAllCountries()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "select * from Countries";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
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

        public static bool Find(int CountryID, ref string CountryName)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM Countries WHERE CountryID = @CountryID";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@CountryID", SqlDbType.Int).Value = CountryID;
                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                CountryName = Reader["CountryName"].ToString();
                            }
                        }
                    }
                }
            }
            catch { return IsFound; }
            return IsFound;
        }

        public static bool Find(string CountryName, ref int CountryID)
        {
            bool IsFound = false;
            try
            {
                using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                {
                    string Query = "SELECT * FROM Countries WHERE CountryName = @CountryName";
                    Connection.Open();
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add("@CountryName", SqlDbType.NVarChar, 100).Value = CountryName;
                        using (SqlDataReader Reader = Command.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                IsFound = true;
                                CountryID = (int)Reader["CountryID"];
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

