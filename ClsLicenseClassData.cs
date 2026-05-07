using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.DataAccess
{
    public class ClsLicenseClassData
    {
            public static DataTable GetAllLicenseClasses()
            {
                DataTable dt = new DataTable();
                try
                {
                    using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = "SELECT * FROM LicenseClasses ORDER BY LicenseClassID";
                        Connection.Open();
                        using (SqlCommand Command = new SqlCommand(Query, Connection))
                        {
                            using (SqlDataReader Reader = Command.ExecuteReader())
                            {
                                if (Reader.HasRows)
                                    dt.Load(Reader);
                            }
                        }
                    }
                }
                catch { return dt; }
                return dt;
            }

            public static bool Find(int LicenseClassID, ref string ClassName,  ref string ClassDescription, ref int MinimumAllowedAge,
                ref int DefaultValidityLength, ref decimal ClassFees)
            {
                bool IsFound = false;
                try
                {
                    using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = "SELECT * FROM LicenseClasses WHERE LicenseClassID = @LicenseClassID";
                        Connection.Open();
                        using (SqlCommand Command = new SqlCommand(Query, Connection))
                        {
                            Command.Parameters.Add("@LicenseClassID", SqlDbType.Int).Value = LicenseClassID;
                            using (SqlDataReader Reader = Command.ExecuteReader())
                            {
                                if (Reader.Read())
                                {
                                    IsFound   = true;
                                    ClassName = Reader["ClassName"].ToString();
                                    ClassDescription = Reader["ClassDescription"] == DBNull.Value ? "" :Reader["ClassDescription"].ToString();
                                    MinimumAllowedAge = (int)Reader["MinimumAllowedAge"];
                                    DefaultValidityLength = (int)Reader["DefaultValidityLength"];
                                    ClassFees = (decimal)Reader["ClassFees"];
                                }
                            }
                        }
                    }
                }
                catch { return IsFound; }
                return IsFound;
            }

            public static bool Find(string ClassName, ref int LicenseClassID, ref string ClassDescription, ref int MinimumAllowedAge,
                ref int DefaultValidityLength, ref decimal ClassFees)
            {
                bool IsFound = false;
                try
                {
                    using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = "SELECT * FROM LicenseClasses WHERE LOWER(ClassName) = LOWER(@ClassName)";
                        Connection.Open();
                        using (SqlCommand Command = new SqlCommand(Query, Connection))
                        {
                            Command.Parameters.Add("@ClassName", SqlDbType.NVarChar, 100).Value = ClassName;
                            using (SqlDataReader Reader = Command.ExecuteReader())
                            {
                                if (Reader.Read())
                                {
                                    IsFound = true;
                                    LicenseClassID = (int)Reader["LicenseClassID"];
                                    ClassDescription = Reader["ClassDescription"] == DBNull.Value ? "" : Reader["ClassDescription"].ToString();
                                    MinimumAllowedAge = (int)Reader["MinimumAllowedAge"];
                                    DefaultValidityLength = (int)Reader["DefaultValidityLength"];
                                    ClassFees = (decimal)Reader["ClassFees"];
                                }
                            }
                        }
                    }
                }
                catch { return IsFound; }
                return IsFound;
            }

            public static bool UpdateLicenseClass(int LicenseClassID,int MinimumAllowedAge, int DefaultValidityLength, decimal ClassFees)
            {
                int RowsAffected = 0;
                try
                {
                    using (SqlConnection Connection = new SqlConnection(ClsDataAccessSettings.ConnectionString))
                    {
                        string Query = @"UPDATE LicenseClasses
                                     SET    MinimumAllowedAge    = @MinimumAllowedAge,
                                            DefaultValidityLength = @DefaultValidityLength,
                                            ClassFees            = @ClassFees
                                     WHERE  LicenseClassID       = @LicenseClassID";
                        Connection.Open();
                        using (SqlCommand Command = new SqlCommand(Query, Connection))
                        {
                            Command.Parameters.Add("@LicenseClassID", SqlDbType.Int).Value = LicenseClassID;
                            Command.Parameters.Add("@MinimumAllowedAge", SqlDbType.Int).Value = MinimumAllowedAge;
                            Command.Parameters.Add("@DefaultValidityLength", SqlDbType.Int).Value = DefaultValidityLength;
                            Command.Parameters.Add("@ClassFees", SqlDbType.Decimal).Value = ClassFees;

                            RowsAffected = Command.ExecuteNonQuery();
                            return (RowsAffected > 0);
                        }
                    }
                }
                catch { return false; }
            }
        }
    }

