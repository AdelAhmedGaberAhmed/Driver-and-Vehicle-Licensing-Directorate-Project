using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DVLD.Business
{
    public class ClsDetainedLicense
    {
 
      // ── Properties ───────────────────────────────
            public int DetainID { get; private set; }
            public int LicenseID { get; set; }
            public DateTime DetainDate { get; set; }
            public decimal FineFees { get; set; }
            public int CreatedByUserID { get; set; }
            public bool IsReleased { get; set; }
            public DateTime? ReleaseDate { get; set; }
            public int? ReleasedByUserID { get; set; }
            public int? ReleaseApplicationID { get; set; }
            public enMode Mode { get; private set; }

            // ── Constructors ─────────────────────────────
            public ClsDetainedLicense()
            {
                this.DetainID = -1;
                this.LicenseID = -1;
                this.DetainDate = DateTime.Now;
                this.FineFees = 0;
                this.CreatedByUserID = -1;
                this.IsReleased = false;
                this.ReleaseDate = null;
                this.ReleasedByUserID = null;
                this.ReleaseApplicationID = null;
                this.Mode = enMode.AddNew;
            }

            public ClsDetainedLicense(int detainID, int licenseID,
                DateTime detainDate, decimal fineFees, int createdByUserID,
                bool isReleased, DateTime? releaseDate,
                int? releasedByUserID, int? releaseApplicationID)
            {
                this.DetainID = detainID;
                this.LicenseID = licenseID;
                this.DetainDate = detainDate;
                this.FineFees = fineFees;
                this.CreatedByUserID = createdByUserID;
                this.IsReleased = isReleased;
                this.ReleaseDate = releaseDate;
                this.ReleasedByUserID = releasedByUserID;
                this.ReleaseApplicationID = releaseApplicationID;
                this.Mode = enMode.Update;
            }

            // ── Find Methods ─────────────────────────────
            public static ClsDetainedLicense Find(int DetainID)
            {
                int LicenseID = -1;
                DateTime DetainDate = DateTime.Now;
                decimal FineFees = 0;
                int CreatedByUserID = -1;
                bool IsReleased = false;
                DateTime? ReleaseDate = null;
                int? ReleasedByUserID = null;
                int? ReleaseApplicationID = null;

                bool IsFound = ClsDetainedLicenseData.Find(
                    DetainID,
                    ref LicenseID,
                    ref DetainDate,
                    ref FineFees,
                    ref CreatedByUserID,
                    ref IsReleased,
                    ref ReleaseDate,
                    ref ReleasedByUserID,
                    ref ReleaseApplicationID);

                if (IsFound)
                    return new ClsDetainedLicense(
                        DetainID, LicenseID, DetainDate, FineFees,
                        CreatedByUserID, IsReleased, ReleaseDate,
                        ReleasedByUserID, ReleaseApplicationID);

                return null;
            }

            // ── Get Active Detain By LicenseID ───────────
            public static ClsDetainedLicense GetActiveDetainByLicenseID( int LicenseID)
            {
                int DetainID = -1;
                DateTime DetainDate = DateTime.Now;
                decimal FineFees = 0;
                int CreatedByUserID = -1;
                bool IsReleased = false;
                DateTime? ReleaseDate = null;
                int? ReleasedByUserID = null;
                int? ReleaseApplicationID = null;

                bool IsFound = ClsDetainedLicenseData.FindByLicenseID(
                    LicenseID,
                    ref DetainID,
                    ref DetainDate,
                    ref FineFees,
                    ref CreatedByUserID,
                    ref IsReleased,
                    ref ReleaseDate,
                    ref ReleasedByUserID,
                    ref ReleaseApplicationID);

                if (IsFound)
                    return new ClsDetainedLicense(
                        DetainID, LicenseID, DetainDate, FineFees,
                        CreatedByUserID, IsReleased, ReleaseDate,
                        ReleasedByUserID, ReleaseApplicationID);

                return null;
            }

            public static DataTable GetAllDetainedLicenses()
            {
                return ClsDetainedLicenseData.GetAllDetainedLicenses();
            }

            // ── Check Methods ────────────────────────────
            public static bool IsLicenseDetained(int LicenseID)
            {
                return ClsDetainedLicenseData.IsLicenseDetained(LicenseID);
            }

            // ── Validation ───────────────────────────────
            private bool _Validate()
            {
                // LicenseID must be valid
                if (LicenseID == -1)
                    return false;

                // FineFees cannot be negative
                if (FineFees < 0)
                    return false;
            if (ClsGlobal.CurrentUser == null)
                return false; // or throw

            if (Mode == enMode.AddNew)
                {
                    // Rule 4 — License must be ACTIVE to detain
                    ClsLicense license = ClsLicense.Find(LicenseID);
                    if (license == null)
                        return false;

                    if (!license.IsActive)
                        return false;

                    // Rule 1 — Cannot detain already detained license
                    if (IsLicenseDetained(LicenseID))
                        return false;
                }

                return true;
            }

            // ── Private Methods ──────────────────────────
            private bool _AddNew()
            {

                this.CreatedByUserID = ClsGlobal.CurrentUser.UserID;
                this.DetainDate = DateTime.Now;

                this.DetainID = ClsDetainedLicenseData.AddNew(
                    this.LicenseID,
                    this.DetainDate,
                    this.FineFees,
                    this.CreatedByUserID);

                if (DetainID == -1)
                    return false;

                // Deactivate the license
                ClsLicense license = ClsLicense.Find(this.LicenseID);
                if (license != null)
                {
                    license.IsActive = false;
                    license.Save();
                }

                return true;
            }

        // ── Release License ──────────────────────────
        public bool ReleaseLicense(out int ApplicationID)
        {
            ApplicationID = -1;

            // 1. Rule Check: Cannot release if already released
            if (this.IsReleased)
                return false;

            // Get current user from your Global class
            int currentUserID = ClsGlobal.CurrentUser?.UserID ?? -1;
            if (currentUserID == -1) return false;


            // 2. Get PersonID via License and Driver
            ClsLicense license = ClsLicense.Find(this.LicenseID);
            if (license == null) return false;

            ClsDriver driver = ClsDriver.Find(license.DriverID);
            if (driver == null) return false;

            // 3. Get Application Type Info (Type 6: Release Detained)
            ClsApplicationType appType = ClsApplicationType.Find((int)enApplicationType.ReleaseDetained);
            if (appType == null) return false;

            // 4. Create and Save the Application
            ClsApplication releaseApp = new ClsApplication();
            releaseApp.PersonID = driver.PersonID;
            releaseApp.ApplicationDate = DateTime.Now;
            releaseApp.ApplicationTypeID = appType.ID; // ID = 6
            releaseApp.ApplicationStatus = enApplicationStatus.Completed;
            releaseApp.LastStatusDate = DateTime.Now;
            releaseApp.PaidFees = appType.ApplicationFees;
            releaseApp.CreatedByUserID = currentUserID;

            if (!releaseApp.Save())
                return false;

            ApplicationID = releaseApp.ApplicationID;

            // 5. Update DetainedLicenses Table
            bool detentionReleased = ClsDetainedLicenseData.ReleaseLicense(
                this.DetainID,
                DateTime.Now,
                currentUserID,
                releaseApp.ApplicationID);

            if (!detentionReleased)
            {
                // ROLLBACK: Delete the application if we couldn't update the detention record
                ClsApplication.DeleteApplication(releaseApp.ApplicationID);
                ApplicationID = -1;
                return false;
            }

            // 6. Reactivate the License
            license.IsActive = true;
            if (!license.Save())
            {
                // ROLLBACK: This is a deep failure. 
                // 1. Attempt to "Un-release" the detention record (logic depends on your DAL)
                // 2. Delete the application
                ClsApplication.DeleteApplication(releaseApp.ApplicationID);
                ApplicationID = -1;
                return false;
            }

            // 7. Update object in memory
            this.IsReleased = true;
            this.ReleaseDate = DateTime.Now;
            this.ReleasedByUserID = ReleasedByUserID;
            this.ReleaseApplicationID = releaseApp.ApplicationID;

            return true;
        }

        // ── Public Methods ───────────────────────────


        public bool Save()
            {
                if (!_Validate())
                    return false;

                // Only AddNew supported
                // Cannot update a detention — only release it
                if (Mode == enMode.AddNew)
                {
                    if (_AddNew())
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

