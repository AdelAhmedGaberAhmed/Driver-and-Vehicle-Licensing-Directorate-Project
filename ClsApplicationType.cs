using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
    

    public class ClsApplicationType
    {
        public string Title { get; set; }
        public int ID { get; private set; }
        public decimal ApplicationFees { get; set; }

        public enMode Mode { get;private set; }

        public ClsApplicationType(int applicationTypeID, string title, decimal fees)
        {
            this.ID = applicationTypeID;
            this.Title = title;
            this.ApplicationFees = fees;
            this.Mode = enMode.Update;
        }

        public ClsApplicationType()
        {
            this.ID = -1;
            this.Title = "";
            this.ApplicationFees = 0;
            this.Mode = enMode.AddNew;
        }
        public static DataTable GetAllApplicationTypes()
        {

            return ClsApplicationTypesData.GetAllApplicationTypes();
        }

        public static ClsApplicationType Find(int ApplicationTypeID)
        {
            string Title = "";
            decimal Fees = 0;

            bool IsFound = ClsApplicationTypesData.Find(ApplicationTypeID, ref Title, ref Fees);

            if (IsFound)
                return new ClsApplicationType(ApplicationTypeID, Title, Fees);
            else
                return null;
        }

        private bool _Validate()
        {
            // Title cannot be empty
            if (string.IsNullOrWhiteSpace(Title))
                return false;

            // Fees cannot be negative
            if (ApplicationFees < 0)
                return false;

            return true;
        }

        private bool _Update()
        {
            if (!_Validate())
                return false;

            return ClsApplicationTypesData.UpdateApplicationType( this.ID,  this.Title, this.ApplicationFees);
        }

        public bool Save()
        {
            if(Mode == enMode.Update)
            return _Update();
            return false;
        }
    }

}
