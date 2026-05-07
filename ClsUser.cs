using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVLD.DataAccess;

namespace DVLD.Business
{
    public enum enPermissions
    {
        // -1 means ALL permissions (admin)
        // Binary: 11111111...11111111
        // Any & check against -1 always returns true

        Admin = -1,

        None = 0,
        ViewPeople = 1,
        AddPerson = 2,
        UpdatePerson = 4,
        DeletePerson = 8,
        ViewLicenses = 16,
        AddLicense = 32,
        UpdateLicense = 64,
        ManageUsers = 128
    }

    public class ClsGlobal
    {
        public static ClsUser CurrentUser { get; set; }

        public static string AppTitle = "DVLD System";
        public static string AppVersion = "1.0.0";

        public static void SignOut()
        {
            CurrentUser = null;
        }

    }

    public class ClsUser
    {

        public enMode Mode { get; private set; }

        public int       UserID { get; set; }
        public int       PersonID { get; set; }
        public ClsPerson Person { get; set; }
        public string    UserName { get; set; }
        public string    Password { get; set; }
        public bool      IsActive { get; set; }
        public int       Permissions { get; set; }

        //  FullName shortcut property
        public string FullName
        {
            get { return Person?.FullName ?? ""; }
        }
        public ClsUser(enMode Mode , int UserID , ClsPerson Person , int PersonID , string UserName , string Password ,int  Permissions , bool IsActive)
        {
            this.UserID   = UserID;
            this.Person   = Person;
            this.UserName = UserName;
            this.Password = Password;
            
            this.Permissions = Permissions;
            this.PersonID  = PersonID;
            this.IsActive  = IsActive;
            this.Mode = Mode;
           
        }

        public ClsUser ()
        {
            this.UserID = -1;
            this.PersonID = -1;
            this.Person = null;
            this.UserName = "";
            this.Password = "";
            this.IsActive = false;
            this.Permissions = (int)enPermissions.None;
            this.Mode = enMode.AddNew;

        }

        private bool _Validate()
        {
            if (PersonID == -1) return false;
            if (string.IsNullOrWhiteSpace(UserName)) return false;
            if (string.IsNullOrWhiteSpace(Password)) return false;

            // Fix: Only check uniqueness if it's a new user OR the username changed
            ClsUser user = ClsUser.Find(UserName);
            if (user != null)
            {
                if (this.Mode == enMode.AddNew) return false; // Username exists
                if (this.Mode == enMode.Update && user.UserID != this.UserID) return false; // Taken by someone else
            }
            return true;

        }

        // Check if user has a specific permission
        public bool HasPermission(enPermissions permission)
        {
            // Admin has everything
            if (Permissions == (int)enPermissions.Admin)
                return true;

            return (Permissions & (int)permission) != 0;
        }

        // Add a permission
        public void AddPermission(enPermissions permission)
        {
            Permissions |= (int)permission;
        }

        // Remove a permission
        public void RemovePermission(enPermissions permission)
        {
            Permissions &= ~(int)permission;
        }

        // Check if username already taken 
        public static bool IsUserExists(int UserID)
        {
            return ClsUserData.IsUserExists(UserID);
        }

        public static bool IsUserExists(string Username)
        {
            return ClsUserData.IsUserExists(Username);
        }
        public static ClsUser Find(int UserID)
        {
            ClsPerson Person = null;

            string    UserName = "";
            string    Password = "";
            int       PersonID = -1;
            bool      IsActive = false;
            int       Permissions = (int)enPermissions.None;

            bool IsFound = false;

            IsFound = ClsUserData.Find(UserID,  ref PersonID, ref UserName, ref Password, ref Permissions, ref IsActive);

            if (IsFound)
            {
                Person = ClsPerson.Find(PersonID);

                return (new ClsUser( enMode.Update, UserID, Person, PersonID, UserName, Password, Permissions, IsActive));
            }
            else return null;

        }

        public static ClsUser Find(string UserName)
        {
            ClsPerson Person = null;

            int UserID = -1 ;
            string Password = "";
            int PersonID = -1;
            bool IsActive = false;
            int Permissions = (int)enPermissions.None;

            bool IsFound = false;

            IsFound = ClsUserData.Find(UserName, ref UserID,ref PersonID,ref Password,ref Permissions,ref IsActive);

            if (IsFound)
            {
                Person = ClsPerson.Find(PersonID);

                return (new ClsUser(enMode.Update, UserID, Person, PersonID, UserName, Password, Permissions, IsActive));
            }
            else return null;

        }

        public static DataTable GetAllUsers()
        {
            return ClsUserData.GetAllUsers();
        }

        public static bool DeleteUser(int UserID)
        {
            return ClsUserData.DeleteUser(UserID);
        }

        private bool _AddNewUser()
        {
            this.UserID = ClsUserData.AddNewUser
            (
              this.PersonID,
              this.UserName,
              this.Password,
              this.Permissions,
              this.IsActive
            );
            return (this.UserID != -1);
        }

        private bool _UpdateUser()
        {
            return ClsUserData.UpdateUser
            (
                this.UserID,
                this.PersonID,
                this.UserName,
                this.Password,
                this.Permissions,
                this.IsActive
            );
        }
        public bool Save()
        {
            if (!_Validate())
                return false;

            switch (Mode)
            {
                case enMode.AddNew:
                    if (ClsGlobal.CurrentUser == null || !ClsGlobal.CurrentUser.HasPermission(enPermissions.AddPerson))
                        return false;

                    if (_AddNewUser())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else return false;

                case enMode.Update:
                    if (!ClsGlobal.CurrentUser.HasPermission(enPermissions.UpdatePerson))
                        return false;

                    return _UpdateUser();
            }
            return false;
        }


        public bool ChangePassword(string CurrentPassword, string NewPassword)
        {
            bool IsChanged = false;


            if (this.Password != CurrentPassword)
            {
                return false;
            }

            if (NewPassword == CurrentPassword)
            {
                return false;
            }

            if(string.IsNullOrEmpty(NewPassword))
            {
                return false;
            }
            IsChanged = ClsUserData.ChangePassword(this.UserID, NewPassword);

            if (IsChanged)
            {
                this.Password = NewPassword;
            }
            return IsChanged; 
        }


        

    }
}
