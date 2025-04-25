using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Models
{
    public class Employees
    {
        [Key]
        public int Id { get; set; }
        public string LastName {  get; set; }
        public string FirstName { get; set; }
        public string Patronymic {  get; set; }
        public DateTime BirthDate {  get; set; }
        public int PositionId {  get; set; }
        public int DepartmentId {  get; set; }
        public DateTime HireDate { get; set; }
        public string Email {  get; set; }
        public string PhoneNumber { get; set; }
        public string Status {  get; set; }
    }
}
