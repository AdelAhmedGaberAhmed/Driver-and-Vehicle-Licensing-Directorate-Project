using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
    public  enum enIssueReason
    {
        FirstTime = 1,
        Renew = 2,
        ReplacementLost = 3,
        ReplacementDamaged = 4
    }
 
    public enum enTestTypeID
    {
        Vision = 1 ,
       Theory  = 2 ,
       Driving = 3
    }

    public class ClsLicense
    {
            public int LicenseID { get; private set; }
            public int DriverID { get; set; }
            public int LicenseClassID { get; set; }
            public int ApplicationID { get; set; }
            public DateTime IssueDate { get; set; }
            public DateTime ExpirationDate { get; set; }

            public string Notes { get; set; }

            public decimal PaidFees { get; set; }

            public bool IsActive { get; set; }

            public enIssueReason IssueReason { get; set; }

            public int CreatedByUserID { get; set; }

            public enMode Mode { get; private set; }

        
        public ClsLicense(int licenseID, int driverID, int licenseClassID, int applicationID, DateTime issueDate, DateTime expirationDate,
            string notes, decimal paidFees, bool isActive, enIssueReason issueReason, int createdByUserID)
        {
            LicenseID = licenseID;
            DriverID = driverID;
            LicenseClassID = licenseClassID;
            ApplicationID = applicationID;
            IssueDate = issueDate;
            ExpirationDate = expirationDate;
            Notes = notes;
            PaidFees = paidFees;
            IsActive = isActive;
            IssueReason = issueReason;
            CreatedByUserID = createdByUserID;
            Mode = enMode.Update;
        }
        public ClsLicense()
        {
            this.LicenseID = -1;
            this.DriverID = -1;
            this.LicenseClassID = -1;
            this.ApplicationID = -1;
            this.CreatedByUserID = -1;
            this.IssueReason = enIssueReason.FirstTime;
            this.IssueDate = DateTime.Now; 
            this.ExpirationDate = DateTime.Now;
            this.Notes = null;
            this.IsActive = false;
            Mode = enMode.AddNew;
            
        }

        public static DataTable GetDriverLicenses(int DriverID)
        {
            return ClsLicenseData.GetDriverLicenses(DriverID);
        }

        public static DataTable GetAllLicenses()
        {
            return ClsLicenseData.GetAllLicense();
        }
        private bool _AddNewLicense()
        {
            // ✅ Mark application as Completed

            ClsApplication app = ClsApplication.Find(this.ApplicationID);

            if (app == null) return false;

           
          
           this.LicenseID = ClsLicenseData.AddNewLicense(this.ApplicationID, this.DriverID, this.LicenseClassID, this.IssueDate, this.ExpirationDate,
           this.Notes, this.PaidFees, this.IsActive, (int)this.IssueReason, this.CreatedByUserID);

            if (this.LicenseID == -1) return false;

            app.ApplicationStatus = enApplicationStatus.Completed;


            if (!app.Save())
            {
                // ✅ Rollback if application update fails
                // Assuming you have a Delete method in ClsLicenseData
                 ClsLicenseData.DeleteLicense(this.LicenseID); 
                return false;
            }

            return true;
        }


        public static ClsLicense FindByApplicationID(int applicationID)
        {
            int licenseID = -1;
            int driverID = -1;
            int licenseClassID = -1;
            DateTime issueDate = DateTime.Now;
            DateTime expirationDate = DateTime.Now;
            string notes = "";
            decimal paidFees = 0;
            bool isActive = false;
            int issueReason = 0;
            int createdByUserID = -1;

            bool isFound = ClsLicenseData.FindByApplicationID(
                applicationID,
                ref licenseID,
                ref driverID,
                ref licenseClassID,
                ref issueDate,
                ref expirationDate,
                ref notes,
                ref paidFees,
                ref isActive,
                ref issueReason,
                ref createdByUserID
            );

            if (!isFound)
                return null;

            return new ClsLicense(
                licenseID,
                driverID,
                licenseClassID,
                applicationID,
                issueDate,
                expirationDate,
                notes,
                paidFees,
                isActive,
                (enIssueReason)issueReason,
                createdByUserID
            );
        }
        public static bool IsLicenseActive(int LicenseID)
        {
            ClsLicense license = Find(LicenseID);
            if (license == null) return false;
            return license.IsActive && !license.IsExpired();
        }
        private bool _UpdateLicense()
        {
            return ClsLicenseData.UpdateLicense(this.LicenseID, this.LicenseClassID, this.IssueDate, this.ExpirationDate,
                this.Notes,this.IsActive,(int)this.IssueReason, this.CreatedByUserID);

        }

        private void _SetExpirationDate()
        {
            ClsLicenseClass cls = ClsLicenseClass.Find(this.LicenseClassID);

            if (cls != null)
            
                this.ExpirationDate = this.IssueDate.AddYears(cls.DefaultValidityLength);

        }
       
        public bool IsExpired()
        {
            return DateTime.Now > ExpirationDate;
        }

        public static ClsLicense Find(int licenseID)
        {
            int driverID = -1;
            int licenseClassID = -1;
            int applicationID = -1;
            DateTime issueDate = DateTime.Now;
            DateTime expirationDate = DateTime.Now;
            string notes = "";
            decimal paidFees = 0;
            bool isActive = false;
            int issueReason = 1;
            int createdByUserID = -1;

            bool isFound = ClsLicenseData.Find(
                licenseID,
                ref driverID,
                ref licenseClassID,
                ref applicationID,
                ref issueDate,
                ref expirationDate,
                ref notes,
                ref paidFees,
                ref isActive,
                ref issueReason,
                ref createdByUserID
            );

            if (!isFound)
                return null;

            return new ClsLicense(
                licenseID,
                driverID,
                licenseClassID,
                applicationID,
                issueDate,
                expirationDate,
                notes,
                paidFees,
                isActive,
                (enIssueReason)issueReason,
                createdByUserID
            );
        }
        public bool Deactivate()
        {
            this.IsActive = false;
            return Save();
        }
        public static bool HasActiveLicense(int driverID, int licenseClassID)
        {
            return ClsLicenseData.HasActiveLicense(driverID, licenseClassID); 
        }

        private bool _Validate()
        {
            if (DriverID == -1) return false;
            if (LicenseClassID == -1) return false;
            if (CreatedByUserID == -1) return false;


            //   Must have valid ApplicationID
            if (ApplicationID == -1) return false;
            //  Fees cannot be negative
            if (PaidFees < 0) return false;

            
            // Expiration must be after issue
            if (ExpirationDate <= IssueDate)
                return false;


            // Cannot create license if already active one exists
            if (Mode == enMode.AddNew && HasActiveLicense(DriverID, LicenseClassID))
                return false;


            // ✅ Only check tests for FirstTime -- For AddNew: person must have passed all 3 tests

            if (Mode == enMode.AddNew && IssueReason == enIssueReason.FirstTime)
            {
                // Get LocalDrivingLicenseApplicationID from ApplicationID
                ClsLicenseApplication licApp = ClsLicenseApplication.FindByApplicationID(ApplicationID);
                if (licApp == null) return false;

                // 3. Check tests

                if (!ClsTestAppointment.HasPassedTest(licApp.LicenseApplicationID, (int)enTestTypeID.Vision))
                    return false;

                if (!ClsTestAppointment.HasPassedTest(licApp.LicenseApplicationID, (int)enTestTypeID.Theory))
                    return false;

                if (!ClsTestAppointment.HasPassedTest(licApp.LicenseApplicationID, (int)enTestTypeID.Driving))
                    return false;

            }

            return true;
        }

        private ClsLicense CreateNewLicense(enIssueReason reason)
        {
            if (!ClsLicenseData.IsLicenseExist(this.LicenseID)) return null;

            if (!this.IsActive)
                return null;

            if (this.DriverID == -1 || this.LicenseClassID == -1)
                return null;

            int currentUserID = ClsGlobal.CurrentUser?.UserID ?? -1;
            if (currentUserID == -1)
                return null;

            // ✅ Get Driver → Person
            var driver = ClsDriver.Find(this.DriverID);
            if (driver == null)
                return null;


            // ✅ Get License Class (for fees)
            ClsLicenseClass lc = ClsLicenseClass.Find(this.LicenseClassID);
            if (lc == null)
                return null;

            ClsApplicationType appType = null;
            switch (reason)
            {
                case enIssueReason.Renew:
                    appType = ClsApplicationType.Find((int)enApplicationType.RenewLicense);
                    break;
                case enIssueReason.ReplacementLost:
                    appType = ClsApplicationType.Find((int)enApplicationType.ReplaceLostLicense);
                    break;
                case enIssueReason.ReplacementDamaged:
                    appType = ClsApplicationType.Find((int)enApplicationType.ReplaceDamagedLicense);
                    break;
                default:
                    return null;
            }
            if (appType == null) return null;

            //  Create App
            ClsApplication newApp = new ClsApplication();
            newApp.PersonID = driver.PersonID;
            newApp.CreatedByUserID = currentUserID;
            newApp.ApplicationTypeID = appType.ID;
            newApp.PaidFees = appType.ApplicationFees; // ✅ App fees are NOT the license fees

            if (!newApp.Save()) return null;

            this.IsActive = false;

            if (!this.Save())
            {
                // Rollback App if we can't deactivate the old license
                ClsApplication.DeleteApplication(newApp.ApplicationID);
                return null;
            }

            // ✅  Create new License
            ClsLicense newLicense = new ClsLicense();
            newLicense.DriverID = this.DriverID;
            newLicense.LicenseClassID = this.LicenseClassID;
            newLicense.ApplicationID = newApp.ApplicationID;
            newLicense.IssueReason = reason;
            newLicense.IsActive = true;
            newLicense.CreatedByUserID = currentUserID;
            newLicense.PaidFees = lc.ClassFees;

            if (!newLicense.Save())
            {
                // ROLLBACK: If new license fails to save, Reactivate the old one
                this.IsActive = true;
                this.Save();
                // Rollback App
                ClsApplication.DeleteApplication(newApp.ApplicationID);
                return null;
            }

            return newLicense;
        }
        public ClsLicense Renew()
        {
            // A license must be Active AND Expired to be renewed.
            if (!this.IsActive || !this.IsExpired())
                return null;

            return CreateNewLicense(enIssueReason.Renew);
        }
        public ClsLicense ReplacmentForDamaged()
        {
            return CreateNewLicense(enIssueReason.ReplacementDamaged);

        }
        public ClsLicense ReplacmentForLost()
        {
           return CreateNewLicense(enIssueReason.ReplacementLost);

        }
        private static int _GetOrCreateDriver(int personID)
        {
            if (personID <= 0)
                return -1;

            int driverID = ClsDriver.GetDriverIDByPersonID(personID);

            if (driverID > 0)
                return driverID;

            driverID = ClsDriver.AddNewDriver( personID);

            if (driverID <= 0)
                return -1;

            return driverID;
        }
        public static bool IsLicenseExist(int LicenseID)
        {
            return ClsLicenseData.IsLicenseExist(LicenseID);
        }
        public static ClsLicense IssueLicense(int applicationID, string notesapp)
        {
            
            // 1. Get application

            ClsApplication app =  ClsApplication.Find(applicationID);
            if (app == null) return null;

            // 2. Get license application
            ClsLicenseApplication lcApp = ClsLicenseApplication.FindByApplicationID(applicationID);
            if (lcApp == null) return null;

         

            int DriverID =  _GetOrCreateDriver(app.PersonID);
            if (DriverID == -1 )
                return null;

            if (HasActiveLicense(DriverID, lcApp.LicenseClassID))
                return null;

            // Get the License Class to apply the correct fees
            ClsLicenseClass lc = ClsLicenseClass.Find(lcApp.LicenseClassID);
            if (lc == null) return null;

            // 5. Create License object
            ClsLicense license = new ClsLicense();
            license.DriverID = DriverID;
            license.ApplicationID = applicationID;
            license.LicenseClassID = lcApp.LicenseClassID;
            license.Notes = notesapp;
            license.IsActive = true;
            license.IssueReason = enIssueReason.FirstTime;
            license.PaidFees = lc.ClassFees;
            // 6. Save
            if (license.Save())
                return license;

            return null;
        }
        public bool Save()
        {
            // Set dates first 
            if (Mode == enMode.AddNew)
            {
                this.IssueDate = DateTime.Now;
                _SetExpirationDate();
            }

            // Set user before validation 
            
            if (this.CreatedByUserID == -1)
            {
                if (ClsGlobal.CurrentUser != null)
                    this.CreatedByUserID = ClsGlobal.CurrentUser.UserID;
                else
                    return false; // Cannot save without a valid user
            }

            // Now validate 
            if (!_Validate())
                return false;

            switch (Mode)
            {
                case enMode.AddNew:

                   
                    if (_AddNewLicense())
                    {
                        Mode = enMode.Update;
                        return true;
                    }

                    else return false;

                case enMode.Update:
                    return _UpdateLicense();
            }
            return false;
            

        }

    }
}
