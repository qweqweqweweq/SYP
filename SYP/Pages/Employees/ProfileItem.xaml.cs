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

        public Employees MainEmployees { get; set; }

        public ProfileItem()
        {
            InitializeComponent();
        }

        public void SetEmployee(Models.Employees employees)
        {
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

    }
}
