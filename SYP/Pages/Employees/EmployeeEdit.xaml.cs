using SYP.Context;
using SYP.Models;
using System.Windows;
using System.Windows.Controls;

namespace SYP.Pages.Employees
{
    /// <summary>
    /// Логика взаимодействия для Employees.xaml
    /// </summary>
    public partial class EmployeeEdit : Page
    {
        Employees MainEmployees;
        Models.Employees employees;

        DepartmentContext departmentContext = new DepartmentContext();
        PositionContext positionContext = new PositionContext();
        EmployeeStatusContext statusContext = new EmployeeStatusContext();

        public EmployeeEdit(Employees MainEmployees, Models.Employees employees)
        {
            InitializeComponent();

            this.MainEmployees = MainEmployees;
            this.employees = employees;

            foreach (var item in positionContext.Positions)
                position.Items.Add(item.Name);

            foreach (var item in departmentContext.Departments)
                department.Items.Add(item.Name);

            foreach (var item in statusContext.EmployeeStatus)
                status.Items.Add(item.Name);

            if (employees != null)
            {
                lbTitle.Content = "Изменение существующего сотрудника";
                FIO.Text = employees.LastName + " " + employees.FirstName + " " + employees.Patronymic;
                dateBirth.Text = employees.BirthDate.ToString("dd.MM.yyyy");
                position.SelectedItem = positionContext.Positions.Where(x => x.Id == employees.PositionId).FirstOrDefault().Name;
                department.SelectedItem = departmentContext.Departments.Where(x => x.Id == employees.DepartmentId).FirstOrDefault().Name;
                dateHire.Text = employees.HireDate.ToString("dd.MM.yyyy");
                email.Text = employees.Email;
                phoneNumber.Text = employees.PhoneNumber;
                status.SelectedItem = statusContext.EmployeeStatus.Where(x => x.Id == employees.StatusId).FirstOrDefault().Name;
            }

            CheckValidation();
        }

        private void CheckValidation()
        {
            FIO.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(FIO, RegexValidator.IsFullNameValid(FIO.Text));

            email.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(email, RegexValidator.IsEmailValid(email.Text));

            phoneNumber.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(phoneNumber, RegexValidator.IsPhoneValid(phoneNumber.Text));

            position.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(position, position.SelectedItem != null);

            department.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(department, department.SelectedItem != null);

            status.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(status, status.SelectedItem != null);

            dateBirth.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(dateBirth, dateBirth.SelectedDate != null);

            dateHire.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(dateHire, dateHire.SelectedDate != null);
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Employees(null));
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            bool emailExists = MainEmployees.EmployeeContext.Employees.Any(e => e.Email == email.Text && (employees == null || e.Id != employees.Id));

            try
            {
                if (!RegexValidator.IsFullNameValid(FIO.Text))
                {
                    MessageBox.Show("Введите корректное ФИО (только буквы, минимум 5 символов).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!RegexValidator.IsEmailValid(email.Text))
                {
                    MessageBox.Show("Введите корректный Email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (emailExists)
                {
                    MessageBox.Show("Сотрудник с таким Email уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!RegexValidator.IsPhoneValid(phoneNumber.Text))
                {
                    MessageBox.Show("Введите корректный номер телефона (формат: +7XXXXXXXXXX).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dateBirth.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату рождения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dateHire.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату поступления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (position.SelectedItem == null)
                {
                    MessageBox.Show("Выберите должность.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (department.SelectedItem == null)
                {
                    MessageBox.Show("Выберите отдел.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (status.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string[] fullNameParts = FIO.Text.Trim().Split(' ');

                string lastName = fullNameParts.Length > 0 ? fullNameParts[0] : "";
                string firstName = fullNameParts.Length > 1 ? fullNameParts[1] : "";
                string patronymic = fullNameParts.Length > 2 ? fullNameParts[2] : "";

                DateTime birth = dateBirth.SelectedDate.Value;
                DateTime hire = dateHire.SelectedDate.Value;

                if (employees == null)
                {

                    employees = new Models.Employees()
                    {
                        LastName = lastName,
                        FirstName = firstName,
                        Patronymic = patronymic,
                        BirthDate = birth,
                        PositionId = positionContext.Positions.Where(x => x.Name == position.SelectedItem.ToString()).First().Id,
                        DepartmentId = departmentContext.Departments.Where(x => x.Name == department.SelectedItem.ToString()).First().Id,
                        HireDate = hire,
                        Email = email.Text,
                        PhoneNumber = phoneNumber.Text,
                        StatusId = statusContext.EmployeeStatus.Where(x => x.Name ==  status.SelectedItem.ToString()).First().Id
                    };
                    MainEmployees.EmployeeContext.Employees.Add(employees);
                }
                else
                {
                    employees.LastName = lastName;
                    employees.FirstName = firstName;
                    employees.Patronymic = patronymic;
                    employees.BirthDate = birth;
                    employees.PositionId = positionContext.Positions.Where(x => x.Name == position.SelectedItem.ToString()).First().Id;
                    employees.DepartmentId = departmentContext.Departments.Where(x => x.Name == department.SelectedItem.ToString()).First().Id;
                    employees.HireDate = hire;
                    employees.Email = email.Text;
                    employees.PhoneNumber = phoneNumber.Text;
                    employees.StatusId = statusContext.EmployeeStatus.Where(x => x.Name == status.SelectedItem.ToString()).First().Id;
                }
                MainEmployees.EmployeeContext.SaveChanges();
                MainWindow.mw.OpenPages(new Pages.Employees.Employees(null));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
