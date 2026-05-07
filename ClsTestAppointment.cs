using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
    public class ClsTestAppointment
    {
            public int TestAppointmentID { get; private set; }
            public int LocalDrivingLicenseApplicationID { get; set; }
            public int TestTypeID { get; set; }
            public DateTime AppointmentDate { get; set; }
            public decimal PaidFees { get; set; }
            public int CreatedByUserID { get; set; }
            public bool IsLocked { get; set; }
            public int? RetakeTestApplicationID { get; set; }
            public enMode Mode { get; private set; }

            public ClsTestAppointment()
            {
                this.TestAppointmentID = -1;
                this.LocalDrivingLicenseApplicationID = -1;
                this.TestTypeID = -1;
                this.AppointmentDate = DateTime.Now.Date;
                this.PaidFees = 0;
                this.CreatedByUserID = -1;
                this.IsLocked = false;
                this.RetakeTestApplicationID = null;
                this.Mode = enMode.AddNew;
            }

            public ClsTestAppointment(int testAppointmentID,
                int localDrivingLicenseApplicationID,
                int testTypeID, DateTime appointmentDate,
                decimal paidFees, int createdByUserID,
                bool isLocked, int? retakeTestApplicationID)
            {
                this.TestAppointmentID = testAppointmentID;
                this.LocalDrivingLicenseApplicationID = localDrivingLicenseApplicationID;
                this.TestTypeID = testTypeID;
                this.AppointmentDate = appointmentDate;
                this.PaidFees = paidFees;
                this.CreatedByUserID = createdByUserID;
                this.IsLocked = isLocked;
                this.RetakeTestApplicationID = retakeTestApplicationID;
                this.Mode = enMode.Update;
            }

            public static DataTable GetAllTestAppointments()
            {
                return ClsTestAppointmentData.GetAllTestAppointments();
            }

            public static DataTable GetAppointmentsForLicApp(  int LocalDrivingLicenseApplicationID)
            {
                return ClsTestAppointmentData.GetAppointmentsForLicApp(LocalDrivingLicenseApplicationID);
            }

            public static ClsTestAppointment Find(int TestAppointmentID)
            {
                int TestTypeID = -1;
                int LicenseAppID = -1;
                DateTime AppointmentDate = DateTime.Now;
                decimal PaidFees = 0;
                int CreatedByUserID = -1;
                bool IsLocked = false;
                int? RetakeTestAppID = null;

                bool IsFound = ClsTestAppointmentData.Find(
                    TestAppointmentID,
                    ref TestTypeID,
                    ref LicenseAppID,
                    ref AppointmentDate,
                    ref PaidFees,
                    ref CreatedByUserID,
                    ref IsLocked,
                    ref RetakeTestAppID);

                if (IsFound)
                    return new ClsTestAppointment(
                        TestAppointmentID,
                        LicenseAppID,
                        TestTypeID,
                        AppointmentDate,
                        PaidFees,
                        CreatedByUserID,
                        IsLocked,
                        RetakeTestAppID);

                return null;
            }

            private bool _Validate()
            {
                // Basic checks
                if (LocalDrivingLicenseApplicationID == -1)
                    return false;

                if (TestTypeID == -1)
                    return false;

                if (CreatedByUserID == -1)
                    return false;

                if (Mode == enMode.AddNew && AppointmentDate.Date < DateTime.Now.Date)
                return false;

            // Fees cannot be negative
            if (PaidFees < 0)
                    return false;

                if (Mode == enMode.AddNew)
                {
                    // Cannot schedule if already has 
                    // active appointment for same test
                    if (ClsTestAppointmentData.HasActiveAppointmentForTest(LocalDrivingLicenseApplicationID, TestTypeID))
                        return false;

                // ✅ Retake validation
                if (RetakeTestApplicationID != null)
                {
                    // cannot retake if already passed
                    if (HasPassedTest(LocalDrivingLicenseApplicationID, TestTypeID))
                        return false;
                }

                // Must pass previous test first
                // Vision(1) → no prerequisite
                // Theory(2) → must pass Vision first
                // Driving(3) → must pass Theory first
                if (TestTypeID > 1)
                    {
                        if (!ClsTestAppointmentData.HasPassedTest(LocalDrivingLicenseApplicationID, TestTypeID - 1))
                            return false;
                    }
                }
            if (Mode == enMode.Update && IsLocked)
                return false;

            return true;
            }

            private bool _AddNew()
            {
                
               // Get fees from TestType
               ClsTestType testType = ClsTestType.Find(this.TestTypeID);

                if (testType != null)
                    this.PaidFees = testType.Fees;

                this.CreatedByUserID = ClsGlobal.CurrentUser.UserID;

                this.TestAppointmentID = ClsTestAppointmentData.AddNew(
                    this.LocalDrivingLicenseApplicationID,
                    this.TestTypeID,
                    this.AppointmentDate,
                    this.PaidFees,
                    this.CreatedByUserID,
                    this.RetakeTestApplicationID);

                return (this.TestAppointmentID != -1);
            }

            private bool _Update()
            {
                return ClsTestAppointmentData.Update(
                    this.TestAppointmentID,
                    
                    this.TestTypeID,
                    this.AppointmentDate,
                    this.PaidFees,
                    this.CreatedByUserID,
                    this.IsLocked,
                    this.RetakeTestApplicationID);
            }

            public static bool Delete(int TestAppointmentID)
            {
                return ClsTestAppointmentData.Delete(TestAppointmentID);
            }

            public static bool HasPassedTest(int LocalDrivingLicenseApplicationID, int TestTypeID)
            {
                return ClsTestAppointmentData.HasPassedTest(LocalDrivingLicenseApplicationID, TestTypeID);
            }

            public static bool HasActiveAppointmentForTest( int LocalDrivingLicenseApplicationID, int TestTypeID)
            {
                return ClsTestAppointmentData.HasActiveAppointmentForTest( LocalDrivingLicenseApplicationID, TestTypeID);
            }

            public bool Save()
            {
            // Set fees before validation
            if (Mode == enMode.AddNew && this.PaidFees == 0)
            {
                ClsTestType testType = ClsTestType.Find(this.TestTypeID);
                if (testType == null) return false;
                this.PaidFees = testType.Fees;
            }
            // Set CreatedByUserID before validation ✅
            if (this.CreatedByUserID == -1)
                this.CreatedByUserID = ClsGlobal.CurrentUser.UserID;

            if (!_Validate())
                    return false;

                switch (Mode)
                {
                    case enMode.AddNew:
                        if (_AddNew())
                        {
                            Mode = enMode.Update;
                            return true;
                        }
                        return false;

                    case enMode.Update:
                        return _Update();
                }
                return false;
            }
        
    }
}