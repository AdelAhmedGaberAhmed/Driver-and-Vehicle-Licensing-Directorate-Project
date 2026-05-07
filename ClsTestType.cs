using DVLD.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLD.Business
{
    public class ClsTestType
    {
        public int ID { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Fees { get; set; }
        
        private bool _Validate()
        {
            // Title cannot be empty
            if (string.IsNullOrWhiteSpace(Title))
                return false;

            // Fees cannot be negative
            if (Fees < 0)
                return false;

            return true;
        }
        public ClsTestType (int id , string title , string description , decimal fees )
        {
            this.ID = id;
            this.Title = title;
            this.Description = description;
            this.Fees = fees;
        }
        public ClsTestType()
        {
            this.ID = -1;
            this.Title = "";
            this.Description = "";
            this.Fees = 0 ;
        }



        public static DataTable GetAllTypes()
        {
            return ClsTestTypesData.GetAllTypes();

        }
        private   bool _Update()
        {
            if (!_Validate()) return false;

            return ClsTestTypesData.Update(this.ID, this.Title, this.Description , this.Fees);
            
        }

        public bool Save()
        {
            return _Update();
        }
        public static ClsTestType Find(int ID)
        {
            string Title = "";
            string Description = "";
            decimal Fees = 0;

            bool IsFound = ClsTestTypesData.Find(ID, ref Title,ref Description, ref Fees);

            if (IsFound)
                return new ClsTestType(ID, Title, Description, Fees);
            else
                return null;
        }

    }
}
