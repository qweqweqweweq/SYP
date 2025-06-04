using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Models
{
    public class EmployeeValidator
    {
        public static bool ValidateFullName(string fullName)
        {
            return RegexValidator.IsFullNameValid(fullName);
        }

        public static bool ValidateEmail(string email)
        {
            return RegexValidator.IsEmailValid(email);
        }

        public static bool ValidatePhone(string phone)
        {
            return RegexValidator.IsPhoneValid(phone);
        }

        public static bool ValidateDates(DateTime? birthDate, DateTime? hireDate)
        {
            return birthDate != null && hireDate != null && birthDate < hireDate;
        }

        public static bool ValidateSelection(object position, object department, object status)
        {
            return position != null && department != null && status != null;
        }
    }
}
