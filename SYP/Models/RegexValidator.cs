using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace SYP.Models
{
    public static class RegexValidator
    {
        public static bool IsFullNameValid(string fio) =>
        Regex.IsMatch(fio, @"^[А-Яа-яЁё\s\-]{5,}$");

        public static bool IsEmailValid(string email) =>
            Regex.IsMatch(email, @"^[\w\.-]+@[\w\.-]+\.\w{2,4}$");

        public static bool IsPhoneValid(string phone) =>
            Regex.IsMatch(phone, @"^\+7\d{10}$");

        public static void ValidateControl(TextBox textBox, bool isValid)
        {
            textBox.BorderBrush = isValid ? (Brush)new BrushConverter().ConvertFrom("#FF728EBB") : Brushes.Red;
        }
        public static void ValidateControl(ComboBox comboBox, bool isValid)
        {
            comboBox.BorderBrush = isValid ? (Brush)new BrushConverter().ConvertFrom("#FF728EBB") : Brushes.Red;
        }

        public static void ValidateControl(DatePicker datePicker, bool isValid)
        {
            datePicker.BorderBrush = isValid ? (Brush)new BrushConverter().ConvertFrom("#FF728EBB") : Brushes.Red;
        }
    }
}
