using SYP.Context;
using SYP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SYP.Pages.Employees
{
    /// <summary>
    /// Логика взаимодействия для ProfileItem.xaml
    /// </summary>
    public partial class ProfileItem : UserControl
    {
        DepartmentContext departmentContext = new DepartmentContext();
        PositionContext positionContext = new PositionContext();
        StatusContext statusContext = new StatusContext();
        UserContext userContext = new UserContext();

        Models.Employees employee;

        private Models.Users currentUser;

        public Employees MainEmployees { get; set; }

        public ProfileItem()
        {
            InitializeComponent();
        }

        public void SetEmployee(Models.Employees employees)
        {
            employee = employees;

            lbTitle.Content = "Профиль пользователя " + employees.FirstName;
            fio.Content = "ФИО: " + employees.LastName + " " + employees.FirstName + " " + employees.Patronymic;
            DateTime birthDate = employees.BirthDate;
            int age = DateTime.Today.Year - birthDate.Year;
            if (DateTime.Today < birthDate.AddYears(age)) age--;

            dateBirth.Content = $"Дата рождения: {birthDate:dd.MM.yyyy} ({age} {GetYearWord(age)})";

            department.Content = "Отдел: " + departmentContext.Departments.FirstOrDefault(x => x.Id == employees.DepartmentId).Name;
            position.Content = "Должность: " + positionContext.Positions.FirstOrDefault(x => x.Id == employees.PositionId).Name;
            dateHire.Content = "Дата поступления: " + employees.HireDate.ToString("dd.MM.yyyy");
            email.Content = "Эл. почта: " + employees.Email;
            phone.Content = "Номер телефона: " + employees.PhoneNumber;
            status.Content = "Статус: " + statusContext.Status.FirstOrDefault(x => x.Id == employees.StatusId).Name;

            currentUser = MainWindow.mw.CurrentUser;
            var user = userContext.Users.FirstOrDefault(u => u.EmployeeId == employees.Id);
            var statusName = statusContext.Status.FirstOrDefault(s => s.Id == employees.StatusId)?.Name;

            bool shouldShowButton = currentUser != null &&
                           currentUser.Role == "Admin" &&
                           user == null &&
                           statusName != "Уволен";

            newUser.Visibility = shouldShowButton ? Visibility.Visible : Visibility.Hidden;
        }

        private string GetYearWord(int age)
        {
            if (age % 100 >= 11 && age % 100 <= 14)
                return "лет";
            switch (age % 10)
            {
                case 1: return "год";
                case 2:
                case 3:
                case 4: return "года";
                default: return "лет";
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            MainEmployees.ProfileOverlay.Visibility = Visibility.Collapsed;
            MainEmployees.OverlayBackground.Visibility = Visibility.Collapsed;
        }

        private string GenerateUsername(Models.Employees employee)
        {
            string lastName = Transliterate(employee.LastName);
            string firstInitial = Transliterate(employee.FirstName.Substring(0, 1));
            string patronymicInitial = Transliterate(employee.Patronymic.Substring(0, 1));

            string baseUsername = $"{lastName}{firstInitial}{patronymicInitial}".ToLower();

            if (baseUsername.Length > 8)
                return baseUsername.Substring(0, 8);

            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            while (baseUsername.Length < 8)
            {
                baseUsername += chars[random.Next(chars.Length)];
            }

            return baseUsername;
        }

        private string GeneratePassword()
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        private static string Transliterate(string text)
        {
            var map = new Dictionary<char, string>
            {
                {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
                {'е', "e"}, {'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
                {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
                {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
                {'у', "u"}, {'ф', "f"}, {'х', "kh"}, {'ц', "ts"}, {'ч', "ch"},
                {'ш', "sh"}, {'щ', "shch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
                {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
            };

            var sb = new StringBuilder();
            foreach (var c in text.ToLower())
            {
                sb.Append(map.ContainsKey(c) ? map[c] : c.ToString());
            }

            return sb.ToString();
        }

        private void NewUserButton(object sender, RoutedEventArgs e)
        {
            string username = GenerateUsername(employee);
            string password = GeneratePassword();

            var newUsers = new Users
            {
                Username = username,
                Password = password,
                Role = "User",
                EmployeeId = employee.Id
            };

            userContext.Users.Add(newUsers);
            userContext.SaveChanges();

            MessageBox.Show($"Имя пользователя: {username}\nПароль: {password}", "Данные нового пользователя", MessageBoxButton.OK, MessageBoxImage.Information);

            newUser.Visibility = Visibility.Hidden;
        }
    }
}
