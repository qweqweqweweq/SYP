using SYP.Context;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SYP.Pages.Employees
{
    /// <summary>
    /// Логика взаимодействия для EmployeeItem.xaml
    /// </summary>
    public partial class EmployeeItem : UserControl
    {
        Employees MainEmployees;
        Models.Employees Employee;
        PositionContext positionContext = new PositionContext();
        DepartmentContext departmentContext = new DepartmentContext();
        EmployeeStatusContext statusContext = new EmployeeStatusContext();
        VacationContext vacationContext = new VacationContext();
        private Models.Users currentUser;

        public EmployeeItem(Employees MainEmployees, Models.Employees Employee)
        {
            InitializeComponent();

            this.MainEmployees = MainEmployees;
            this.Employee = Employee;

            currentUser = MainWindow.mw.CurrentUser;
            if (currentUser != null && currentUser.Role == "Admin")
            {
                Edit.Visibility = Visibility.Visible;
                Delete.Visibility = Visibility.Visible;
            }

            lbFIO.Content = Employee.LastName + " " + Employee.FirstName + " " + Employee.Patronymic;
            lbPosition.Content = "Должность: " + positionContext.Positions.Where(x => x.Id == Employee.PositionId).FirstOrDefault().Name;
            lbDepartment.Content = "Отдел: " + departmentContext.Departments.Where(x => x.Id == Employee.DepartmentId).FirstOrDefault().Name;

            var status = statusContext.EmployeeStatus.FirstOrDefault(x => x.Id == Employee.StatusId);
            lbStatus.Content = status.Name;

            SetStatusColor(status.Name);

            var today = DateTime.Today;
            var currentVac = vacationContext.Vacations.FirstOrDefault(v => v.EmployeeId == Employee.Id && v.StartDate.Date <= today && v.EndDate.Date >= today);

            if (currentVac != null)
            {
                lbStatus.Content = "В отпуске";
                lbStatus.Foreground = new BrushConverter().ConvertFrom("#fc7e01") as Brush;
            }
            else
            {
                var lastVac = vacationContext.Vacations.Where(v => v.EmployeeId == Employee.Id).OrderByDescending(v => v.EndDate).FirstOrDefault();
                if (lastVac != null && today > lastVac.EndDate)
                {
                    lbStatus.Content = "Активен";
                    lbStatus.Foreground = new BrushConverter().ConvertFrom("#258117") as Brush;
                }
            }
        }

        private void SetStatusColor(string statusName)
        {
            switch (statusName)
            {
                case "Активен":
                    lbStatus.Foreground = new BrushConverter().ConvertFrom("#258117") as Brush;
                    break;
                case "В отпуске":
                    lbStatus.Foreground = new BrushConverter().ConvertFrom("#fc7e01") as Brush;
                    break;
                case "Уволен":
                    lbStatus.Foreground = new BrushConverter().ConvertFrom("#ac2517") as Brush;
                    break;
            }
        }

        private void ViewClick(object sender, MouseButtonEventArgs e)
        {
            var profileControl = MainEmployees.ProfileOverlay;
            profileControl.Visibility = Visibility.Visible;
            MainEmployees.OverlayBackground.Visibility = Visibility.Visible;
            profileControl.SetEmployee(Employee);
        }

        private void EditClick(object sender, MouseButtonEventArgs e) => MainWindow.mw.OpenPages(new Pages.Employees.EmployeeEdit(MainEmployees, Employee));

        private void DeleteClick(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить сотрудника?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new EmployeeContext())
                {
                    var employeeToDelete = context.Employees.FirstOrDefault(x => x.Id == Employee.Id);
                    if (employeeToDelete != null)
                    {
                        context.Employees.Remove(employeeToDelete);
                        context.SaveChanges();
                        MessageBox.Show("Сотрудник удалён");
                        (this.Parent as Panel).Children.Remove(this);
                    }
                }
            }
        }
    }
}
