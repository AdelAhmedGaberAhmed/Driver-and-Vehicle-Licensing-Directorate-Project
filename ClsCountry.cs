using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
    public class ClsCountry
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public ClsCountry(int countryID, string countryName)
        {
            this.CountryID = countryID;
            this.CountryName = countryName;
        }

        public static DataTable GetAllCountries()
        {
            return (ClsCountryData.GetAllCountries());
        }
        
        
        // Find by ID — for Web API
        public static ClsCountry Find(int CountryID)
        {
            string CountryName = "";

            bool IsFound = ClsCountryData.Find(CountryID, ref CountryName);

            if (IsFound)
                return new ClsCountry(CountryID, CountryName);
            else
                return null;
        }

        // Find by Name — for search
        public static ClsCountry Find(string CountryName)
        {
            int CountryID = -1;

            bool IsFound = ClsCountryData.Find(CountryName, ref CountryID);

            if (IsFound)
                return new ClsCountry(CountryID, CountryName);
            else
                return null;
        }
    }
}
