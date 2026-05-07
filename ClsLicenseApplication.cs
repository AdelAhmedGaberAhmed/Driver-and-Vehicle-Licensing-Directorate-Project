using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{

    //  Clear and maintainable
    public enum enApplicationType
    {
        NewDrivingLicense = 1,
        RetakeTest = 2,
        RenewLicense = 3,
        ReplaceLostLicense = 4,
        ReplaceDamagedLicense = 5,
        ReleaseDetained = 6,
        NewInternationalLicense = 7
    }

    public enum enValidationResultLicenseApp
    {
        Valid,
        InvalidPerson,
        InvalidLicenseClass,
        AgeNotAllowed,
        HasActiveApplication,
        AlreadyHasLicense,
        MissingApplicationID
    }

    public class ClsLicenseApplication
    {
        public int LicenseApplicationID { get; private set; }
        public int ApplicationID { get; set; }
        public int LicenseClassID { get; set; }
        public int MinimumAllowedAge { get; private set; }


        public int PersonID { get; set; }
        public enMode Mode { get;  private set; }




        // Default — AddNew 
        public ClsLicenseApplication()
        {
            this.LicenseApplicationID = -1;
            this.ApplicationID = -1;
            this.LicenseClassID = -1;
            this.PersonID = -1;
            this.Mode = enMode.AddNew;
        }

        // Parameterized — Update 
        public ClsLicenseApplication(int licenseApplicationID, int applicationID, int licenseClassID, int personID , int minimumAllowedAge )
        {
            this.LicenseApplicationID = licenseApplicationID;
            this.ApplicationID        = applicationID;
            this.LicenseClassID       = licenseClassID;
            this.PersonID             = personID;
            this.Mode                 = enMode.Update;
            this.MinimumAllowedAge    = minimumAllowedAge;
        }

        public static ClsLicenseApplication FindByLicenseApplicationID(int LicenseApplicationID )
        {
          

            int ApplicationID = -1;
            int LicenseClassID = -1;
            int PersonID = -1;
            int MinimumAllowedAge = -1;

            bool IsFound = ClsLicenseApplicationData
                .FindByLicenseApplicationID
                (
                    LicenseApplicationID,
                    ref ApplicationID,
                    ref LicenseClassID,
                    ref PersonID,
                    ref MinimumAllowedAge);

            if (IsFound)
                return new ClsLicenseApplication(
                    LicenseApplicationID,
                    ApplicationID,
                    LicenseClassID,
                    PersonID ,
                    MinimumAllowedAge);

            return null;
        }

        public static ClsLicenseApplication FindByApplicationID(int ApplicationID)
        {
            int LicenseApplicationID = -1;
            int LicenseClassID = -1;
            int PersonID = -1;
            int MinimumAllowedAge = -1;

            bool IsFound = ClsLicenseApplicationData.FindByApplicationID(
                     ApplicationID,            // ✅ pass ApplicationID
                    ref LicenseApplicationID,
                    ref LicenseClassID,
                    ref PersonID,
                    ref MinimumAllowedAge);

            if (IsFound)
                return new ClsLicenseApplication(
                    LicenseApplicationID,
                    ApplicationID,
                    LicenseClassID,
                    PersonID,
                    MinimumAllowedAge);

            return null;
        }

        public static DataTable GetAllLicenseApplication()
        {
            return ClsLicenseApplicationData.GetAllLicenseApplication();
            
        }

        private bool _AddNewLicenseApplication()
        {
            // Step 1 — Create and save the Application first
            ClsApplication app = new ClsApplication();

            app.PersonID      = this.PersonID;
            app.ApplicationTypeID = (int) enApplicationType.NewDrivingLicense ; // 1 = New License

            // ✅ Get from ApplicationType
            ClsApplicationType appType = ClsApplicationType.Find((int)enApplicationType.NewDrivingLicense);

            app.PaidFees = appType.ApplicationFees;
            app.CreatedByUserID = ClsGlobal.CurrentUser.UserID;
            app.ApplicationStatus = enApplicationStatus.New;
            app.ApplicationDate = DateTime.Now;
            app.LastStatusDate = DateTime.Now;

            if (!app.Save())
                return false;

            // Step 2 — Store the ApplicationID
            this.ApplicationID = app.ApplicationID;

            // Step 3 — Save LicenseApplication record
            this.LicenseApplicationID = ClsLicenseApplicationData.AddNew(this.ApplicationID, this.LicenseClassID);

            // ✅ FIX: Rollback Check
            if (this.LicenseApplicationID == -1)
            {
                // If the second insert fails, delete the first one to keep the DB clean
                ClsApplication.DeleteApplication(this.ApplicationID);
                return false;
            }
            return true;
        }

        public  bool Delete()
        {
            // Delete LicenseApplication first
            if (!ClsLicenseApplicationData.Delete(this.LicenseApplicationID))
                return false;

            // Then delete the Application
            return ClsApplication.DeleteApplication(this.ApplicationID);
        }

     
        private enValidationResultLicenseApp _Validate()
        {
            // 1) Basic checks (cheap)
            if (LicenseClassID == -1)
                return enValidationResultLicenseApp.InvalidLicenseClass;

            if (PersonID == -1)
                return enValidationResultLicenseApp.InvalidPerson;

            // 2) Update mode check
            if (Mode == enMode.Update)
            {
                if (ApplicationID == -1)
                    return enValidationResultLicenseApp.MissingApplicationID;

                return enValidationResultLicenseApp.Valid;
            }

            // =========================
            // AddNew Mode Validations
            // =========================

            // 3) Load License Class (DB)
            ClsLicenseClass lc = ClsLicenseClass.Find(LicenseClassID);
            if (lc == null)
                return enValidationResultLicenseApp.InvalidLicenseClass;

            // ✔ Store it (important)
            this.MinimumAllowedAge = lc.MinimumAllowedAge;

            // 4) Load Person (DB)
            ClsPerson person = ClsPerson.Find(PersonID);
            if (person == null)
                return enValidationResultLicenseApp.InvalidPerson;

            // 5) Age check (NO DB — fast)
            int age = DateTime.Now.Year - person.DateOfBirth.Year;
            if (DateTime.Now < person.DateOfBirth.AddYears(age))
                age--;

            if (age < this.MinimumAllowedAge)
                return enValidationResultLicenseApp.AgeNotAllowed;

            // 6) Check Active Application (DB)
            if (ClsLicenseApplicationData.DoesPersonHaveActiveLicenseApplication(PersonID, LicenseClassID))
                return enValidationResultLicenseApp.HasActiveApplication;

            // 7) Check Existing License (DB)
            if (ClsLicenseApplicationData
                .DoesPersonHaveLicenseClass(PersonID, LicenseClassID))
                return enValidationResultLicenseApp.AlreadyHasLicense;

            return enValidationResultLicenseApp.Valid;
        }

        public bool Save()
        {
            var result = _Validate();

            if (result != enValidationResultLicenseApp.Valid)
            {
                return false;
            }
            // Only AddNew supported 
            // No Update — to change class, delete and create new
            if (Mode == enMode.AddNew)
            {
                if (_AddNewLicenseApplication())
                {
                    Mode = enMode.Update;
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
