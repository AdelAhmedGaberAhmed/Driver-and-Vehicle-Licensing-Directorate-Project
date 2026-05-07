using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
    public class ClsInternationalLicense
    {

        public int InternationalLicenseID { get; private set; }
        public int ApplicationID { get; set; }
        public int IssuedUsingLocalLicenseID { get; set; }
        public int DriverID { get; set; }
        public int CreatedByUserID { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public enMode Mode { get; private set; }



        public ClsInternationalLicense()
        {
            this.InternationalLicenseID = -1;
            this.ApplicationID = -1;
            this.IssuedUsingLocalLicenseID = -1;
            this.DriverID= -1;
            this.CreatedByUserID = -1;
            this.IssueDate = DateTime.Now;
            this.ExpirationDate = DateTime.MaxValue;
            this.IsActive = true;
            this.Mode = enMode.AddNew;
        }

        public ClsInternationalLicense(int internationalLicenseID, int applicationID, int issuedIsingLocalLicenseID, 
            int driverID, int createdByUserID, DateTime issueDate, DateTime expirationDate, bool isActive)
        {
            InternationalLicenseID = internationalLicenseID;
            ApplicationID = applicationID;
            IssuedUsingLocalLicenseID = issuedIsingLocalLicenseID;
            DriverID = driverID;
            CreatedByUserID = createdByUserID;
            IssueDate = issueDate;
            ExpirationDate = expirationDate;
            IsActive = isActive;
            Mode = enMode.Update;
        }
        public static ClsInternationalLicense Find(int internationalLicenseID)
        {
            int driverID = -1;
            int applicationID = -1;
            int createdByUserID = -1;
            int IssuedUsingLocalLicenseID = -1;
            DateTime issueDate = DateTime.Now;
            DateTime expirationDate = DateTime.Now;
            bool isActive = false;

            bool isFound = ClsInternationalLicenseData.Find(
                internationalLicenseID,
                ref driverID,
                ref applicationID,
                ref createdByUserID,
                ref IssuedUsingLocalLicenseID,
                ref issueDate,
                ref expirationDate,
                ref isActive
            );

            if (!isFound)
                return null;

            return new ClsInternationalLicense(
                internationalLicenseID,
                applicationID,
                IssuedUsingLocalLicenseID, 
                driverID,
                createdByUserID,
                issueDate,
                expirationDate,
                isActive
            );
        }
        private bool _Validate()
        {
            if (ApplicationID == -1) return false;
            if (DriverID == -1) return false;
            if (IssuedUsingLocalLicenseID == -1) return false;
            if (CreatedByUserID == -1) return false;

            if (ExpirationDate <= IssueDate)
                return false;

            return true;
        }
        public static DataTable GetDriverLicenses(int driverID)
        {
            return ClsInternationalLicenseData.GetDriverIntlLicenses(driverID);
        }
        public static DataTable GetAllIntLicense()
        {
            return ClsInternationalLicenseData.GetAllInternationalLicenses();
        }
        public static ClsInternationalLicense FindByApplicationID(int applicationID)
        {
            int internationalLicenseID = -1;
            int driverID = -1;
            int issuedUsingLocalLicenseID = -1;
            int createdByUserID = -1;
            DateTime issueDate = DateTime.Now;
            DateTime expirationDate = DateTime.Now;
            bool isActive = false;

            bool isFound = ClsInternationalLicenseData.FindByApplicationID(
                applicationID,
                ref internationalLicenseID,
                ref driverID,
                ref issuedUsingLocalLicenseID,
                ref createdByUserID,
                ref issueDate,
                ref expirationDate,
                ref isActive
            );

            if (!isFound)
                return null;

            return new ClsInternationalLicense(
                internationalLicenseID,
                applicationID,
                issuedUsingLocalLicenseID,
                driverID,
                createdByUserID,
                issueDate,
                expirationDate,
                isActive
            );
        }

        public static ClsInternationalLicense Issue(int applicationID)
        {
            // 1. Get Application
            ClsApplication app = ClsApplication.Find(applicationID);
            if (app == null) return null;

            if (app.ApplicationStatus != enApplicationStatus.New)
                return null;
           
            // 2. Get Local License

            ClsInternationalLicense intlApp = ClsInternationalLicense.FindByApplicationID(applicationID);
            if (intlApp == null) return null;


            ClsLicense localLicense = ClsLicense.Find(intlApp.IssuedUsingLocalLicenseID);
            if (localLicense == null) return null;

            
            // 3. Check local license is active
            if (!localLicense.IsActive || localLicense.IsExpired())
                return null;

            var driver = ClsDriver.Find(localLicense.DriverID);
            if (driver == null) return null;

            if (app.PersonID != driver.PersonID)
                return null;

            // 4. Check already has active international license
            if (ClsInternationalLicenseData.HasActiveIntlLicense(localLicense.DriverID))
                return null;

            if (ClsDetainedLicense.IsLicenseDetained(localLicense.LicenseID)) 
                return null; 

            //  Must have Class 3 local license
            if (localLicense.LicenseClassID != 3) 
                return null ; 

            // 5. Create object
            ClsInternationalLicense intl = new ClsInternationalLicense();

            intl.ApplicationID = applicationID;
            intl.DriverID = localLicense.DriverID;
            intl.IssuedUsingLocalLicenseID = localLicense.LicenseID;
            intl.CreatedByUserID = ClsGlobal.CurrentUser.UserID;

            intl.IssueDate = DateTime.Now;
            intl.ExpirationDate = DateTime.Now.AddYears(1); // usually 1 year
            intl.IsActive = true;

            // 6. Save
            return intl.Save() ? intl : null;
        }

      
        public bool Save()
        {
            if (!_Validate())
                return false;

            if (Mode == enMode.AddNew)
            {
                this.InternationalLicenseID = ClsInternationalLicenseData.AddNew(
                    this.ApplicationID,
                    this.DriverID,
                    this.CreatedByUserID,
                    this.IssuedUsingLocalLicenseID,
                    this.IssueDate,
                    this.ExpirationDate,
                    this.IsActive
                );

                if (this.InternationalLicenseID == -1)
                    return false;

                Mode = enMode.Update;
                return true;
            }

            return false; // no update supported
        }

    }
}

