using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
    public class ClsDriver
    {
        public int DriverID { get; set; }
        public int PersonID { get; set; }
        public int CreatedByUserID { get; set; }
        public DateTime CreatedDate { get; set; }

        public ClsDriver()
        {
            this.DriverID = -1;
            this.PersonID = -1;
            this.CreatedByUserID = -1;
            this.CreatedDate = DateTime.Now;
        }

        public ClsDriver (int driverID, int personID, int createdByUserID, DateTime createdDate)
        {
            this.DriverID = driverID;
            this.PersonID = personID;
            this.CreatedByUserID = createdByUserID;
            this.CreatedDate = createdDate;
        }

        public static DataTable GetAllDrivers()
        {
            return ClsDriverData.GetAllDrivers();
        }

        public static int GetDriverIDByPersonID(int PersonID)
        {
            return ClsDriverData.GetDriverIDByPersonID(PersonID);
        }

      
        public static int AddNewDriver(int PersonID)
        {
            if (!ClsDriver.IsPersonDriver(PersonID) || PersonID == -1)
                return -1;

            return ClsDriverData.AddNewDriver(PersonID, ClsGlobal.CurrentUser.UserID, DateTime.Now);
        }
        public static ClsDriver Find(int DriverID)
        {
            int personID = -1;
            int CreatedByUserID = -1;
            DateTime CreatedDate = DateTime.Now;

            bool IsFound = false;
            IsFound = ClsDriverData.Find(DriverID,ref personID,ref CreatedByUserID, ref CreatedDate);
            if (IsFound)
            {
                return new ClsDriver(DriverID , personID , CreatedByUserID, CreatedDate);
            }
            return null;
        }

        // Very useful check
        public static bool IsPersonDriver(int PersonID)
        {
            return ClsDriverData.GetDriverIDByPersonID(PersonID) != -1;
        }


    }
}
