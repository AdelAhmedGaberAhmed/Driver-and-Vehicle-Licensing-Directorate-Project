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
    public enum enValidationResultPerson
    {
        Valid,
        EmptyFirstName,
        EmptySecondName,
        EmptyLastName,
        EmptyNationalNo,
        AgeTooYoung,
        EmptyPhone,
        DuplicateNationalNo,
        InvalidCountry
    }

    public enum enMode
    {
        AddNew,
        Update,
    }
    public class ClsPerson
    {

     
    
        public enMode Mode { get; private set; }  

        public string FullName
        {
            get
            {
                return $"{FirstName} {SecondName} {ThirdName} {LastName}";
            }
        }
        public int PersonID { get; set; }
        public string NationalNo { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }

        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public int NationalityCountryID { get; set; }
        public string ImagePath { get; set; }

        public ClsPerson(enMode mode, int personID, string nationalNo, string firstName, string secondName,
            string thirdName, string lastName, DateTime dateOfBirth, bool gender, string address,
            string phone, string email, int nationalCountryID, string imagePath)
        {
            this.Mode = mode;
            this.PersonID = personID;
            this.NationalNo = nationalNo;
            this.FirstName = firstName;
            this.SecondName = secondName;
            this.ThirdName = thirdName;
            this.LastName = lastName;
            this.DateOfBirth = dateOfBirth;
            this.Gender = gender;
            this.Address = address;
            this.Phone = phone;
            this.Email = email;
            this.NationalityCountryID = nationalCountryID;
            this.ImagePath = imagePath;
        }

        
        public ClsPerson()
        {
            this.PersonID             = -1;
            this.NationalityCountryID = -1;
            this.NationalNo           = "";
            this.FirstName            = "";
            this.SecondName           = "";
            this.ThirdName            = "";
            this.LastName             = "";
            this.Gender               = false;
            this.Address              = "";
            this.Phone                = "";
            this.Email                = "";
            this.ImagePath            = "";
            this.DateOfBirth          = DateTime.Now.AddYears(-18);
            this.Mode                 = enMode.AddNew;
         
        }

        private enValidationResultPerson _Validate()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                return enValidationResultPerson.EmptyFirstName;

            if (string.IsNullOrWhiteSpace(SecondName))
                return enValidationResultPerson.EmptySecondName;  

            if (string.IsNullOrWhiteSpace(NationalNo))
                return enValidationResultPerson.EmptyNationalNo;

           

            if (string.IsNullOrWhiteSpace(Phone))
                return enValidationResultPerson.EmptyPhone;

            ClsPerson person = ClsPerson.Find(NationalNo);
            if (person != null && person.PersonID != this.PersonID)
                return enValidationResultPerson.DuplicateNationalNo;

            if (NationalityCountryID == -1)
                return enValidationResultPerson.InvalidCountry;

            return enValidationResultPerson.Valid;
        }

        public static bool DeletePerson(int PersonID)
        {
            return (ClsPersonData.DeletePerson(PersonID));
        }

        public static DataTable GetAllPeople()
        {
            return ClsPersonData.GetAllPeople();
        }
        private bool _AddNewPerson()
        {
            this.PersonID = ClsPersonData.AddNewPerson(NationalNo, FirstName, SecondName, ThirdName, LastName,
                DateOfBirth, Gender, Address, Phone, Email, NationalityCountryID, ImagePath);

            return ( this.PersonID != -1 );
        }

        private bool _UpdatePerson()
        {
            return ClsPersonData.UpdatePerson(this.PersonID , this.NationalNo, this.FirstName, 
                this.SecondName, this.ThirdName, this.LastName,this.DateOfBirth, Gender,
                this.Address, this.Phone, this.Email, this.NationalityCountryID, this.ImagePath);
        }

        public static ClsPerson Find(int PersonID)
        {
            string NationalNo  = "", FirstName = "", SecondName = "",
                   ThirdName   = "", LastName  = "", Address    = "",
                   Phone       = "", Email     = "", ImagePath  = "";

                                DateTime DateOfBirth = DateTime.Now.AddYears(-18);
                                bool Gender = false;
                                int NationalityCountryID = -1;

            bool IsFound = ClsPersonData.Find(PersonID, ref NationalNo,
                           ref FirstName, ref SecondName, ref ThirdName,
                           ref LastName, ref DateOfBirth, ref Gender,
                           ref Address, ref Phone, ref Email,
                           ref NationalityCountryID, ref ImagePath);

            if (IsFound)
                return new ClsPerson(enMode.Update, PersonID, NationalNo,
                           FirstName, SecondName, ThirdName, LastName,
                           DateOfBirth, Gender, Address, Phone, Email,
                           NationalityCountryID, ImagePath);
            else
                return null;
        }

        public static ClsPerson Find(string NationalNo)
        {
            int PersonID = -1;
            string FirstName = "", SecondName = "",
                   ThirdName = "", LastName = "", Address = "",
                   Phone = "", Email = "", ImagePath = "";
            DateTime DateOfBirth = DateTime.Now.AddYears(-18);
            bool Gender = false;
            int NationalityCountryID = -1;

            bool IsFound = ClsPersonData.Find(NationalNo, ref PersonID,
                           ref FirstName, ref SecondName, ref ThirdName,
                           ref LastName, ref DateOfBirth, ref Gender,
                           ref Address, ref Phone, ref Email,
                           ref NationalityCountryID, ref ImagePath);

            if (IsFound)
                return new ClsPerson(enMode.Update, PersonID, NationalNo,
                           FirstName, SecondName, ThirdName, LastName,
                           DateOfBirth, Gender, Address, Phone, Email,
                           NationalityCountryID, ImagePath);
            else
                return null;
        }
        public  bool Save ()
        {

            if (_Validate() != enValidationResultPerson.Valid)
                return false;

            switch (Mode)
            {
                case enMode.AddNew:

                    if (_AddNewPerson())
                    {
                        Mode = enMode.Update;
                        return true;
                    }

                    else return false;

                 case enMode.Update:
                    return _UpdatePerson();
                    
            
            }
            return false;

        }
    }
}
