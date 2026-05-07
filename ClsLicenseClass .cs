using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
    public class ClsLicenseClass
    {
        // ── Properties ──────────────────────────────────
        public int LicenseClassID { get; private set; }
        public string ClassName { get; set; }
        public string ClassDescription { get; set; }
        public int MinimumAllowedAge { get; set; }
        public int DefaultValidityLength { get; set; }
        public decimal ClassFees { get; set; }

        // ── Constructors ─────────────────────────────────
        public ClsLicenseClass(int licenseClassID, string className, string classDescription, int minimumAllowedAge,
            int defaultValidityLength, decimal classFees)
        {
            this.LicenseClassID = licenseClassID;
            this.ClassName      = className;
            this.ClassDescription = classDescription;
            this.MinimumAllowedAge = minimumAllowedAge;
            this.DefaultValidityLength = defaultValidityLength;
            this.ClassFees = classFees;
        }

        public ClsLicenseClass()
        {
            this.LicenseClassID = -1;
            this.ClassName = "";
            this.ClassDescription = "";
            this.MinimumAllowedAge = 18;  // default minimum age
            this.DefaultValidityLength = 5;   // default 5 years
            this.ClassFees = 0;
        }

        // ── Validation ───────────────────────────────────
        private bool _Validate()
        {
            // Age must be at least 18
            if (MinimumAllowedAge < 18)
                return false;

            // Validity must be positive
            if (DefaultValidityLength <= 0)
                return false;

            // Fees cannot be negative
            if (ClassFees < 0)
                return false;

            return true;
        }

        // ── Static Methods ───────────────────────────────
        public static DataTable GetAllLicenseClasses()
        {
              return  ClsLicenseClassData.GetAllLicenseClasses();
            
        }

        // Find by ID
        public static ClsLicenseClass Find(int LicenseClassID)
        {
            string ClassName = "";
            string ClassDescription = "";
            int MinimumAllowedAge = 18;
            int DefaultValidityLength = 5;
            decimal ClassFees = 0;

            bool IsFound = ClsLicenseClassData.Find(LicenseClassID, ref ClassName, ref ClassDescription,
                ref MinimumAllowedAge, ref DefaultValidityLength,  ref ClassFees);

            if (IsFound)
                return new ClsLicenseClass(LicenseClassID, ClassName, ClassDescription, MinimumAllowedAge,
                           DefaultValidityLength, ClassFees);
            else
                return null;
        }

        // Find by ClassName
        public static ClsLicenseClass Find(string ClassName)
        {
            int LicenseClassID = -1;
            string ClassDescription = "";
            int MinimumAllowedAge = 18;
            int DefaultValidityLength = 5;
            decimal ClassFees = 0;

            bool IsFound = ClsLicenseClassData.Find(ClassName,
                           ref LicenseClassID, ref ClassDescription,
                           ref MinimumAllowedAge, ref DefaultValidityLength,
                           ref ClassFees);

            if (IsFound)
                return new ClsLicenseClass(LicenseClassID, ClassName,
                           ClassDescription, MinimumAllowedAge,
                           DefaultValidityLength, ClassFees);
            else
                return null;
        }

        // ── Instance Methods ─────────────────────────────
        private bool _Update()
        {
            if (!_Validate())
                return false;

            return ClsLicenseClassData.UpdateLicenseClass(
                this.LicenseClassID,
                this.MinimumAllowedAge,
                this.DefaultValidityLength,
                this.ClassFees
            );
        }

       

        public bool Save()
        {
            return _Update();
        }


    }

}

