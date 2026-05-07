using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DVLD.Business
{
    public enum enApplicationStatus
    {
        New = 1,
        Cancelled = 2,
        Completed = 3
    }

    public class ClsApplication
    {
        public int ApplicationID { get;  set; }                                // unique ID
        public DateTime ApplicationDate { get; set; }                         // when applied
        public DateTime LastStatusDate {  get; set; }
        public int PersonID { get;  set; }                                   // who applied
        public int ApplicationTypeID { get;  set; }                           // which service
                 
        public decimal PaidFees { get; set; }                              // fees paid
        public int CreatedByUserID { get;  set; }                             // which staff member

        // ClsApplication needs mode like all other classes
        public enMode Mode { get; private set; }


        // Should auto-update LastStatusDate when status changes ✅
        private enApplicationStatus _ApplicationStatus;
        public enApplicationStatus ApplicationStatus // New/Cancelled/Completed
        {
            get { return _ApplicationStatus; }
            set
            {
                _ApplicationStatus = value;
                LastStatusDate = DateTime.Now;
            }
        }


        public ClsApplication()
        {
            this.ApplicationID = -1;
            this.ApplicationDate = DateTime.Now;
            
            this.PersonID = -1;
            this.ApplicationTypeID = -1;

            // This automatically sets LastStatusDate = DateTime.Now ✅
            this.ApplicationStatus = enApplicationStatus.New;

            this.CreatedByUserID = -1;
            this.Mode = enMode.AddNew;
            
        }


        public ClsApplication(int  applicationID , DateTime ApplicationDate , DateTime LastStatusDate 
            , int personID , int applicationTypeID , enApplicationStatus ApplicationStatus, decimal PaidFees , int CreatedByUserID)
        {
            this.ApplicationID = applicationID;
            this.ApplicationDate = ApplicationDate;
            
            this.PersonID = personID;
            this.ApplicationTypeID = applicationTypeID;
            this.PaidFees = PaidFees;
            this.CreatedByUserID = CreatedByUserID;
            this.Mode = enMode.Update;
            this._ApplicationStatus = ApplicationStatus;
            this.LastStatusDate = LastStatusDate;
        }

        public static DataTable GetAllApplications()
        {
            return ClsApplicationData.GetAllApplications();
        }

        public static DataTable GetApplicationsForPerson(int PersonID)
        {
            return ClsApplicationData.GetApplicationsForPerson(PersonID);
        }


        public static ClsApplication Find(int ApplicationID )
        {           
                          DateTime ApplicationDate = DateTime.Now;
                          DateTime LastStatusDate = DateTime.Now;
                          int  PersonID = -1;
                          int ApplicationTypeID = -1;
                          int _ApplicationStatus = -1;
                          int CreatedByUserID = -1;
                          decimal PaidFees = 0;

            bool IsFound = ClsApplicationData.Find(ApplicationID ,ref PersonID ,ref ApplicationDate ,ref ApplicationTypeID ,
                ref _ApplicationStatus, ref LastStatusDate , ref PaidFees ,ref CreatedByUserID);


            if (IsFound )
            {
                return new ClsApplication(ApplicationID , ApplicationDate , LastStatusDate , PersonID ,
                                ApplicationTypeID, (enApplicationStatus)_ApplicationStatus, PaidFees , CreatedByUserID);
            }
            return null;

        }


        private bool _Validate()
        {
            // Person must be valid
            if (PersonID == -1)
                return false;

            // ApplicationType must be valid
            if (ApplicationTypeID == -1)
                return false;

            // CreatedByUser must be valid
            if (CreatedByUserID == -1)
                return false;

            // Fees cannot be negative
            if (PaidFees < 0)
                return false;

            // Person must not have active application of same type
            //  Business rules prevent duplicate active apps.
            if (Mode == enMode.AddNew)
            {
                if (ClsApplicationData.DoesPersonHaveActiveApplication(this.PersonID, this.ApplicationTypeID))
                    return false;
            }

            return true;
        }

        private bool _AddNewApplication()
        {
           this.ApplicationID = ClsApplicationData.AddNewApplication
                (
                       this.PersonID , 
                       this.ApplicationDate , 
                       this.ApplicationTypeID , 
                       (int)this.ApplicationStatus , 
                       this.LastStatusDate , this.PaidFees ,
                           this.CreatedByUserID  
                );

            return (this.ApplicationID != -1);
        }

        private bool _UpdateApplication()
        {
            return ClsApplicationData.UpdateApplication(
                this.ApplicationID,
                this.ApplicationDate,
                (int)this.ApplicationStatus,
                this.LastStatusDate,
                this.PaidFees
            );
        }

        public static bool DeleteApplication(int ApplicationID)
        {
            return ClsApplicationData.DeleteApplication(ApplicationID);
        }

        public bool Cancel()
        {
            if (Mode == enMode.AddNew)
                return false;

            if (this.ApplicationStatus == enApplicationStatus.Cancelled || this.ApplicationStatus == enApplicationStatus.Completed)
                return false;

            this.ApplicationStatus = enApplicationStatus.Cancelled;

            return _UpdateApplication();
        }


        // NO Reason For This Here in this Class 

        /* public static bool DoesPersonHaveActiveApplication(int PersonID, int ApplicationTypeID )
         {
             return ClsApplicationData.DoesPersonHaveActiveApplication( PersonID, ApplicationTypeID );
         }*/

        public bool Save()
        {
            if (!_Validate())
                return false;

            switch (Mode)
            {
                
                case enMode.AddNew:
                    if (_AddNewApplication())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else return false;

                case enMode.Update:
                    return _UpdateApplication();
            }
            return false;
        }

    }
}
