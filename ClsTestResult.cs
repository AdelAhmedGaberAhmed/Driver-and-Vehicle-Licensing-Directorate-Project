using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
   
    public class ClsTestResult
    {
        // ── Properties ───────────────────────────────
        public int TestID { get; private set; }
        public int TestAppointmentID { get; private set; }
        public bool TestResult { get; set; }  // false=Fail, true=Pass
        public string Notes { get; set; }
        public int CreatedByUserID { get; set; }
        public enMode Mode { get; private set; }

        // Shortcut — easy to check
        public bool IsPassed { get { return TestResult; } }
        public bool IsFailed { get { return !TestResult; } }

        // ── Constructors ─────────────────────────────
        public ClsTestResult()
        {
            this.TestID = -1;
            this.TestAppointmentID = -1;
            this.TestResult = false;
            this.Notes = "";
            this.CreatedByUserID = -1;
            this.Mode = enMode.AddNew;
        }

        public ClsTestResult( int TestID , int testAppointmentID, bool testResult,string notes, int createdByUserID)
        {
            this.TestID = TestID;
            this.TestAppointmentID = testAppointmentID;
            this.TestResult = testResult;
            this.Notes = notes;
            this.CreatedByUserID = createdByUserID;
            this.Mode = enMode.Update;
        }

        // ── Find ─────────────────────────────────────
        public static ClsTestResult Find(int TestID)
        {
            bool TestResult = false;
            string Notes = "";
            int  TestAppointmentID =-1 ;
            int CreatedByUserID = -1;

            bool IsFound = ClsTestResultData.Find(
                TestID , 
                ref TestAppointmentID,
                ref TestResult,
                ref Notes,
                ref CreatedByUserID);

            if (IsFound)
                return new ClsTestResult(
                    TestID ,
                    TestAppointmentID,
                    TestResult,
                    Notes,
                    CreatedByUserID);

            return null;
        }

        // ── Validation ───────────────────────────────
        private bool _Validate()
        {
            // Must be linked to valid appointment
            if (TestAppointmentID == -1)
                return false;

            if (Mode == enMode.Update && TestID == -1)
                return false;

            // Must have valid user
            if (CreatedByUserID == -1)
                return false;


            // Only check for AddNew ✅
            if (Mode == enMode.AddNew)
            {
                // Cannot add result if appointment not found
                ClsTestAppointment appointment = ClsTestAppointment.Find(TestAppointmentID);

                if (appointment == null)
                    return false;

                // Cannot add result if already locked
                if (Mode == enMode.AddNew && appointment.IsLocked)
                    return false;
            }

            return true;
        }

        // ── Private Methods ──────────────────────────
        private bool _AddNew()
        {

             this.TestID= ClsTestResultData.AddNew(
                this.TestAppointmentID,
                this.TestResult,
                this.Notes,
                this.CreatedByUserID);

            if (TestID == -1)
                return false;

            // Load once ✅
            ClsTestAppointment appointment = ClsTestAppointment.Find(this.TestAppointmentID);

            if (appointment != null)
                _LockAppointment(appointment);


            return true;
        }

        private bool _Update()
        {
            bool IsUpdated = ClsTestResultData.Update(
                this.TestID,
                this.TestResult,
                this.Notes,
                this.CreatedByUserID);

            // Load once ✅
            ClsTestAppointment appointment = ClsTestAppointment.Find(this.TestAppointmentID);

            if (appointment != null)
                _LockAppointment(appointment);

            return IsUpdated;
        }

        // Lock appointment after passing ✅
        // Accept as parameter — no extra DB call ✅
        private void _LockAppointment(ClsTestAppointment appointment)
        {
            // Fix: The appointment is locked regardless of pass or fail.
            // The test has been taken, so this appointment is finished.
            appointment.IsLocked = true;
            appointment.Save();
        }

        // ── Public Methods ───────────────────────────
        public static bool Delete(int TestID)
        {
            return ClsTestResultData.Delete(TestID);
        }

        // ✅  very useful
        public static ClsTestResult FindByAppointmentID(int TestAppointmentID)
        {
            int TestID = -1;
            bool TestResult = false;
            string Notes = "";
            int CreatedByUserID = -1;

            bool IsFound = ClsTestResultData.FindByAppointmentID(
                TestAppointmentID,
                ref TestID,
                ref TestResult,
                ref Notes,
                ref CreatedByUserID);

            if (IsFound)
                return new ClsTestResult(
                    TestID,
                    TestAppointmentID,
                    TestResult,
                    Notes,
                    CreatedByUserID);

            return null;
        }
        public bool Save()
        {
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
